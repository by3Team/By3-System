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
using By3.Repository.Entities;
using By3.Repository.Repositories;
using By3.Service.DTOs;

namespace By3.Service.Services;

public class EmailSettingService
{
    private readonly EmailSettingRepository _repo;

    public EmailSettingService(EmailSettingRepository repo)
    {
        _repo = repo;
    }

    public async Task<EmailSettingDto> GetAsync()
    {
        var setting = await _repo.GetOrCreateDefaultAsync();
        return MapToDto(setting);
    }

    public async Task<EmailSettingDto> SaveAsync(EmailSettingDto dto)
    {
        var entity = new SysEmailSetting
        {
            Id = dto.Id,
            SmtpHost = dto.SmtpHost,
            SmtpPort = dto.SmtpPort,
            Username = dto.Username,
            Password = dto.Password,
            FromName = dto.FromName,
            FromAddress = dto.FromAddress,
            EnableSsl = dto.EnableSsl,
            IsEnabled = dto.IsEnabled
        };

        var saved = await _repo.SaveAsync(entity);
        return MapToDto(saved);
    }

    /// <summary>
    /// 测试邮件发送端连接，仅连接并验证，不发送邮件。
    /// </summary>
    public async Task<(bool Success, string Message)> TestConnectionAsync(EmailSettingDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.SmtpHost) || dto.SmtpPort <= 0)
            return (false, "SMTP 服务器地址或端口未配置");

        using var client = new MailKit.Net.Smtp.SmtpClient();
        try
        {
            var sslOptions = dto.EnableSsl
                ? MailKit.Security.SecureSocketOptions.StartTls
                : MailKit.Security.SecureSocketOptions.Auto;
            await client.ConnectAsync(dto.SmtpHost, dto.SmtpPort, sslOptions);

            if (!string.IsNullOrWhiteSpace(dto.Username))
            {
                await client.AuthenticateAsync(dto.Username, dto.Password);
            }

            await client.DisconnectAsync(true);
            return (true, "连接成功");
        }
        catch (Exception ex)
        {
            return (false, $"连接失败：{ex.Message}");
        }
    }

    private static EmailSettingDto MapToDto(SysEmailSetting s) => new()
    {
        Id = s.Id,
        SmtpHost = s.SmtpHost,
        SmtpPort = s.SmtpPort,
        Username = s.Username,
        Password = s.Password,
        FromName = s.FromName,
        FromAddress = s.FromAddress,
        EnableSsl = s.EnableSsl,
        IsEnabled = s.IsEnabled,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };
}
