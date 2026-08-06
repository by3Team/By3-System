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
    /// <summary>
    /// 岗位名称
    /// </summary>
    public string PositionName { get; set; } = string.Empty;

    /// <summary>
    /// 岗位编码
    /// </summary>
    public string? PositionCode { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    public int SortOrder { get; set; } = 0;
}

/// <summary>
/// 更新岗位请求。
/// </summary>
public class UpdatePositionDto
{
    /// <summary>
    /// 岗位ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 岗位名称
    /// </summary>
    public string? PositionName { get; set; }

    /// <summary>
    /// 岗位编码
    /// </summary>
    public string? PositionCode { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    public int? SortOrder { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }
}

/// <summary>
/// 岗位列表响应。
/// </summary>
public class PositionListDto
{
    /// <summary>
    /// 岗位ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 岗位名称
    /// </summary>
    public string PositionName { get; set; } = string.Empty;

    /// <summary>
    /// 岗位编码
    /// </summary>
    public string? PositionCode { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
