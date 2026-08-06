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

namespace By3.Service.Constants;

/// <summary>
/// 定时任务类型常量定义。
/// </summary>
public static class JobTypes
{
    /// <summary>
    /// 用户数据种子任务：批量生成模拟用户并备份现有数据。
    /// </summary>
    public const string UserDataSeed = "UserDataSeed";

    /// <summary>
    /// 所有已注册的任务类型列表。
    /// </summary>
    public static readonly IReadOnlyList<string> All = new[] { UserDataSeed };
}
