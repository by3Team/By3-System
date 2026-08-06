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

namespace By3.Api.Options;

/// <summary>
/// 限流配置。
/// </summary>
public class RateLimitingOptions
{
    /// <summary>
    /// 全局限流：每窗口允许的请求数。
    /// </summary>
    public int GlobalPermitLimit { get; set; }

    /// <summary>
    /// 全局限流：窗口时长（分钟）。
    /// </summary>
    public int GlobalWindowMinutes { get; set; }

    /// <summary>
    /// 全局限流：排队等待的最大请求数。
    /// </summary>
    public int GlobalQueueLimit { get; set; }

    /// <summary>
    /// 默认策略：每窗口允许的请求数。
    /// </summary>
    public int DefaultPermitLimit { get; set; }

    /// <summary>
    /// 默认策略：窗口时长（分钟）。
    /// </summary>
    public int DefaultWindowMinutes { get; set; }

    /// <summary>
    /// 默认策略：排队等待的最大请求数。
    /// </summary>
    public int DefaultQueueLimit { get; set; }

    /// <summary>
    /// 登录策略：每窗口允许的请求数。
    /// </summary>
    public int LoginPermitLimit { get; set; }

    /// <summary>
    /// 登录策略：窗口时长（分钟）。
    /// </summary>
    public int LoginWindowMinutes { get; set; }

    /// <summary>
    /// 登录策略：排队等待的最大请求数。
    /// </summary>
    public int LoginQueueLimit { get; set; }
}
