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
    /// <summary>
    /// 模板唯一标识
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 模板编码
    /// </summary>
    public string TemplateCode { get; set; } = string.Empty;

    /// <summary>
    /// 模板名称
    /// </summary>
    public string TemplateName { get; set; } = string.Empty;

    /// <summary>
    /// 模板描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 邮件模板版本响应。
/// </summary>
public class EmailTemplateVersionDto
{
    /// <summary>
    /// 版本唯一标识
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 所属模板标识
    /// </summary>
    public Guid TemplateId { get; set; }

    /// <summary>
    /// 版本号
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// 邮件主题
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// 邮件正文内容
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// 正文格式（html / text）
    /// </summary>
    public string BodyFormat { get; set; } = "html";

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 发送邮件请求。
/// </summary>
public class SendEmailDto
{
    /// <summary>
    /// 模板标识
    /// </summary>
    public Guid TemplateId { get; set; }

    /// <summary>
    /// 模板版本号
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// 收件人地址列表
    /// </summary>
    public List<string> ToAddresses { get; set; } = new();

    /// <summary>
    /// 抄送地址列表
    /// </summary>
    public List<string> CcAddresses { get; set; } = new();

    /// <summary>
    /// 模板变量键值对
    /// </summary>
    public Dictionary<string, string>? Variables { get; set; }
}

/// <summary>
/// 测试邮件发送请求。
/// </summary>
public class TestEmailDto
{
    /// <summary>
    /// 模板标识
    /// </summary>
    public Guid TemplateId { get; set; }

    /// <summary>
    /// 模板版本号
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// 收件人地址
    /// </summary>
    public string ToAddress { get; set; } = string.Empty;

    /// <summary>
    /// 抄送地址列表
    /// </summary>
    public List<string> CcAddresses { get; set; } = new();

    /// <summary>
    /// 模板变量键值对
    /// </summary>
    public Dictionary<string, string>? Variables { get; set; }
}

/// <summary>
/// 邮件发送日志响应。
/// </summary>
public class EmailLogDto
{
    /// <summary>
    /// 日志唯一标识
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 关联模板标识
    /// </summary>
    public Guid? TemplateId { get; set; }

    /// <summary>
    /// 收件人地址
    /// </summary>
    public string ToAddresses { get; set; } = string.Empty;

    /// <summary>
    /// 抄送地址
    /// </summary>
    public string CcAddresses { get; set; } = string.Empty;

    /// <summary>
    /// 邮件主题
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// 发送状态
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
