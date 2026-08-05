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
/// 对外 API Token 历史凭证表。
/// 每次重新生成 Key/Secret 时，将旧凭证归档到此表，便于追踪所有生效过的 Secret Key。
/// </summary>
public class SysExternalApiTokenHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 关联的当前 Token Id。
    /// </summary>
    public Guid TokenId { get; set; }

    /// <summary>
    /// 应用名称（归档时快照）。
    /// </summary>
    public string AppName { get; set; } = string.Empty;

    /// <summary>
    /// 历史 ApiKey。
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// 历史 ApiSecret。
    /// </summary>
    public string ApiSecret { get; set; } = string.Empty;

    /// <summary>
    /// 该历史凭证当时的过期时间。
    /// </summary>
    public DateTime? ExpireTime { get; set; }

    /// <summary>
    /// 旧 Key 缓冲截止时间。为空表示立即失效。
    /// </summary>
    public DateTime? ValidUntil { get; set; }

    /// <summary>
    /// 手动作废时间。为空表示未手动作废。
    /// </summary>
    public DateTime? InvalidatedAt { get; set; }

    /// <summary>
    /// 手动作废人 Id。
    /// </summary>
    public Guid? InvalidatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
