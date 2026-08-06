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

public class DictTypeService
{
    private readonly DictTypeRepository _repo;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DictTypeService(DictTypeRepository repo, IHttpContextAccessor httpContextAccessor)
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
    /// 分页查询字典类型列表
    /// </summary>
    public async Task<PageResult<DictTypeListDto>> GetListAsync(int page, int pageSize, string? keyword)
    {
        var items = await _repo.GetListAsync(page, pageSize, keyword);
        var total = await _repo.GetCountAsync(keyword);
        return new PageResult<DictTypeListDto>
        {
            Total = total,
            Items = items.Select(MapToDto).ToList(),
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// 根据ID获取字典类型
    /// </summary>
    public async Task<DictTypeListDto?> GetByIdAsync(Guid id)
    {
        var t = await _repo.GetByIdAsync(id);
        return t == null ? null : MapToDto(t);
    }

    /// <summary>
    /// 创建字典类型
    /// </summary>
    public async Task<Guid> CreateAsync(CreateDictTypeDto dto)
    {
        var type = new SysDictType
        {
            DictName = dto.DictName,
            DictType = dto.DictType,
            CreatedBy = CurrentUserId,
            CreatedAt = DateTime.UtcNow
        };
        return await _repo.CreateAsync(type);
    }

    /// <summary>
    /// 更新字典类型
    /// </summary>
    public async Task<int> UpdateAsync(UpdateDictTypeDto dto)
    {
        var type = await _repo.GetByIdAsync(dto.Id);
        if (type == null) return 0;

        if (dto.DictName != null) type.DictName = dto.DictName;
        if (dto.DictType != null) type.DictType = dto.DictType;
        if (dto.IsEnabled.HasValue) type.IsEnabled = dto.IsEnabled.Value;
        type.UpdatedAt = DateTime.UtcNow;
        type.UpdatedBy = CurrentUserId;

        return await _repo.UpdateAsync(type);
    }

    /// <summary>
    /// 删除字典类型
    /// </summary>
    public async Task<int> DeleteAsync(Guid id)
        => await _repo.DeleteAsync(id);

    private static DictTypeListDto MapToDto(SysDictType t) => new()
    {
        Id = t.Id,
        DictName = t.DictName,
        DictType = t.DictType,
        IsEnabled = t.IsEnabled,
        CreatedAt = t.CreatedAt
    };
}
