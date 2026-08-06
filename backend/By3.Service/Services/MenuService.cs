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

using Microsoft.AspNetCore.Http;
using By3.Repository.Entities;
using By3.Repository.Repositories;
using By3.Service.DTOs;

namespace By3.Service.Services;

public class MenuService
{
    private readonly MenuRepository _repo;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MenuService(MenuRepository repo, IHttpContextAccessor httpContextAccessor)
    {
        _repo = repo;
        _httpContextAccessor = httpContextAccessor;
    }

    private Guid? CurrentUserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userId, out var id) ? id : null;
        }
    }

    /// <summary>
    /// 获取所有菜单树形结构。
    /// </summary>
    public async Task<List<MenuTreeDto>> GetAllAsync()
    {
        var menus = await _repo.GetAllAsync();
        return BuildTree(menus);
    }

    /// <summary>
    /// 根据ID获取菜单详情。
    /// </summary>
    public async Task<MenuTreeDto?> GetByIdAsync(Guid id)
    {
        var m = await _repo.GetByIdAsync(id);
        if (m == null) return null;
        return new MenuTreeDto
        {
            Id = m.Id,
            MenuName = m.MenuName,
            Permission = m.Permission,
            Route = m.Route,
            Icon = m.Icon,
            Component = m.Component,
            MenuType = m.MenuType,
            SortOrder = m.SortOrder,
            ParentId = m.ParentId
        };
    }

    /// <summary>
    /// 创建菜单。
    /// </summary>
    public async Task<Guid> CreateAsync(CreateMenuDto dto)
    {
        var menu = new SysMenu
        {
            MenuName = dto.MenuName,
            Permission = dto.Permission,
            Route = dto.Route,
            Icon = dto.Icon,
            Component = dto.Component,
            MenuType = dto.MenuType,
            SortOrder = dto.SortOrder,
            ParentId = dto.ParentId,
            CreatedBy = CurrentUserId,
            CreatedAt = DateTime.UtcNow
        };
        return await _repo.CreateAsync(menu);
    }

    /// <summary>
    /// 更新菜单信息。
    /// </summary>
    public async Task<int> UpdateAsync(UpdateMenuDto dto)
    {
        var menu = await _repo.GetByIdAsync(dto.Id);
        if (menu == null) return 0;

        if (dto.MenuName != null) menu.MenuName = dto.MenuName;
        if (dto.Permission != null) menu.Permission = dto.Permission;
        if (dto.Route != null) menu.Route = dto.Route;
        if (dto.Icon != null) menu.Icon = dto.Icon;
        if (dto.Component != null) menu.Component = dto.Component;
        if (dto.MenuType.HasValue) menu.MenuType = dto.MenuType.Value;
        if (dto.SortOrder.HasValue) menu.SortOrder = dto.SortOrder.Value;
        if (dto.ParentId.HasValue) menu.ParentId = dto.ParentId.Value;
        if (dto.IsEnabled.HasValue) menu.IsEnabled = dto.IsEnabled.Value;
        menu.UpdatedAt = DateTime.UtcNow;
        menu.UpdatedBy = CurrentUserId;

        return await _repo.UpdateAsync(menu);
    }

    /// <summary>
    /// 删除菜单。
    /// </summary>
    public async Task<int> DeleteAsync(Guid id)
        => await _repo.DeleteAsync(id);

    /// <summary>
    /// 获取系统所有权限标识列表。
    /// </summary>
    public async Task<List<string>> GetAllPermissionsAsync()
        => await _repo.GetAllPermissionsAsync();

    private static List<MenuTreeDto> BuildTree(List<SysMenu> menus)
    {
        var dict = menus.ToDictionary(m => m.Id, m => new MenuTreeDto
        {
            Id = m.Id,
            MenuName = m.MenuName,
            Permission = m.Permission,
            Route = m.Route,
            Icon = m.Icon,
            Component = m.Component,
            MenuType = m.MenuType,
            SortOrder = m.SortOrder,
            ParentId = m.ParentId
        });

        var roots = new List<MenuTreeDto>();
        foreach (var item in dict.Values)
        {
            if (item.ParentId == null || !dict.ContainsKey(item.ParentId.Value))
                roots.Add(item);
            else if (dict.TryGetValue(item.ParentId.Value, out var parent))
                parent.Children.Add(item);
        }
        return roots.OrderBy(r => r.SortOrder).ToList();
    }
}
