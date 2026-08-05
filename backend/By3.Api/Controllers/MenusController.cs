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
/// 菜单管理：提供菜单树查询、详情、增删改功能。
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class MenusController : ControllerBase
{
    private readonly MenuService _menuService;
    public MenusController(MenuService menuService) => _menuService = menuService;


    /// <summary>
    /// 获取。
    /// </summary>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet]
    [Authorize(Policy = "menu:list")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _menuService.GetAllAsync();
        return Ok(ApiResult<List<MenuTreeDto>>.Ok(result));
    }


    /// <summary>
    /// 根据 ID 获取详情。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "menu:list")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var menu = await _menuService.GetByIdAsync(id);
        if (menu == null) return NotFound(ApiResult<object>.Error("菜单不存在", 404));
        return Ok(ApiResult<MenuTreeDto>.Ok(menu));
    }


    /// <summary>
    /// 创建。
    /// </summary>
    /// <param name="dto">请求数据传输对象</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPost]
    [Authorize(Policy = "menu:create")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Create(CreateMenuDto dto)
    {
        var id = await _menuService.CreateAsync(dto);
        return Ok(ApiResult<object>.Ok(new { Id = id }, "创建成功"));
    }


    /// <summary>
    /// 更新。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <param name="dto">请求数据传输对象</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "menu:update")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Update(Guid id, UpdateMenuDto dto)
    {
        dto.Id = id;
        var result = await _menuService.UpdateAsync(dto);
        if (result == 0) return NotFound(ApiResult<object>.Error("菜单不存在", 404));
        return Ok(ApiResult<object>.Ok(null, "更新成功"));
    }


    /// <summary>
    /// 删除。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "menu:delete")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _menuService.DeleteAsync(id);
        if (result == 0) return NotFound(ApiResult<object>.Error("菜单不存在", 404));
        return Ok(ApiResult<object>.Ok(null, "删除成功"));
    }
}
