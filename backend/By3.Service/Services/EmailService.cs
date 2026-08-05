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

    public async Task<EmailTemplateDto?> GetTemplateByIdAsync(Guid id)
    {
        var template = await _templateRepo.GetByIdAsync(id);
        return template == null ? null : MapTemplateToDto(template);
    }

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

    public async Task<int> DeleteTemplateAsync(Guid id)
        => await _templateRepo.DeleteAsync(id);

    public async Task<List<EmailTemplateVersionDto>> GetVersionsByTemplateIdAsync(Guid templateId)
    {
        var versions = await _versionRepo.GetByTemplateIdAsync(templateId);
        return versions.Select(MapVersionToDto).ToList();
    }

    public async Task<EmailTemplateVersionDto?> GetVersionByIdAsync(Guid id)
    {
        var version = await _versionRepo.GetByIdAsync(id);
        return version == null ? null : MapVersionToDto(version);
    }

    public async Task<Guid> CreateVersionAsync(CreateEmailTemplateVersionDto dto, Guid? userId)
    {
        if (await _versionRepo.ExistsAsync(dto.TemplateId, dto.Version))
            throw new InvalidOperationException($"版本 {dto.Version} 已存在");

        var version = new SysEmailTemplateVersion
        {
            Id = Guid.NewGuid(),
            TemplateId = dto.TemplateId,
            Version = dto.Version,
            Subject = dto.Subject,
            Body = dto.Body,
            BodyFormat = dto.BodyFormat ?? "html",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
        return await _versionRepo.CreateAsync(version);
    }

    public async Task<int> UpdateVersionAsync(UpdateEmailTemplateVersionDto dto)
    {
        var version = await _versionRepo.GetByIdAsync(dto.Id);
        if (version == null) return 0;
        version.Subject = dto.Subject ?? version.Subject;
        version.Body = dto.Body ?? version.Body;
        version.BodyFormat = dto.BodyFormat ?? version.BodyFormat;
        version.IsEnabled = dto.IsEnabled ?? version.IsEnabled;
        version.UpdatedAt = DateTime.UtcNow;
        return await _versionRepo.UpdateAsync(version);
    }

    public async Task<int> DeleteVersionAsync(Guid id)
        => await _versionRepo.DeleteAsync(id);

    public async Task SendBatchAsync(SendEmailDto dto)
    {
        var version = await ResolveVersionAsync(dto.TemplateId, dto.Version);
        if (version == null)
            throw new InvalidOperationException("邮件模板或版本不存在");

        var subject = ReplaceVariables(version.Subject, dto.Variables);
        var body = ReplaceVariables(version.Body, dto.Variables);
        var bodyFormat = version.BodyFormat;
        var ccList = dto.CcAddresses.Where(a => !string.IsNullOrWhiteSpace(a)).ToList();
        var ccAddresses = string.Join(",", ccList);

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

    public async Task SendTestAsync(TestEmailDto dto)
    {
        var version = await ResolveVersionAsync(dto.TemplateId, dto.Version);
        if (version == null)
            throw new InvalidOperationException("邮件模板或版本不存在");

        var subject = ReplaceVariables(version.Subject, dto.Variables);
        var body = ReplaceVariables(version.Body, dto.Variables);
        var ccList = dto.CcAddresses.Where(a => !string.IsNullOrWhiteSpace(a)).ToList();
        await SendEmailAsync(dto.ToAddress, ccList, "[TEST] " + subject, body, version.BodyFormat);
    }

    public async Task<PageResult<EmailLogDto>> GetLogListAsync(int page, int pageSize, string? keyword, string? status)
    {
        var items = await _logRepo.GetListAsync(page, pageSize, keyword, status);
        var total = await _logRepo.GetCountAsync(keyword, status);
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
    public async Task SendRawAsync(string toAddress, List<string>? ccAddresses, string subject, string body, string bodyFormat)
    {
        var ccList = ccAddresses?.Where(a => !string.IsNullOrWhiteSpace(a)).ToList() ?? new List<string>();
        var ccString = string.Join(",", ccList);

        var logId = await _logRepo.CreateAsync(new SysEmailLog
        {
            Id = Guid.NewGuid(),
            ToAddresses = toAddress,
            CcAddresses = ccString,
            Subject = subject,
            Body = body,
            Status = "pending",
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

    private async Task<SysEmailTemplateVersion?> ResolveVersionAsync(Guid templateId, string version)
    {
        if (!string.IsNullOrWhiteSpace(version))
        {
            var versions = await _versionRepo.GetByTemplateIdAsync(templateId);
            return versions.FirstOrDefault(v => v.Version == version && v.IsEnabled && !v.IsDeleted);
        }
        return await _versionRepo.GetActiveByTemplateIdAsync(templateId);
    }

    private async Task SendEmailAsync(string toAddress, List<string> ccAddresses, string subject, string body, string bodyFormat)
    {
        var setting = await _settingRepo.GetAsync() ?? throw new InvalidOperationException("邮件发送端未配置，请先在系统设置中配置");

        var host = setting.SmtpHost;
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
        var sslOptions = setting.EnableSsl
            ? MailKit.Security.SecureSocketOptions.StartTls
            : MailKit.Security.SecureSocketOptions.Auto;
        await client.ConnectAsync(host, port, sslOptions);
        await client.AuthenticateAsync(username, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    private static string ReplaceVariables(string content, Dictionary<string, string>? variables)
    {
        if (variables == null) return content;
        foreach (var kv in variables)
        {
            content = content.Replace($"{{{kv.Key}}}", kv.Value);
        }
        return content;
    }

    private static EmailTemplateDto MapTemplateToDto(SysEmailTemplate t) => new()
    {
        Id = t.Id,
        TemplateCode = t.TemplateCode,
        TemplateName = t.TemplateName,
        Description = t.Description,
        IsEnabled = t.IsEnabled,
        CreatedAt = t.CreatedAt
    };

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
        CreatedAt = l.CreatedAt
    };
}

public class CreateEmailTemplateDto
{
    public string TemplateCode { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateEmailTemplateDto
{
    public Guid Id { get; set; }
    public string? TemplateName { get; set; }
    public string? Description { get; set; }
    public bool? IsEnabled { get; set; }
}

public class CreateEmailTemplateVersionDto
{
    public Guid TemplateId { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string BodyFormat { get; set; } = "html";
}

public class UpdateEmailTemplateVersionDto
{
    public Guid Id { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string? BodyFormat { get; set; }
    public bool? IsEnabled { get; set; }
}
