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

public class DictDataService
{
    private readonly DictDataRepository _repo;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DictDataService(DictDataRepository repo, IHttpContextAccessor httpContextAccessor)
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
    /// 分页查询字典数据列表
    /// </summary>
    public async Task<PageResult<DictDataListDto>> GetListAsync(int page, int pageSize, Guid? dictTypeId)
    {
        var items = await _repo.GetListAsync(page, pageSize, dictTypeId);
        var total = await _repo.GetCountAsync(dictTypeId);
        return new PageResult<DictDataListDto>
        {
            Total = total,
            Items = items.Select(MapToDto).ToList(),
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// 根据类型ID获取字典数据列表
    /// </summary>
    public async Task<List<DictDataListDto>> GetByTypeIdAsync(Guid dictTypeId)
    {
        var items = await _repo.GetByTypeIdAsync(dictTypeId);
        return items.Select(MapToDto).ToList();
    }

    /// <summary>
    /// 根据类型编码获取字典数据列表
    /// </summary>
    public async Task<List<DictDataListDto>> GetByTypeCodeAsync(string dictTypeCode)
    {
        var items = await _repo.GetByTypeCodeAsync(dictTypeCode);
        return items.Select(MapToDto).ToList();
    }

    /// <summary>
    /// 根据ID获取字典数据
    /// </summary>
    public async Task<DictDataListDto?> GetByIdAsync(Guid id)
    {
        var d = await _repo.GetByIdAsync(id);
        return d == null ? null : MapToDto(d);
    }

    /// <summary>
    /// 创建字典数据
    /// </summary>
    public async Task<Guid> CreateAsync(CreateDictDataDto dto)
    {
        var data = new SysDictData
        {
            DictTypeId = dto.DictTypeId,
            DictLabel = dto.DictLabel,
            DictValue = dto.DictValue,
            Remark = dto.Remark,
            SortOrder = dto.SortOrder,
            IsDefault = dto.IsDefault,
            CreatedBy = CurrentUserId,
            CreatedAt = DateTime.UtcNow
        };
        return await _repo.CreateAsync(data);
    }

    /// <summary>
    /// 更新字典数据
    /// </summary>
    public async Task<int> UpdateAsync(UpdateDictDataDto dto)
    {
        var data = await _repo.GetByIdAsync(dto.Id);
        if (data == null) return 0;

        if (dto.DictTypeId.HasValue) data.DictTypeId = dto.DictTypeId.Value;
        if (dto.DictLabel != null) data.DictLabel = dto.DictLabel;
        if (dto.DictValue != null) data.DictValue = dto.DictValue;
        if (dto.Remark != null) data.Remark = dto.Remark;
        if (dto.SortOrder.HasValue) data.SortOrder = dto.SortOrder.Value;
        if (dto.IsDefault.HasValue) data.IsDefault = dto.IsDefault.Value;
        if (dto.IsEnabled.HasValue) data.IsEnabled = dto.IsEnabled.Value;
        data.UpdatedAt = DateTime.UtcNow;
        data.UpdatedBy = CurrentUserId;

        return await _repo.UpdateAsync(data);
    }

    /// <summary>
    /// 删除字典数据
    /// </summary>
    public async Task<int> DeleteAsync(Guid id)
        => await _repo.DeleteAsync(id);

    private static DictDataListDto MapToDto(SysDictData d) => new()
    {
        Id = d.Id,
        DictTypeId = d.DictTypeId,
        DictLabel = d.DictLabel,
        DictValue = d.DictValue,
        Remark = d.Remark,
        SortOrder = d.SortOrder,
        IsDefault = d.IsDefault,
        IsEnabled = d.IsEnabled,
        CreatedAt = d.CreatedAt
    };
}
