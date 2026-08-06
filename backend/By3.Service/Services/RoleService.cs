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

public class RoleService
{
    private readonly RoleRepository _repo;
    private readonly MenuRepository _menuRepo;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RoleService(RoleRepository repo, MenuRepository menuRepo, IHttpContextAccessor httpContextAccessor)
    {
        _repo = repo;
        _menuRepo = menuRepo;
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
    /// 分页查询角色列表。
    /// </summary>
    public async Task<PageResult<RoleListDto>> GetListAsync(int page, int pageSize, string? keyword)
    {
        var roles = await _repo.GetListAsync(page, pageSize, keyword);
        var total = await _repo.GetCountAsync(keyword);
        return new PageResult<RoleListDto>
        {
            Total = total,
            Items = roles.Select(r => new RoleListDto
            {
                Id = r.Id,
                RoleName = r.RoleName,
                Description = r.Description,
                IsEnabled = r.IsEnabled,
                CreatedAt = r.CreatedAt
            }).ToList(),
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// 获取所有角色列表。
    /// </summary>
    public async Task<List<RoleListDto>> GetAllAsync()
    {
        var roles = await _repo.GetListAsync();
        return roles.Select(r => new RoleListDto
        {
            Id = r.Id,
            RoleName = r.RoleName,
            Description = r.Description,
            IsEnabled = r.IsEnabled,
            CreatedAt = r.CreatedAt
        }).ToList();
    }

    /// <summary>
    /// 根据ID获取角色详情。
    /// </summary>
    public async Task<RoleListDto?> GetByIdAsync(Guid id)
    {
        var r = await _repo.GetByIdAsync(id);
        if (r == null) return null;
        return new RoleListDto
        {
            Id = r.Id,
            RoleName = r.RoleName,
            Description = r.Description,
            IsEnabled = r.IsEnabled,
            CreatedAt = r.CreatedAt
        };
    }

    /// <summary>
    /// 创建角色。
    /// </summary>
    public async Task<Guid> CreateAsync(CreateRoleDto dto)
    {
        var role = new SysRole
        {
            RoleName = dto.RoleName,
            Description = dto.Description,
            CreatedBy = CurrentUserId,
            CreatedAt = DateTime.UtcNow
        };
        var id = await _repo.CreateAsync(role);
        await _repo.SetRoleMenusAsync(role.Id, dto.MenuIds);
        return role.Id;
    }

    /// <summary>
    /// 更新角色信息。
    /// </summary>
    public async Task<int> UpdateAsync(UpdateRoleDto dto)
    {
        var role = await _repo.GetByIdAsync(dto.Id);
        if (role == null) return 0;

        if (dto.RoleName != null) role.RoleName = dto.RoleName;
        if (dto.Description != null) role.Description = dto.Description;
        if (dto.IsEnabled.HasValue) role.IsEnabled = dto.IsEnabled.Value;
        role.UpdatedAt = DateTime.UtcNow;
        role.UpdatedBy = CurrentUserId;

        var result = await _repo.UpdateAsync(role);
        if (dto.MenuIds.Count > 0)
            await _repo.SetRoleMenusAsync(dto.Id, dto.MenuIds);
        return result;
    }

    /// <summary>
    /// 删除角色。
    /// </summary>
    public async Task<int> DeleteAsync(Guid id)
        => await _repo.DeleteAsync(id);

    /// <summary>
    /// 获取角色关联的菜单ID列表。
    /// </summary>
    public async Task<List<Guid>> GetRoleMenuIdsAsync(Guid roleId)
        => await _repo.GetMenuIdsByRoleIdAsync(roleId);
}
