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
/// 对外 API 签名认证中间件配置。
/// </summary>
public class ExternalApiAuthOptions
{
    /// <summary>
    /// 限流窗口（秒）。默认 1 秒。
    /// </summary>
    public int RateWindowSeconds { get; set; } = 1;

    /// <summary>
    /// 幂等 Key 缓存时长（小时）。默认 24 小时。
    /// </summary>
    public int IdempotencyWindowHours { get; set; } = 24;

    /// <summary>
    /// 连续认证失败最大次数，超过则临时封禁。默认 5 次。
    /// </summary>
    public int MaxConsecutiveFailures { get; set; } = 5;

    /// <summary>
    /// 失败计数窗口（分钟）。默认 15 分钟。
    /// </summary>
    public int FailureWindowMinutes { get; set; } = 15;
}
