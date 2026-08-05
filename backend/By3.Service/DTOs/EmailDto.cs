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

namespace By3.Service.DTOs;

/// <summary>
/// 邮件模板响应。
/// </summary>
public class EmailTemplateDto
{
    public Guid Id { get; set; }
    public string TemplateCode { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 邮件模板版本响应。
/// </summary>
public class EmailTemplateVersionDto
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string BodyFormat { get; set; } = "html";
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 发送邮件请求。
/// </summary>
public class SendEmailDto
{
    public Guid TemplateId { get; set; }
    public string Version { get; set; } = string.Empty;
    public List<string> ToAddresses { get; set; } = new();
    public List<string> CcAddresses { get; set; } = new();
    public Dictionary<string, string>? Variables { get; set; }
}

/// <summary>
/// 测试邮件发送请求。
/// </summary>
public class TestEmailDto
{
    public Guid TemplateId { get; set; }
    public string Version { get; set; } = string.Empty;
    public string ToAddress { get; set; } = string.Empty;
    public List<string> CcAddresses { get; set; } = new();
    public Dictionary<string, string>? Variables { get; set; }
}

/// <summary>
/// 邮件发送日志响应。
/// </summary>
public class EmailLogDto
{
    public Guid Id { get; set; }
    public Guid? TemplateId { get; set; }
    public string ToAddresses { get; set; } = string.Empty;
    public string CcAddresses { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
