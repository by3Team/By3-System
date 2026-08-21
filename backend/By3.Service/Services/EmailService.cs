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

using MailKit.Net.Smtp;
using MimeKit;
using By3.Repository.Entities;
using By3.Repository.Repositories;
using By3.Service.DTOs;
using By3.Service.Enums;

namespace By3.Service.Services;

public class EmailService
{
    private readonly EmailTemplateRepository _templateRepo;
    private readonly EmailTemplateVersionRepository _versionRepo;
    private readonly EmailLogRepository _logRepo;
    private readonly EmailSettingRepository _settingRepo;

    public EmailService(
        EmailTemplateRepository templateRepo,
        EmailTemplateVersionRepository versionRepo,
        EmailLogRepository logRepo,
        EmailSettingRepository settingRepo)
    {
        _templateRepo = templateRepo;
        _versionRepo = versionRepo;
        _logRepo = logRepo;
        _settingRepo = settingRepo;
    }

    /// <summary>
    /// 分页查询邮件模板列表。
    /// </summary>
    public async Task<PageResult<EmailTemplateDto>> GetTemplateListAsync(int page, int pageSize, string? keyword)
    {
        var items = await _templateRepo.GetListAsync(page, pageSize, keyword);
        var total = await _templateRepo.GetCountAsync(keyword);
        return new PageResult<EmailTemplateDto>
        {
            Total = total,
            Items = items.Select(MapTemplateToDto).ToList(),
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// 根据 ID 获取邮件模板。
    /// </summary>
    public async Task<EmailTemplateDto?> GetTemplateByIdAsync(Guid id)
    {
        var template = await _templateRepo.GetByIdAsync(id);
        return template == null ? null : MapTemplateToDto(template);
    }

    /// <summary>
    /// 创建邮件模板。
    /// </summary>
    public async Task<Guid> CreateTemplateAsync(CreateEmailTemplateDto dto, Guid? userId)
    {
        var template = new SysEmailTemplate
        {
            Id = Guid.NewGuid(),
            TemplateCode = dto.TemplateCode,
            TemplateName = dto.TemplateName,
            Description = dto.Description ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
        return await _templateRepo.CreateAsync(template);
    }

    /// <summary>
    /// 更新邮件模板信息。
    /// </summary>
    public async Task<int> UpdateTemplateAsync(UpdateEmailTemplateDto dto)
    {
        var template = await _templateRepo.GetByIdAsync(dto.Id);
        if (template == null) return 0;
        template.TemplateName = dto.TemplateName ?? template.TemplateName;
        template.Description = dto.Description ?? template.Description;
        template.IsEnabled = dto.IsEnabled ?? template.IsEnabled;
        template.UpdatedAt = DateTime.UtcNow;
        return await _templateRepo.UpdateAsync(template);
    }

    /// <summary>
    /// 删除邮件模板（先备份到删除备份表，再物理删除）。
    /// </summary>
    public async Task<int> DeleteTemplateAsync(Guid id, Guid? deletedBy)
        => await _templateRepo.DeleteAsync(id, deletedBy);

    /// <summary>
    /// 获取指定模板的所有版本列表。
    /// </summary>
    public async Task<List<EmailTemplateVersionDto>> GetVersionsByTemplateIdAsync(Guid templateId)
    {
        var versions = await _versionRepo.GetByTemplateIdAsync(templateId);
        return versions.Select(MapVersionToDto).ToList();
    }

    /// <summary>
    /// 根据 ID 获取模板版本。
    /// </summary>
    public async Task<EmailTemplateVersionDto?> GetVersionByIdAsync(Guid id)
    {
        var version = await _versionRepo.GetByIdAsync(id);
        return version == null ? null : MapVersionToDto(version);
    }

    /// <summary>
    /// 创建邮件模板版本。新版本自动启用，并将同模板下其它版本禁用。
    /// </summary>
    public async Task<Guid> CreateVersionAsync(CreateEmailTemplateVersionDto dto, Guid? userId)
    {
        var versionStr = dto.Version;
        if (string.IsNullOrWhiteSpace(versionStr))
        {
            var existingVersions = await _versionRepo.GetByTemplateIdAsync(dto.TemplateId);
            var maxNum = existingVersions
                .Select(v => v.Version)
                .Where(v => v.StartsWith("v") && int.TryParse(v[1..], out _))
                .Select(v => int.Parse(v[1..]))
                .DefaultIfEmpty(0)
                .Max();
            versionStr = $"v{maxNum + 1}";
        }

        if (await _versionRepo.ExistsAsync(dto.TemplateId, versionStr))
            throw new InvalidOperationException($"版本 {versionStr} 已存在");

        var version = new SysEmailTemplateVersion
        {
            Id = Guid.NewGuid(),
            TemplateId = dto.TemplateId,
            Version = versionStr,
            Subject = dto.Subject,
            Body = dto.Body,
            BodyFormat = dto.BodyFormat ?? "html",
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
        var newVersionId = await _versionRepo.CreateAsync(version);

        // 仅保留最新版本启用
        await _versionRepo.DisableOtherVersionsAsync(dto.TemplateId, newVersionId);

        return newVersionId;
    }

    /// <summary>
    /// 更新邮件模板版本内容（仅允许编辑启用中的版本）。
    /// </summary>
    public async Task<int> UpdateVersionAsync(UpdateEmailTemplateVersionDto dto)
    {
        var version = await _versionRepo.GetByIdAsync(dto.Id);
        if (version == null) return 0;
        if (!version.IsEnabled)
            throw new InvalidOperationException("已禁用的版本不允许编辑");
        version.Subject = dto.Subject ?? version.Subject;
        version.Body = dto.Body ?? version.Body;
        version.BodyFormat = dto.BodyFormat ?? version.BodyFormat;
        version.UpdatedAt = DateTime.UtcNow;
        return await _versionRepo.UpdateAsync(version);
    }

    /// <summary>
    /// 删除邮件模板版本（仅允许删除已禁用的版本）。
    /// </summary>
    public async Task<int> DeleteVersionAsync(Guid id, Guid? deletedBy)
    {
        var version = await _versionRepo.GetByIdAsync(id);
        if (version == null) return 0;
        if (version.IsEnabled)
            throw new InvalidOperationException("启用中的版本不能删除");
        return await _versionRepo.DeleteAsync(id, deletedBy);
    }

    /// <summary>
    /// 批量发送邮件，按模板渲染内容并记录日志。
    /// </summary>
    public async Task SendBatchAsync(SendEmailDto dto, EmailSenderType senderType = EmailSenderType.System, string? senderName = null)
    {
        var version = await ResolveVersionAsync(dto.TemplateId, dto.Version);
        if (version == null)
            throw new InvalidOperationException("邮件模板或版本不存在");

        // 预先验证邮件配置
        var setting = await _settingRepo.GetAsync() ?? throw new InvalidOperationException("邮件发送端未配置，请先在系统设置中配置邮件参数");
        if (string.IsNullOrWhiteSpace(setting.SmtpHost))
            throw new InvalidOperationException("SMTP服务器地址未配置");
        if (string.IsNullOrWhiteSpace(setting.Username))
            throw new InvalidOperationException("SMTP用户名未配置");

        var subject = ReplaceVariables(version.Subject, dto.Variables);
        var body = ReplaceVariables(version.Body, dto.Variables);
        var bodyFormat = version.BodyFormat;
        var ccList = dto.CcAddresses.Where(a => !string.IsNullOrWhiteSpace(a)).ToList();
        var ccAddresses = string.Join(",", ccList);

        var resolvedSenderName = GetSenderName(senderType, senderName);
        foreach (var address in dto.ToAddresses.Where(a => !string.IsNullOrWhiteSpace(a)))
        {
            var logId = await _logRepo.CreateAsync(new SysEmailLog
            {
                Id = Guid.NewGuid(),
                TemplateId = dto.TemplateId,
                TemplateVersionId = version.Id,
                ToAddresses = address,
                CcAddresses = ccAddresses,
                Subject = subject,
                Body = body,
                Status = "pending",
                SenderType = senderType.ToString(),
                SenderName = resolvedSenderName,
                CreatedAt = DateTime.UtcNow
            });

            try
            {
                await SendEmailAsync(address, ccList, subject, body, bodyFormat);
                await _logRepo.UpdateStatusAsync(logId, "sent", null, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                await _logRepo.UpdateStatusAsync(logId, "failed", ex.Message, null);
            }
        }
    }

    /// <summary>
    /// 发送测试邮件（前缀 [TEST] ）。
    /// </summary>
    public async Task SendTestAsync(TestEmailDto dto, EmailSenderType senderType = EmailSenderType.System, string? senderName = null)
    {
        var version = await ResolveVersionAsync(dto.TemplateId, dto.Version);
        if (version == null)
            throw new InvalidOperationException("邮件模板或版本不存在");

        var subject = ReplaceVariables(version.Subject, dto.Variables);
        var body = ReplaceVariables(version.Body, dto.Variables);
        var ccList = dto.CcAddresses.Where(a => !string.IsNullOrWhiteSpace(a)).ToList();
        var resolvedSenderName = GetSenderName(senderType, senderName);

        var logId = await _logRepo.CreateAsync(new SysEmailLog
        {
            Id = Guid.NewGuid(),
            TemplateId = dto.TemplateId,
            TemplateVersionId = version.Id,
            ToAddresses = dto.ToAddress,
            CcAddresses = string.Join(",", ccList),
            Subject = "[TEST] " + subject,
            Body = body,
            Status = "pending",
            SenderType = senderType.ToString(),
            SenderName = resolvedSenderName,
            CreatedAt = DateTime.UtcNow
        });

        try
        {
            await SendEmailAsync(dto.ToAddress, ccList, "[TEST] " + subject, body, version.BodyFormat);
            await _logRepo.UpdateStatusAsync(logId, "sent", null, DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            await _logRepo.UpdateStatusAsync(logId, "failed", ex.Message, null);
            throw;
        }
    }

    /// <summary>
    /// 分页查询邮件发送日志。
    /// </summary>
    public async Task<PageResult<EmailLogDto>> GetLogListAsync(int page, int pageSize, string? keyword, string? status, DateTime? startDate = null, DateTime? endDate = null)
    {
        var items = await _logRepo.GetListAsync(page, pageSize, keyword, status, startDate, endDate);
        var total = await _logRepo.GetCountAsync(keyword, status, startDate, endDate);
        return new PageResult<EmailLogDto>
        {
            Total = total,
            Items = items.Select(MapLogToDto).ToList(),
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// 直接发送通知邮件（不依赖模板），并记录邮件日志。
    /// </summary>
    public async Task SendRawAsync(string toAddress, List<string>? ccAddresses, string subject, string body, string bodyFormat, EmailSenderType senderType = EmailSenderType.System, string? senderName = null)
    {
        var ccList = ccAddresses?.Where(a => !string.IsNullOrWhiteSpace(a)).ToList() ?? new List<string>();
        var ccString = string.Join(",", ccList);
        var resolvedSenderName = GetSenderName(senderType, senderName);

        var logId = await _logRepo.CreateAsync(new SysEmailLog
        {
            Id = Guid.NewGuid(),
            ToAddresses = toAddress,
            CcAddresses = ccString,
            Subject = subject,
            Body = body,
            Status = "pending",
            SenderType = senderType.ToString(),
            SenderName = resolvedSenderName,
            CreatedAt = DateTime.UtcNow
        });

        try
        {
            await SendEmailAsync(toAddress, ccList, subject, body, bodyFormat);
            await _logRepo.UpdateStatusAsync(logId, "sent", null, DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            await _logRepo.UpdateStatusAsync(logId, "failed", ex.Message, null);
        }
    }

    /// <summary>
    /// 解析模板版本：按版本号或最新激活版本查找。
    /// </summary>
    private async Task<SysEmailTemplateVersion?> ResolveVersionAsync(Guid templateId, string version)
    {
        if (!string.IsNullOrWhiteSpace(version))
        {
            var versions = await _versionRepo.GetByTemplateIdAsync(templateId);
            return versions.FirstOrDefault(v => v.Version == version && v.IsEnabled);
        }
        return await _versionRepo.GetActiveByTemplateIdAsync(templateId);
    }

    /// <summary>
    /// 通过 SMTP 发送邮件。
    /// </summary>
    private async Task SendEmailAsync(string toAddress, List<string> ccAddresses, string subject, string body, string bodyFormat)
    {
        var setting = await _settingRepo.GetAsync() ?? throw new InvalidOperationException("邮件发送端未配置，请先在系统设置中配置邮件参数");

        if (string.IsNullOrWhiteSpace(setting.SmtpHost))
            throw new InvalidOperationException("SMTP服务器地址未配置");
        if (string.IsNullOrWhiteSpace(setting.Username))
            throw new InvalidOperationException("SMTP用户名未配置");

        var host = NormalizeSmtpHost(setting.SmtpHost);
        var port = setting.SmtpPort;
        var username = setting.Username;
        var password = setting.Password;
        var fromName = string.IsNullOrWhiteSpace(setting.FromName) ? username : setting.FromName;
        var fromAddress = string.IsNullOrWhiteSpace(setting.FromAddress) ? username : setting.FromAddress;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromAddress));
        message.To.Add(new MailboxAddress(toAddress, toAddress));
        foreach (var cc in ccAddresses)
        {
            message.Cc.Add(new MailboxAddress(cc, cc));
        }
        message.Subject = subject;

        var mimeFormat = bodyFormat?.ToLowerInvariant() == "plain" ? "plain" : "html";
        message.Body = new TextPart(mimeFormat) { Text = body };

        using var client = new SmtpClient();
        var sslOptions = ResolveSslOptions(port, setting.EnableSsl);
        await client.ConnectAsync(host, port, sslOptions);
        await client.AuthenticateAsync(username, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    /// <summary>
    /// 根据端口和 SSL 配置选择 SMTP 连接的安全选项。
    /// </summary>
    private static MailKit.Security.SecureSocketOptions ResolveSslOptions(int port, bool enableSsl)
    {
        if (!enableSsl)
            return MailKit.Security.SecureSocketOptions.Auto;

        return port switch
        {
            465 => MailKit.Security.SecureSocketOptions.SslOnConnect,
            _ => MailKit.Security.SecureSocketOptions.StartTls,
        };
    }

    /// <summary>
    /// 规范化 SMTP 主机地址，去除 scheme 和端口号。
    /// </summary>
    private static string NormalizeSmtpHost(string host)
    {
        if (string.IsNullOrWhiteSpace(host)) return host;
        var trimmed = host.Trim();

        // 尝试按绝对 URI 解析并提取 Host
        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
            return uri.Host;

        // 去掉 scheme（如 smtp://、smtps://）
        var schemeIndex = trimmed.IndexOf("://", StringComparison.Ordinal);
        if (schemeIndex > 0)
            trimmed = trimmed[(schemeIndex + 3)..];

        // 去掉端口号
        var colonIndex = trimmed.LastIndexOf(':');
        if (colonIndex > 0)
        {
            var portPart = trimmed[(colonIndex + 1)..];
            if (int.TryParse(portPart, out _))
                trimmed = trimmed[..colonIndex];
        }

        return trimmed;
    }

    /// <summary>
    /// 替换内容中的模板变量（{key} 格式）。
    /// </summary>
    private static string ReplaceVariables(string content, Dictionary<string, string>? variables)
    {
        if (variables == null) return content;
        foreach (var kv in variables)
        {
            content = content.Replace($"{{{kv.Key}}}", kv.Value);
        }
        return content;
    }

    /// <summary>
    /// 将邮件模板实体映射为 DTO。
    /// </summary>
    private static EmailTemplateDto MapTemplateToDto(SysEmailTemplate t) => new()
    {
        Id = t.Id,
        TemplateCode = t.TemplateCode,
        TemplateName = t.TemplateName,
        Description = t.Description,
        IsEnabled = t.IsEnabled,
        CreatedAt = t.CreatedAt
    };

    /// <summary>
    /// 将模板版本实体映射为 DTO。
    /// </summary>
    private static EmailTemplateVersionDto MapVersionToDto(SysEmailTemplateVersion v) => new()
    {
        Id = v.Id,
        TemplateId = v.TemplateId,
        Version = v.Version,
        Subject = v.Subject,
        Body = v.Body,
        BodyFormat = v.BodyFormat,
        IsEnabled = v.IsEnabled,
        CreatedAt = v.CreatedAt
    };

    /// <summary>
    /// 将邮件日志实体映射为 DTO。
    /// </summary>
    private static EmailLogDto MapLogToDto(SysEmailLog l) => new()
    {
        Id = l.Id,
        TemplateId = l.TemplateId,
        ToAddresses = l.ToAddresses,
        CcAddresses = l.CcAddresses,
        Subject = l.Subject,
        Status = l.Status,
        ErrorMessage = l.ErrorMessage,
        SentAt = l.SentAt,
        SenderType = l.SenderType,
        SenderName = l.SenderName,
        CreatedAt = l.CreatedAt
    };

    /// <summary>
    /// 解析发送人名称：系统触发使用默认名称，其他情况使用传入名称。
    /// </summary>
    private static string GetSenderName(EmailSenderType senderType, string? senderName)
    {
        if (!string.IsNullOrWhiteSpace(senderName))
            return senderName.Trim();

        return senderType switch
        {
            EmailSenderType.System => "系统",
            EmailSenderType.Scheduled => "定时任务",
            EmailSenderType.Api => "外部接口",
            _ => senderType.ToString()
        };
    }
}

/// <summary>
/// 创建邮件模板请求。
/// </summary>
public class CreateEmailTemplateDto
{
    /// <summary>
    /// 模板编码
    /// </summary>
    public string TemplateCode { get; set; } = string.Empty;

    /// <summary>
    /// 模板名称
    /// </summary>
    public string TemplateName { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// 更新邮件模板请求。
/// </summary>
public class UpdateEmailTemplateDto
{
    /// <summary>
    /// 模板ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 模板名称
    /// </summary>
    public string? TemplateName { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }
}

/// <summary>
/// 创建邮件模板版本请求。
/// </summary>
public class CreateEmailTemplateVersionDto
{
    /// <summary>
    /// 模板ID
    /// </summary>
    public Guid TemplateId { get; set; }

    /// <summary>
    /// 版本号
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// 邮件主题
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// 邮件正文
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// 正文格式（html/text）
    /// </summary>
    public string BodyFormat { get; set; } = "html";
}

/// <summary>
/// 更新邮件模板版本请求。
/// </summary>
public class UpdateEmailTemplateVersionDto
{
    /// <summary>
    /// 版本ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 邮件主题
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// 邮件正文
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// 正文格式（html/text）
    /// </summary>
    public string? BodyFormat { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }
}
