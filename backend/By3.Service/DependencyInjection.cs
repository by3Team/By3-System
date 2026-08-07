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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using By3.Service.Jobs;
using By3.Service.Services;

namespace By3.Service;

public static class DependencyInjection
{
    public static IServiceCollection AddBy3Services(this IServiceCollection services)
    {
        services.AddScoped<DataProtectionService>();
        services.AddScoped<AuthService>();
        services.AddScoped<UserService>();
        services.AddScoped<RoleService>();
        services.AddScoped<MenuService>();
        services.AddScoped<AuditLogService>();
        services.AddScoped<LoginLogService>();
        services.AddScoped<DepartmentService>();
        services.AddScoped<PositionService>();
        services.AddScoped<DictTypeService>();
        services.AddScoped<DictDataService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<EmailService>();
        services.AddScoped<EmailSettingService>();
        services.AddScoped<SystemInfoService>();
        services.AddScoped<JobService>();
        services.AddScoped<UserSeedJobService>();
        services.AddScoped<ExternalApiTokenService>();
        services.AddScoped<ExternalApiService>();
        services.AddScoped<JobSchedulerService>();

        services.AddTransient<UserDataSeedQuartzJob>();
        services.AddSingleton<IJobFactory, QuartzJobFactory>();
        services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

        // 数据库初始化 HostedService 必须早于 QuartzSchedulerHostedService 注册，确保先建库建表
        services.AddSingleton<DatabaseInitializerHostedService>();
        services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<DatabaseInitializerHostedService>());

        services.AddSingleton<QuartzSchedulerHostedService>();
        services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<QuartzSchedulerHostedService>());

        return services;
    }
}
