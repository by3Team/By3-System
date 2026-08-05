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

using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using By3.Api.Filters;
using By3.Service.DTOs;
using By3.Service.Services;

namespace By3.Api.Controllers;


/// <summary>
/// 角色管理：提供角色分页查询、详情、增删改及菜单分配功能。
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly RoleService _roleService;
    public RolesController(RoleService roleService) => _roleService = roleService;


    /// <summary>
    /// 获取分页列表。
    /// </summary>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="keyword">搜索关键词</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet]
    [Authorize(Policy = "role:list")]
    public async Task<IActionResult> GetList(int page = 1, int pageSize = 10, string? keyword = null)
    {
        var result = await _roleService.GetListAsync(page, pageSize, keyword);
        return Ok(ApiResult<PageResult<RoleListDto>>.Ok(result));
    }


    /// <summary>
    /// 获取。
    /// </summary>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet("all")]
    [Authorize(Policy = "role:list")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _roleService.GetAllAsync();
        return Ok(ApiResult<List<RoleListDto>>.Ok(result));
    }


    /// <summary>
    /// 根据 ID 获取详情。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "role:list")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var role = await _roleService.GetByIdAsync(id);
        if (role == null) return NotFound(ApiResult<object>.Error("角色不存在", 404));
        return Ok(ApiResult<RoleListDto>.Ok(role));
    }


    /// <summary>
    /// 创建。
    /// </summary>
    /// <param name="dto">请求数据传输对象</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPost]
    [Authorize(Policy = "role:create")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Create(CreateRoleDto dto)
    {
        var id = await _roleService.CreateAsync(dto);
        return Ok(ApiResult<object>.Ok(new { Id = id }, "创建成功"));
    }


    /// <summary>
    /// 更新。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <param name="dto">请求数据传输对象</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "role:update")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Update(Guid id, UpdateRoleDto dto)
    {
        dto.Id = id;
        var result = await _roleService.UpdateAsync(dto);
        if (result == 0) return NotFound(ApiResult<object>.Error("角色不存在", 404));
        return Ok(ApiResult<object>.Ok(null, "更新成功"));
    }


    /// <summary>
    /// 删除。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "role:delete")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _roleService.DeleteAsync(id);
        if (result == 0) return NotFound(ApiResult<object>.Error("角色不存在", 404));
        return Ok(ApiResult<object>.Ok(null, "删除成功"));
    }


    /// <summary>
    /// 获取。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet("{id}/menus")]
    [Authorize(Policy = "role:list")]
    public async Task<IActionResult> GetRoleMenus(Guid id)
    {
        var menus = await _roleService.GetRoleMenuIdsAsync(id);
        return Ok(ApiResult<List<Guid>>.Ok(menus));
    }
}
