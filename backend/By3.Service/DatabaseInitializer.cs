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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using By3.Repository;
using By3.Repository.Data;
using By3.Service.Services;

namespace By3.Service;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(DatabaseInitializer));

        var autoMigrate = configuration.GetValue<bool>("Database:AutoMigrate");
        var autoSeed = configuration.GetValue<bool>("Database:AutoSeed");

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (autoMigrate)
        {
            logger.LogInformation("Database:AutoMigrate 为 true，开始执行 EF Core 迁移...");
            await db.Database.MigrateAsync();
        }
        else
        {
            logger.LogInformation("Database:AutoMigrate 为 false，跳过自动迁移。");
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
    }
}
