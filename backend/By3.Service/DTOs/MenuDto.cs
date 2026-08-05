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
/// 创建菜单请求。
/// </summary>
public class CreateMenuDto
{
    public string MenuName { get; set; } = string.Empty;
    public string? Permission { get; set; }
    public string? Route { get; set; }
    public string? Icon { get; set; }
    public string? Component { get; set; }
    public int MenuType { get; set; }
    public int SortOrder { get; set; }
    public Guid? ParentId { get; set; }
}

/// <summary>
/// 更新菜单请求。
/// </summary>
public class UpdateMenuDto
{
    public Guid Id { get; set; }
    public string? MenuName { get; set; }
    public string? Permission { get; set; }
    public string? Route { get; set; }
    public string? Icon { get; set; }
    public string? Component { get; set; }
    public int? MenuType { get; set; }
    public int? SortOrder { get; set; }
    public Guid? ParentId { get; set; }
    public bool? IsEnabled { get; set; }
}
