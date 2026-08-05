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
using By3.Repository.Repositories;

namespace By3.Repository;

public static class DependencyInjection
{
    public static IServiceCollection AddBy3Repositories(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("DefaultConnection 未配置，请在环境变量或 User Secrets 中设置。");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<UserRepository>();
        services.AddScoped<RoleRepository>();
        services.AddScoped<MenuRepository>();
        services.AddScoped<AuditLogRepository>();
        services.AddScoped<LoginLogRepository>();
        services.AddScoped<DepartmentRepository>();
        services.AddScoped<PositionRepository>();
        services.AddScoped<DictTypeRepository>();
        services.AddScoped<DictDataRepository>();
        services.AddScoped<FileRecordRepository>();
        services.AddScoped<EmailTemplateRepository>();
        services.AddScoped<EmailTemplateVersionRepository>();
        services.AddScoped<EmailLogRepository>();
        services.AddScoped<EmailSettingRepository>();
        services.AddScoped<JobRepository>();
        services.AddScoped<JobLogRepository>();
        services.AddScoped<ExternalApiTokenRepository>();
        services.AddScoped<ExternalApiTokenHistoryRepository>();
        services.AddScoped<ExternalApiTokenLogRepository>();
        services.AddScoped<ExternalApiAccessLogRepository>();
        services.AddScoped<ExternalApiRepository>();

        return services;
    }
}
