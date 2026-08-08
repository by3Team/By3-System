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
/// 部门管理：提供组织机构树查询、详情、增删改功能。
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly DepartmentService _service;
    public DepartmentsController(DepartmentService service) => _service = service;


    /// <summary>
    /// 获取。
    /// </summary>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet]
    [Authorize(Policy = "dept:list")]
    public async Task<IActionResult> GetTree()
    {
        var result = await _service.GetTreeAsync();
        return Ok(ApiResult<List<DepartmentTreeDto>>.Ok(result));
    }


    /// <summary>
    /// 根据 ID 获取详情。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "dept:list")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var dept = await _service.GetByIdAsync(id);
        if (dept == null) return NotFound(ApiResult<object>.Error("部门不存在", 404));
        return Ok(ApiResult<DepartmentTreeDto>.Ok(dept));
    }


    /// <summary>
    /// 创建。
    /// </summary>
    /// <param name="dto">请求数据传输对象</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPost]
    [Authorize(Policy = "dept:create")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Create(CreateDepartmentDto dto)
    {
        var id = await _service.CreateAsync(dto);
        return Ok(ApiResult<object>.Ok(new { Id = id }, "创建成功"));
    }


    /// <summary>
    /// 更新。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <param name="dto">请求数据传输对象</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "dept:update")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Update(Guid id, UpdateDepartmentDto dto)
    {
        dto.Id = id;
        var result = await _service.UpdateAsync(dto);
        if (result == 0) return NotFound(ApiResult<object>.Error("部门不存在", 404));
        return Ok(ApiResult<object>.Ok(null, "更新成功"));
    }


    /// <summary>
    /// 删除。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "dept:delete")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Delete(Guid id)
    {
        var error = await _service.DeleteAsync(id);
        if (error != null) return BadRequest(ApiResult<object>.Error(error, 400));
        return Ok(ApiResult<object>.Ok(null, "删除成功"));
    }
}
