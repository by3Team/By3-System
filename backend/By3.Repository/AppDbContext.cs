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
using By3.Repository.Entities;

namespace By3.Repository;

public class AppDbContext : DbContext
{
    private readonly string _tablePrefix;

    public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration) : base(options)
    {
        _tablePrefix = configuration["TablePrefix"]?.Trim() ?? "by3_";
        if (string.IsNullOrEmpty(_tablePrefix))
            _tablePrefix = "by3_";
    }

    /// <summary>
    /// 获取带前缀的表名。
    /// </summary>
    /// <param name="name">实际表名，例如 sysuser。</param>
    /// <returns>前缀 + 表名，例如 by3_sysuser。</returns>
    public string TableName(string name) => $"{_tablePrefix}{name}";

    public DbSet<SysUser> Users => Set<SysUser>();
    public DbSet<SysRole> Roles => Set<SysRole>();
    public DbSet<SysMenu> Menus => Set<SysMenu>();
    public DbSet<SysUserRole> UserRoles => Set<SysUserRole>();
    public DbSet<SysRoleMenu> RoleMenus => Set<SysRoleMenu>();
    public DbSet<SysAuditLog> AuditLogs => Set<SysAuditLog>();
    public DbSet<SysLoginLog> LoginLogs => Set<SysLoginLog>();
    public DbSet<SysDepartment> Departments => Set<SysDepartment>();
    public DbSet<SysPosition> Positions => Set<SysPosition>();
    public DbSet<SysDictType> DictTypes => Set<SysDictType>();
    public DbSet<SysDictData> DictData => Set<SysDictData>();
    public DbSet<SysFileRecord> FileRecords => Set<SysFileRecord>();
    public DbSet<SysEmailTemplate> EmailTemplates => Set<SysEmailTemplate>();
    public DbSet<SysEmailTemplateVersion> EmailTemplateVersions => Set<SysEmailTemplateVersion>();
    public DbSet<SysEmailLog> EmailLogs => Set<SysEmailLog>();
    public DbSet<SysEmailSetting> EmailSettings => Set<SysEmailSetting>();
    public DbSet<SysJob> Jobs => Set<SysJob>();
    public DbSet<SysJobLog> JobLogs => Set<SysJobLog>();
    public DbSet<SysExternalApiToken> ExternalApiTokens => Set<SysExternalApiToken>();
    public DbSet<SysExternalApiTokenLog> ExternalApiTokenLogs => Set<SysExternalApiTokenLog>();
    public DbSet<SysExternalApiTokenHistory> ExternalApiTokenHistories => Set<SysExternalApiTokenHistory>();
    public DbSet<SysExternalApiAccessLog> ExternalApiAccessLogs => Set<SysExternalApiAccessLog>();
    public DbSet<SysExternalApi> ExternalApis => Set<SysExternalApi>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureSysUser(modelBuilder);
        ConfigureSysRole(modelBuilder);
        ConfigureSysMenu(modelBuilder);
        ConfigureSysUserRole(modelBuilder);
        ConfigureSysRoleMenu(modelBuilder);
        ConfigureSysAuditLog(modelBuilder);
        ConfigureSysLoginLog(modelBuilder);
        ConfigureSysDepartment(modelBuilder);
        ConfigureSysPosition(modelBuilder);
        ConfigureSysDictType(modelBuilder);
        ConfigureSysDictData(modelBuilder);
        ConfigureSysFileRecord(modelBuilder);
        ConfigureSysEmailTemplate(modelBuilder);
        ConfigureSysEmailTemplateVersion(modelBuilder);
        ConfigureSysEmailLog(modelBuilder);
        ConfigureSysEmailSetting(modelBuilder);
        ConfigureSysJob(modelBuilder);
        ConfigureSysJobLog(modelBuilder);
        ConfigureSysExternalApiToken(modelBuilder);
        ConfigureSysExternalApiTokenLog(modelBuilder);
        ConfigureSysExternalApiTokenHistory(modelBuilder);
        ConfigureSysExternalApiAccessLog(modelBuilder);
        ConfigureSysExternalApi(modelBuilder);
    }

    private void ConfigureSysUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SysUser>(entity =>
        {
            entity.ToTable(TableName("sysuser"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(200).IsRequired(false);
            entity.Property(e => e.Phone).HasMaxLength(50).IsRequired(false);
            entity.Property(e => e.RealName).HasMaxLength(100).IsRequired(false);
            entity.Property(e => e.IsEnabled).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired(false);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    private void ConfigureSysRole(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SysRole>(entity =>
        {
            entity.ToTable(TableName("sysrole"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RoleName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500).IsRequired(false);
            entity.Property(e => e.IsEnabled).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired(false);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    private void ConfigureSysMenu(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SysMenu>(entity =>
        {
            entity.ToTable(TableName("sysmenu"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MenuName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Permission).HasMaxLength(100).IsRequired(false);
            entity.Property(e => e.Route).HasMaxLength(200).IsRequired(false);
            entity.Property(e => e.Icon).HasMaxLength(50).IsRequired(false);
            entity.Property(e => e.Component).HasMaxLength(200).IsRequired(false);
            entity.Property(e => e.MenuType).IsRequired();
            entity.Property(e => e.SortOrder).IsRequired();
            entity.Property(e => e.ParentId).IsRequired(false);
            entity.Property(e => e.IsEnabled).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired(false);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    private void ConfigureSysUserRole(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SysUserRole>(entity =>
        {
            entity.ToTable(TableName("sysuserrole"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.RoleId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => new { e.UserId, e.RoleId });
        });
    }

    private void ConfigureSysRoleMenu(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SysRoleMenu>(entity =>
        {
            entity.ToTable(TableName("sysrolemenu"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RoleId).IsRequired();
            entity.Property(e => e.MenuId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => new { e.RoleId, e.MenuId });
        });
    }

    private void ConfigureSysAuditLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SysAuditLog>(entity =>
        {
            entity.ToTable(TableName("sysauditlog"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired(false);
            entity.Property(e => e.UserName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Action).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Controller).HasMaxLength(200).IsRequired(false);
            entity.Property(e => e.RequestPath).HasMaxLength(500).IsRequired(false);
            entity.Property(e => e.RequestMethod).HasMaxLength(20).IsRequired(false);
            entity.Property(e => e.RequestParams).HasColumnType("text").IsRequired(false);
            entity.Property(e => e.RequestBody).HasColumnType("text").IsRequired(false);
            entity.Property(e => e.RequestHeaders).HasColumnType("text").IsRequired(false);
            entity.Property(e => e.ResponseResult).HasColumnType("text").IsRequired(false);
            entity.Property(e => e.ResponseHeaders).HasColumnType("text").IsRequired(false);
            entity.Property(e => e.StatusCode).IsRequired(false);
            entity.Property(e => e.ExceptionMessage).HasColumnType("text").IsRequired(false);
            entity.Property(e => e.ElapsedMs).IsRequired();
            entity.Property(e => e.IpAddress).HasMaxLength(100).IsRequired(false);
            entity.Property(e => e.UserAgent).HasMaxLength(500).IsRequired(false);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.CreatedAt);
        });
    }

    private void ConfigureSysLoginLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SysLoginLog>(entity =>
        {
            entity.ToTable(TableName("sysloginlog"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired(false);
            entity.Property(e => e.UserName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.IsSuccess).IsRequired();
            entity.Property(e => e.Message).HasMaxLength(500).IsRequired(false);
            entity.Property(e => e.IpAddress).HasMaxLength(100).IsRequired(false);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.CreatedAt);
        });
    }

    private void ConfigureSysDepartment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SysDepartment>(entity =>
        {
            entity.ToTable(TableName("sysdepartment"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DeptName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DeptCode).HasMaxLength(100).IsRequired(false);
            entity.Property(e => e.ParentId).IsRequired(false);
            entity.Property(e => e.SortOrder).IsRequired();
            entity.Property(e => e.IsEnabled).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired(false);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.ParentId);
        });
    }

    private void ConfigureSysPosition(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SysPosition>(entity =>
        {
            entity.ToTable(TableName("sysposition"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PositionName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PositionCode).HasMaxLength(100).IsRequired(false);
            entity.Property(e => e.SortOrder).IsRequired();
            entity.Property(e => e.IsEnabled).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired(false);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    private void ConfigureSysDictType(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SysDictType>(entity =>
        {
            entity.ToTable(TableName("sysdicttype"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DictName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DictType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.IsEnabled).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired(false);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.DictType).IsUnique();
        });
    }

    private void ConfigureSysDictData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SysDictData>(entity =>
        {
            entity.ToTable(TableName("sysdictdata"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DictTypeId).IsRequired();
            entity.Property(e => e.DictLabel).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DictValue).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Remark).HasMaxLength(500).IsRequired(false);
            entity.Property(e => e.SortOrder).IsRequired();
            entity.Property(e => e.IsDefault).HasDefaultValue(false);
            entity.Property(e => e.IsEnabled).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired(false);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.DictTypeId);
        });
    }

    private void ConfigureSysFileRecord(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SysFileRecord>(entity =>
        {
            entity.ToTable(TableName("sysfilerecord"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.OriginalFileName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.StoragePath).HasMaxLength(500).IsRequired();
            entity.Property(e => e.ContentType).HasMaxLength(100).IsRequired(false);
            entity.Property(e => e.FileCategory).HasMaxLength(50).IsRequired();
            entity.Property(e => e.UploadMode).HasMaxLength(20).IsRequired();
            entity.Property(e => e.IsEnabled).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired(false);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.CreatedAt);
        });
    }

    private void ConfigureSysEmailTemplate(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SysEmailTemplate>(entity =>
        {
            entity.ToTable(TableName("sysemailtemplate"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TemplateCode).HasMaxLength(100).IsRequired();
            entity.Property(e => e.TemplateName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500).IsRequired(false);
            entity.Property(e => e.IsEnabled).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired(false);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.TemplateCode).IsUnique();
        });
    }

    private void ConfigureSysEmailTemplateVersion(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SysEmailTemplateVersion>(entity =>
        {
            entity.ToTable(TableName("sysemailtemplateversion"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TemplateId).IsRequired();
            entity.Property(e => e.Version).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Subject).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Body).HasColumnType("text").IsRequired();
            entity.Property(e => e.BodyFormat).HasMaxLength(20).IsRequired();
            entity.Property(e => e.IsEnabled).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired(false);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => new { e.TemplateId, e.Version }).IsUnique();
        });
    }

    private void ConfigureSysEmailLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SysEmailLog>(entity =>
        {
            entity.ToTable(TableName("sysemaillog"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TemplateId).IsRequired(false);
            entity.Property(e => e.TemplateVersionId).IsRequired(false);
            entity.Property(e => e.ToAddresses).HasColumnType("text").IsRequired();
            entity.Property(e => e.CcAddresses).HasColumnType("text").IsRequired(false);
            entity.Property(e => e.Subject).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Body).HasColumnType("text").IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            entity.Property(e => e.ErrorMessage).HasColumnType("text").IsRequired(false);
            entity.Property(e => e.SentAt).IsRequired(false);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.Status);
        });
    }

    private void ConfigureSysEmailSetting(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SysEmailSetting>(entity =>
        {
            entity.ToTable(TableName("sysemailsetting"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SmtpHost).HasMaxLength(200).IsRequired();
            entity.Property(e => e.SmtpPort).IsRequired();
            entity.Property(e => e.Username).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Password).HasMaxLength(500).IsRequired();
            entity.Property(e => e.FromName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.FromAddress).HasMaxLength(200).IsRequired();
            entity.Property(e => e.EnableSsl).IsRequired();
            entity.Property(e => e.IsEnabled).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired(false);
        });
    }

    private void ConfigureSysJob(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SysJob>(entity =>
        {
            entity.ToTable(TableName("sysjob"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.JobName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.JobGroup).HasMaxLength(100).IsRequired();
            entity.Property(e => e.JobType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.CronExpression).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500).IsRequired(false);
            entity.Property(e => e.ConfigJson).HasColumnType("text").IsRequired(false);
            entity.Property(e => e.IsEnabled).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired(false);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    private void ConfigureSysJobLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SysJobLog>(entity =>
        {
            entity.ToTable(TableName("sysjoblog"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.JobId).IsRequired();
            entity.Property(e => e.JobName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Result).HasMaxLength(500).IsRequired(false);
            entity.Property(e => e.ExceptionMessage).HasColumnType("text").IsRequired(false);
            entity.Property(e => e.FireTime).IsRequired();
            entity.Property(e => e.EndTime).IsRequired(false);
            entity.Property(e => e.NextFireTime).IsRequired(false);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.JobId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.Status);
        });
    }

    private void ConfigureSysExternalApiToken(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SysExternalApiToken>(entity =>
        {
            entity.ToTable(TableName("sysexternalapitoken"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AppName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ApiKey).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ApiSecret).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500).IsRequired(false);
            entity.Property(e => e.ExpireTime).IsRequired(false);
            entity.Property(e => e.ExpireType).HasMaxLength(20).IsRequired().HasDefaultValue("30");
            entity.Property(e => e.AllowedApiIds).HasColumnType("text").IsRequired(false);
            entity.Property(e => e.ContactEmail).HasMaxLength(500).IsRequired(false);
            entity.Property(e => e.IsEnabled).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.PreviousValidUntil).IsRequired(false);
            entity.Property(e => e.PreviousApiKey).HasMaxLength(100).IsRequired(false);
            entity.Property(e => e.PreviousApiSecret).HasMaxLength(200).IsRequired(false);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired(false);
            entity.HasIndex(e => e.ApiKey).IsUnique();
            entity.HasIndex(e => e.PreviousApiKey);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    private void ConfigureSysExternalApiTokenLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SysExternalApiTokenLog>(entity =>
        {
            entity.ToTable(TableName("sysexternalapitokenlog"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TokenId).IsRequired();
            entity.Property(e => e.Action).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ApiKey).HasMaxLength(100).IsRequired();
            entity.Property(e => e.IpAddress).HasMaxLength(100).IsRequired(false);
            entity.Property(e => e.OperatorName).HasMaxLength(100).IsRequired(false);
            entity.Property(e => e.Remark).HasColumnType("text").IsRequired(false);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.TokenId);
            entity.HasIndex(e => e.CreatedAt);
        });
    }

    private void ConfigureSysExternalApiTokenHistory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SysExternalApiTokenHistory>(entity =>
        {
            entity.ToTable(TableName("sysexternalapitokenhistory"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TokenId).IsRequired();
            entity.Property(e => e.AppName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ApiKey).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ApiSecret).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ExpireTime).IsRequired(false);
            entity.Property(e => e.ValidUntil).IsRequired(false);
            entity.Property(e => e.InvalidatedAt).IsRequired(false);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.TokenId);
            entity.HasIndex(e => e.ApiKey);
            entity.HasIndex(e => e.CreatedAt);
        });
    }

    private void ConfigureSysExternalApiAccessLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SysExternalApiAccessLog>(entity =>
        {
            entity.ToTable(TableName("sysexternalapiaccesslog"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ApiKey).HasMaxLength(100).IsRequired();
            entity.Property(e => e.RequestPath).HasMaxLength(500).IsRequired();
            entity.Property(e => e.RequestMethod).HasMaxLength(20).IsRequired();
            entity.Property(e => e.RequestParams).HasColumnType("text").IsRequired(false);
            entity.Property(e => e.IpAddress).HasMaxLength(100).IsRequired(false);
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            entity.Property(e => e.ErrorMessage).HasColumnType("text").IsRequired(false);
            entity.Property(e => e.IdempotencyKey).HasMaxLength(100).IsRequired(false);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.ApiKey);
            entity.HasIndex(e => e.CreatedAt);
        });
    }

    private void ConfigureSysExternalApi(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SysExternalApi>(entity =>
        {
            entity.ToTable(TableName("sysexternalapi"));
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ApiName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Route).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Method).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500).IsRequired(false);
            entity.Property(e => e.RateLimitPerSecond).IsRequired();
            entity.Property(e => e.RequireIdempotency).HasDefaultValue(true);
            entity.Property(e => e.IsEnabled).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired(false);
            entity.HasIndex(e => new { e.Route, e.Method }).IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }
}
