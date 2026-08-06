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

public class PositionService
{
    private readonly PositionRepository _repo;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PositionService(PositionRepository repo, IHttpContextAccessor httpContextAccessor)
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
    /// 分页查询岗位列表。
    /// </summary>
    public async Task<PageResult<PositionListDto>> GetListAsync(int page, int pageSize, string? keyword)
    {
        var items = await _repo.GetListAsync(page, pageSize, keyword);
        var total = await _repo.GetCountAsync(keyword);
        return new PageResult<PositionListDto>
        {
            Total = total,
            Items = items.Select(MapToDto).ToList(),
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// 根据ID获取岗位详情。
    /// </summary>
    public async Task<PositionListDto?> GetByIdAsync(Guid id)
    {
        var p = await _repo.GetByIdAsync(id);
        return p == null ? null : MapToDto(p);
    }

    /// <summary>
    /// 创建岗位。
    /// </summary>
    public async Task<Guid> CreateAsync(CreatePositionDto dto)
    {
        var position = new SysPosition
        {
            PositionName = dto.PositionName,
            PositionCode = dto.PositionCode,
            SortOrder = dto.SortOrder,
            CreatedBy = CurrentUserId,
            CreatedAt = DateTime.UtcNow
        };
        return await _repo.CreateAsync(position);
    }

    /// <summary>
    /// 更新岗位信息。
    /// </summary>
    public async Task<int> UpdateAsync(UpdatePositionDto dto)
    {
        var position = await _repo.GetByIdAsync(dto.Id);
        if (position == null) return 0;

        if (dto.PositionName != null) position.PositionName = dto.PositionName;
        if (dto.PositionCode != null) position.PositionCode = dto.PositionCode;
        if (dto.SortOrder.HasValue) position.SortOrder = dto.SortOrder.Value;
        if (dto.IsEnabled.HasValue) position.IsEnabled = dto.IsEnabled.Value;
        position.UpdatedAt = DateTime.UtcNow;
        position.UpdatedBy = CurrentUserId;

        return await _repo.UpdateAsync(position);
    }

    /// <summary>
    /// 删除岗位。
    /// </summary>
    public async Task<int> DeleteAsync(Guid id)
        => await _repo.DeleteAsync(id);

    private static PositionListDto MapToDto(SysPosition p) => new()
    {
        Id = p.Id,
        PositionName = p.PositionName,
        PositionCode = p.PositionCode,
        SortOrder = p.SortOrder,
        IsEnabled = p.IsEnabled,
        CreatedAt = p.CreatedAt
    };
}
