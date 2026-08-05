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
/// 字典数据管理：提供字典数据分页查询、详情、增删改功能。
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class DictDataController : ControllerBase
{
    private readonly DictDataService _service;
    public DictDataController(DictDataService service) => _service = service;


    /// <summary>
    /// 获取分页列表。
    /// </summary>
    /// <param name="dictTypeId">dictTypeId</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet]
    [Authorize(Policy = "dict:list")]
    public async Task<IActionResult> GetList(Guid? dictTypeId = null, int page = 1, int pageSize = 10)
    {
        var result = await _service.GetListAsync(page, pageSize, dictTypeId);
        return Ok(ApiResult<PageResult<DictDataListDto>>.Ok(result));
    }


    /// <summary>
    /// 获取。
    /// </summary>
    /// <param name="dictTypeId">dictTypeId</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet("by-type/{dictTypeId}")]
    [Authorize(Policy = "dict:list")]
    public async Task<IActionResult> GetByTypeId(Guid dictTypeId)
    {
        var result = await _service.GetByTypeIdAsync(dictTypeId);
        return Ok(ApiResult<List<DictDataListDto>>.Ok(result));
    }


    /// <summary>
    /// 获取。
    /// </summary>
    /// <param name="dictTypeCode">dictTypeCode</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet("by-type-code/{dictTypeCode}")]
    [Authorize(Policy = "dict:list")]
    public async Task<IActionResult> GetByTypeCode(string dictTypeCode)
    {
        var result = await _service.GetByTypeCodeAsync(dictTypeCode);
        return Ok(ApiResult<List<DictDataListDto>>.Ok(result));
    }


    /// <summary>
    /// 根据 ID 获取详情。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "dict:list")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var d = await _service.GetByIdAsync(id);
        if (d == null) return NotFound(ApiResult<object>.Error("字典数据不存在", 404));
        return Ok(ApiResult<DictDataListDto>.Ok(d));
    }


    /// <summary>
    /// 创建。
    /// </summary>
    /// <param name="dto">请求数据传输对象</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPost]
    [Authorize(Policy = "dict:create")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Create(CreateDictDataDto dto)
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
    [Authorize(Policy = "dict:update")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Update(Guid id, UpdateDictDataDto dto)
    {
        dto.Id = id;
        var result = await _service.UpdateAsync(dto);
        if (result == 0) return NotFound(ApiResult<object>.Error("字典数据不存在", 404));
        return Ok(ApiResult<object>.Ok(null, "更新成功"));
    }


    /// <summary>
    /// 删除。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "dict:delete")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        if (result == 0) return NotFound(ApiResult<object>.Error("字典数据不存在", 404));
        return Ok(ApiResult<object>.Ok(null, "删除成功"));
    }
}
