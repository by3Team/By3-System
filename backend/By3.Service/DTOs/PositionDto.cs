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
/// 创建岗位请求。
/// </summary>
public class CreatePositionDto
{
    public string PositionName { get; set; } = string.Empty;
    public string? PositionCode { get; set; }
    public int SortOrder { get; set; } = 0;
}

/// <summary>
/// 更新岗位请求。
/// </summary>
public class UpdatePositionDto
{
    public Guid Id { get; set; }
    public string? PositionName { get; set; }
    public string? PositionCode { get; set; }
    public int? SortOrder { get; set; }
    public bool? IsEnabled { get; set; }
}

/// <summary>
/// 岗位列表响应。
/// </summary>
public class PositionListDto
{
    public Guid Id { get; set; }
    public string PositionName { get; set; } = string.Empty;
    public string? PositionCode { get; set; }
    public int SortOrder { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
}
