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
    /// <summary>
    /// 菜单名称
    /// </summary>
    public string MenuName { get; set; } = string.Empty;

    /// <summary>
    /// 权限标识
    /// </summary>
    public string? Permission { get; set; }

    /// <summary>
    /// 路由地址
    /// </summary>
    public string? Route { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 组件路径
    /// </summary>
    public string? Component { get; set; }

    /// <summary>
    /// 菜单类型
    /// </summary>
    public int MenuType { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// 父级菜单ID
    /// </summary>
    public Guid? ParentId { get; set; }
}

/// <summary>
/// 更新菜单请求。
/// </summary>
public class UpdateMenuDto
{
    /// <summary>
    /// 菜单ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 菜单名称
    /// </summary>
    public string? MenuName { get; set; }

    /// <summary>
    /// 权限标识
    /// </summary>
    public string? Permission { get; set; }

    /// <summary>
    /// 路由地址
    /// </summary>
    public string? Route { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 组件路径
    /// </summary>
    public string? Component { get; set; }

    /// <summary>
    /// 菜单类型
    /// </summary>
    public int? MenuType { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    public int? SortOrder { get; set; }

    /// <summary>
    /// 父级菜单ID
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }
}
