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

public class DepartmentService
{
    private readonly DepartmentRepository _repo;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DepartmentService(DepartmentRepository repo, IHttpContextAccessor httpContextAccessor)
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
    /// 获取部门树形结构。
    /// </summary>
    public async Task<List<DepartmentTreeDto>> GetTreeAsync()
    {
        var all = await _repo.GetAllAsync();
        return BuildTree(all);
    }

    /// <summary>
    /// 根据ID获取部门详情。
    /// </summary>
    public async Task<DepartmentTreeDto?> GetByIdAsync(Guid id)
    {
        var dept = await _repo.GetByIdAsync(id);
        return dept == null ? null : MapToDto(dept);
    }

    /// <summary>
    /// 创建部门。
    /// </summary>
    public async Task<Guid> CreateAsync(CreateDepartmentDto dto)
    {
        var dept = new SysDepartment
        {
            DeptName = dto.DeptName,
            DeptCode = dto.DeptCode,
            ParentId = dto.ParentId,
            SortOrder = dto.SortOrder,
            CreatedBy = CurrentUserId,
            CreatedAt = DateTime.UtcNow
        };
        return await _repo.CreateAsync(dept);
    }

    /// <summary>
    /// 更新部门信息。
    /// </summary>
    public async Task<int> UpdateAsync(UpdateDepartmentDto dto)
    {
        var dept = await _repo.GetByIdAsync(dto.Id);
        if (dept == null) return 0;

        if (dto.DeptName != null) dept.DeptName = dto.DeptName;
        if (dto.DeptCode != null) dept.DeptCode = dto.DeptCode;
        if (dto.ParentId.HasValue) dept.ParentId = dto.ParentId.Value;
        if (dto.SortOrder.HasValue) dept.SortOrder = dto.SortOrder.Value;
        if (dto.IsEnabled.HasValue) dept.IsEnabled = dto.IsEnabled.Value;
        dept.UpdatedAt = DateTime.UtcNow;
        dept.UpdatedBy = CurrentUserId;

        return await _repo.UpdateAsync(dept);
    }

    /// <summary>
    /// 删除部门。
    /// </summary>
    public async Task<int> DeleteAsync(Guid id)
        => await _repo.DeleteAsync(id);

    /// <summary>
    /// 将部门实体映射为树形 DTO。
    /// </summary>
    private static DepartmentTreeDto MapToDto(SysDepartment dept) => new()
    {
        Id = dept.Id,
        DeptName = dept.DeptName,
        DeptCode = dept.DeptCode,
        ParentId = dept.ParentId,
        SortOrder = dept.SortOrder,
        IsEnabled = dept.IsEnabled,
        CreatedAt = dept.CreatedAt
    };

    /// <summary>
    /// 将扁平部门列表构建为树形结构。
    /// </summary>
    private static List<DepartmentTreeDto> BuildTree(List<SysDepartment> departments)
    {
        var dict = departments.ToDictionary(d => d.Id, d => MapToDto(d));
        var roots = new List<DepartmentTreeDto>();
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
