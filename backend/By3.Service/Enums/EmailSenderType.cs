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

using System.Text.Json.Serialization;

namespace By3.Service.Enums;

/// <summary>
/// 邮件发送来源类型。
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EmailSenderType
{
    /// <summary>
    /// 系统内部触发。
    /// </summary>
    System,

    /// <summary>
    /// 后台手动发送。
    /// </summary>
    Manual,

    /// <summary>
    /// 定时任务触发。
    /// </summary>
    Scheduled,

    /// <summary>
    /// 外部 API 触发。
    /// </summary>
    Api
}
