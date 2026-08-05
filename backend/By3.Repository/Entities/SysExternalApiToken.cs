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

namespace By3.Repository.Entities;

public class SysExternalApiToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string AppName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? ExpireTime { get; set; }

    /// <summary>
    /// 有效期类型：30/60/90/custom。
    /// </summary>
    public string ExpireType { get; set; } = "30";

    /// <summary>
    /// 允许访问的对外接口 ID 列表，JSON 数组格式。为空表示允许访问所有已注册接口。
    /// </summary>
    public string? AllowedApiIds { get; set; }

    /// <summary>
    /// 负责人邮箱，多个邮箱用逗号分隔。
    /// </summary>
    public string ContactEmail { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// 重新生成后，旧 ApiKey 的缓冲期截止时间。为空表示旧 Key 立即失效。
    /// </summary>
    public DateTime? PreviousValidUntil { get; set; }

    /// <summary>
    /// 重新生成前的旧 ApiKey，用于缓冲期内继续验证。
    /// </summary>
    public string? PreviousApiKey { get; set; }

    /// <summary>
    /// 重新生成前的旧 ApiSecret，与 PreviousApiKey 配套使用。
    /// </summary>
    public string? PreviousApiSecret { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}
