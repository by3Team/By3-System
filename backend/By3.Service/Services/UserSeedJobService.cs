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

using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using By3.Repository.Entities;
using By3.Repository.Repositories;
using By3.Service.DTOs;

namespace By3.Service.Services;

public class UserSeedJobService
{
    private readonly IConfiguration _config;
    private readonly UserRepository _userRepo;
    private readonly DepartmentRepository _deptRepo;
    private readonly PositionRepository _positionRepo;

    public UserSeedJobService(IConfiguration config, UserRepository userRepo, DepartmentRepository deptRepo, PositionRepository positionRepo)
    {
        _config = config;
        _userRepo = userRepo;
        _deptRepo = deptRepo;
        _positionRepo = positionRepo;
    }

    /// <summary>
    /// 执行用户数据种子任务：批量生成模拟用户并备份现有数据。
    /// </summary>
    public async Task<JobExecutionResult> ExecuteAsync(Guid jobId, string jobName, string configJson, CancellationToken cancellationToken = default)
    {
        var config = ParseConfig(configJson);
        var backupDir = ResolveBackupDirectory(config.BackupDirectory);
        Directory.CreateDirectory(backupDir);

        var departments = await _deptRepo.GetAllAsync();
        var positions = await _positionRepo.GetListAsync(1, int.MaxValue, null);

        var existingUsers = await _userRepo.GetRecentListAsync(null);

        var backupFileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_users_backup.csv";
        var backupFilePath = Path.Combine(backupDir, backupFileName);

        var defaultPassword = _config["Jobs:UserSeed:DefaultPassword"]
            ?? throw new InvalidOperationException("Jobs:UserSeed:DefaultPassword 未配置。");
        var newUsers = GenerateMockUsers(config.BatchSize, departments, positions, defaultPassword);

        await _userRepo.InsertRangeWithTransactionAsync(newUsers, async () =>
        {
            await WriteUsersToCsvAsync(existingUsers, backupFilePath, cancellationToken);
        });

        var cleanedCount = CleanupOldBackups(backupDir, config.KeepBackupCount);

        return new JobExecutionResult
        {
            InsertedCount = newUsers.Count,
            BackupFilePath = backupFilePath,
            CleanedUpCount = cleanedCount
        };
    }

    /// <summary>
    /// 解析任务配置 JSON，失败则返回默认配置。
    /// </summary>
    private static UserSeedJobConfig ParseConfig(string configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
            return new UserSeedJobConfig();

        try
        {
            return JsonSerializer.Deserialize<UserSeedJobConfig>(configJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new UserSeedJobConfig();
        }
        catch
        {
            return new UserSeedJobConfig();
        }
    }

    /// <summary>
    /// 解析备份目录路径（相对路径基于应用根目录）。
    /// </summary>
    private static string ResolveBackupDirectory(string configDirectory)
    {
        if (string.IsNullOrWhiteSpace(configDirectory))
            configDirectory = "./backups/users";

        if (Path.IsPathRooted(configDirectory))
            return configDirectory;

        var baseDir = AppContext.BaseDirectory;
        return Path.GetFullPath(Path.Combine(baseDir, configDirectory));
    }

    /// <summary>
    /// 生成指定数量的模拟用户数据。
    /// </summary>
    private static List<SysUser> GenerateMockUsers(int batchSize, List<SysDepartment> departments, List<SysPosition> positions, string defaultPassword)
    {
        var users = new List<SysUser>(batchSize);
        var surnames = new[] { "赵", "钱", "孙", "李", "周", "吴", "郑", "王", "冯", "陈", "褚", "卫", "蒋", "沈", "韩", "杨" };
        var names = new[] { "伟", "芳", "娜", "敏", "静", "丽", "强", "磊", "洋", "勇", "军", "杰", "娟", "艳", "涛", "明" };
        var genders = new[] { "male", "female" };
        var random = new Random();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        for (int i = 0; i < batchSize; i++)
        {
            var index = timestamp + i;
            var surname = surnames[random.Next(surnames.Length)];
            var name = names[random.Next(names.Length)];
            var realName = $"{surname}{name}";
            var userName = $"demo_user_{index}";

            users.Add(new SysUser
            {
                Id = Guid.NewGuid(),
                UserName = userName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(defaultPassword),
                RealName = realName,
                Email = $"{userName}@by3.demo",
                Phone = $"13{random.Next(100000000, 999999999)}",
                Gender = genders[random.Next(genders.Length)],
                DepartmentId = departments.Count > 0 ? departments[random.Next(departments.Count)].Id : null,
                PositionId = positions.Count > 0 ? positions[random.Next(positions.Count)].Id : null,
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        return users;
    }

    /// <summary>
    /// 将用户列表导出为 CSV 备份文件。
    /// </summary>
    private static async Task WriteUsersToCsvAsync(List<SysUser> users, string filePath, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id,UserName,Email,Phone,RealName,Gender,DepartmentId,PositionId,IsEnabled,CreatedAt");

        foreach (var u in users)
        {
            sb.AppendLine($"{u.Id},{Escape(u.UserName)},{Escape(u.Email)},{Escape(u.Phone)},{Escape(u.RealName)},{Escape(u.Gender)},{u.DepartmentId},{u.PositionId},{u.IsEnabled},{u.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        }

        await File.WriteAllTextAsync(filePath, sb.ToString(), new UTF8Encoding(true), cancellationToken);
    }

    /// <summary>
    /// CSV 字段转义（处理逗号、引号、换行）。
    /// </summary>
    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        var escaped = value.Replace("\"", "\"\"");
        if (escaped.Contains(',') || escaped.Contains('\n') || escaped.Contains('\r'))
            escaped = $"\"{escaped}\"";
        return escaped;
    }

    /// <summary>
    /// 清理旧备份文件，保留指定数量的最新备份。
    /// </summary>
    private static int CleanupOldBackups(string backupDir, int keepCount)
    {
        if (!Directory.Exists(backupDir)) return 0;

        var files = Directory.GetFiles(backupDir, "*_users_backup.csv")
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.CreationTime)
            .ToList();

        var toDelete = files.Skip(keepCount).ToList();
        foreach (var file in toDelete)
        {
            try
            {
                file.Delete();
            }
            catch
            {
                // ignore cleanup errors
            }
        }

        return toDelete.Count;
    }
}
