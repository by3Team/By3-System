// Copyright 2026 By3 Team
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using By3.Repository;
using By3.Repository.Data;
using By3.Service.Services;

namespace By3.Service;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceCollection services)
    {
        // 构建临时服务提供程序，仅用于数据库初始化。
        // 最终的服务提供程序由 builder.Build() 构建，此时数据库和种子数据已就绪。
        await using var tempProvider = services.BuildServiceProvider();
        await InitializeAsync((IServiceProvider)tempProvider);
    }

    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(DatabaseInitializer));

        logger.LogInformation("开始执行数据库初始化...");

        var autoMigrate = configuration.GetValue<bool>("Database:AutoMigrate");
        var autoSeed = configuration.GetValue<bool>("Database:AutoSeed");

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // 先确保数据库本身存在（连接字符串中指定的库），再建表/迁移
        await EnsureDatabaseExistsAsync(configuration, logger);

        if (autoMigrate)
        {
            logger.LogInformation("Database:AutoMigrate 为 true，开始执行 EF Core 迁移...");
            await db.Database.MigrateAsync();
        }
        else
        {
            // 自动创建表结构（幂等：已存在则跳过）
            await EnsureTablesCreatedAsync(db, logger);
        }

        if (autoSeed)
        {
            logger.LogInformation("Database:AutoSeed 为 true，开始初始化种子数据...");
            var defaultPassword = configuration["Jobs:UserSeed:DefaultPassword"];
            await db.EnsureSeedDataAsync(defaultPassword);

            var userService = scope.ServiceProvider.GetRequiredService<UserService>();
            var migratedCount = await userService.MigratePlaintextPhonesAsync();
            if (migratedCount > 0)
            {
                logger.LogInformation("已迁移 {Count} 条明文手机号记录为加密存储。", migratedCount);
            }
        }
        else
        {
            logger.LogInformation("Database:AutoSeed 为 false，跳过种子数据初始化。");
        }

        logger.LogInformation("数据库初始化完成。");
    }

    /// <summary>
    /// 在连接目标库之前，先确保目标数据库存在。
    /// 如果目标库不存在，则通过连接 PostgreSQL 的系统库（postgres/template1）来创建它。
    /// </summary>
    private static async Task EnsureDatabaseExistsAsync(IConfiguration configuration, ILogger logger)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("DefaultConnection 未配置，无法检查/创建数据库。");
        }

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var targetDatabase = builder.Database;
        if (string.IsNullOrWhiteSpace(targetDatabase))
        {
            throw new InvalidOperationException("连接字符串中未指定 Database，无法自动创建数据库。");
        }

        // 先尝试直接连接目标库，若成功说明已存在
        try
        {
            await using var checkConnection = new NpgsqlConnection(connectionString);
            await checkConnection.OpenAsync();
            logger.LogInformation("数据库 {Database} 已存在。", targetDatabase);
            return;
        }
        catch (PostgresException ex) when (ex.SqlState == "3D000")
        {
            // 3D000: database does not exist，继续创建
            logger.LogInformation("数据库 {Database} 不存在，准备自动创建。", targetDatabase);
        }
        catch (NpgsqlException ex)
        {
            logger.LogWarning(ex, "检查数据库 {Database} 是否存在时发生非预期错误，将尝试创建。", targetDatabase);
        }

        // 使用系统库创建目标库。优先尝试 postgres，再尝试 template1
        var systemDatabases = new[] { "postgres", "template1" };
        foreach (var systemDb in systemDatabases)
        {
            builder.Database = systemDb;
            var systemConnectionString = builder.ConnectionString;
            logger.LogInformation("尝试连接到系统库 {SystemDb} 以创建目标库。", systemDb);

            try
            {
                await using var connection = new NpgsqlConnection(systemConnectionString);
                await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = $"CREATE DATABASE \"{targetDatabase}\";";
                await command.ExecuteNonQueryAsync();

                logger.LogInformation("数据库 {Database} 已自动创建。", targetDatabase);

                // 清理连接池，避免后续连接使用建库前的失效连接
                NpgsqlConnection.ClearPool(new NpgsqlConnection(connectionString));
                return;
            }
            catch (PostgresException ex) when (ex.SqlState == "42P04")
            {
                // 42P04: database already exists（并发创建时可能出现）
                logger.LogInformation("数据库 {Database} 已存在（并发创建），无需重复创建。", targetDatabase);
                return;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "通过系统库 {SystemDb} 创建数据库失败：{Message}，尝试下一个系统库。", systemDb, ex.Message);
            }
        }

        throw new InvalidOperationException($"无法自动创建数据库 {targetDatabase}，请检查 PostgreSQL 服务、用户名密码及创建数据库权限。");
    }

    /// <summary>
    /// 使用 EF Core 的 RelationalDatabaseCreator 创建表结构。
    /// 数据库必须已存在；若表已存在则跳过。
    /// </summary>
    private static async Task EnsureTablesCreatedAsync(AppDbContext db, ILogger logger)
    {
        var creator = db.Database.GetService<IRelationalDatabaseCreator>();

        // 检查是否已有表；没有则创建。避免 EnsureCreatedAsync 内部再次检查数据库是否存在时抛 3D000。
        var hasTables = await creator.HasTablesAsync();
        if (hasTables)
        {
            logger.LogInformation("数据库表结构已存在，跳过建表。");
            return;
        }

        await creator.CreateTablesAsync();
        logger.LogInformation("数据库表结构已自动创建。");
    }
}
