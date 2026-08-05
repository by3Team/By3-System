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
/// 对外 API Token 操作日志表。
/// 记录 Token 的创建、更新、删除、重生成、启用/禁用等关键操作，便于审计和问题排查。
/// </summary>
public class SysExternalApiTokenLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 关联的 Token Id。
    /// </summary>
    public Guid TokenId { get; set; }

    /// <summary>
    /// 操作类型：Create、Update、Delete、Regenerate、Enable、Disable。
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// 操作时的 ApiKey（当前 Key）。
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// 操作人 IP。
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// 操作人用户 Id。
    /// </summary>
    public Guid? OperatorId { get; set; }

    /// <summary>
    /// 操作人用户名。
    /// </summary>
    public string? OperatorName { get; set; }

    /// <summary>
    /// 操作备注，例如重生成时的缓冲期说明。
    /// </summary>
    public string? Remark { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
