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

/// <summary>
/// 对外开放的 API 接口注册表。
/// 只有在此表中注册并启用的接口，才允许通过 AK/SK 签名方式被外部调用。
/// </summary>
public class SysExternalApi
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>接口名称</summary>
    public string ApiName { get; set; } = string.Empty;

    /// <summary>请求路径，例如 /api/external/v1/users</summary>
    public string Route { get; set; } = string.Empty;

    /// <summary>请求方法，例如 GET、POST、PUT、DELETE</summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>接口描述</summary>
    public string? Description { get; set; }

    /// <summary>单个 AK 每秒钟最大请求数，0 表示不限流</summary>
    public int RateLimitPerSecond { get; set; } = 0;

    /// <summary>是否需要校验 Idempotency-Key</summary>
    public bool RequireIdempotency { get; set; } = true;

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>软删除标记</summary>
    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}
