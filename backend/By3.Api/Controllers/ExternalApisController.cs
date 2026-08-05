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
/// 对外 API 接口管理：维护允许被外部 AK/SK 访问的接口清单，包含限流与幂等配置。
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class ExternalApisController : ControllerBase
{
    private readonly ExternalApiService _service;

    public ExternalApisController(ExternalApiService service)
    {
        _service = service;
    }


    /// <summary>
    /// 获取分页列表。
    /// </summary>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="keyword">搜索关键词</param>
    /// <param name="isEnabled">状态：enabled/disabled，默认全部</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet]
    [Authorize(Policy = "externalapi:list")]
    public async Task<IActionResult> GetList(int page = 1, int pageSize = 10, string? keyword = null, string? isEnabled = null)
    {
        var result = await _service.GetListAsync(page, pageSize, keyword, isEnabled);
        return Ok(ApiResult<PageResult<ExternalApiDto>>.Ok(result));
    }


    /// <summary>
    /// 根据 ID 获取详情。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "externalapi:list")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var api = await _service.GetByIdAsync(id);
        if (api == null) return NotFound(ApiResult<object>.Error("接口不存在", 404));
        return Ok(ApiResult<ExternalApiDto>.Ok(api));
    }


    /// <summary>
    /// 创建对外接口。
    /// </summary>
    /// <param name="dto">请求数据传输对象</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPost]
    [Authorize(Policy = "externalapi:create")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Create(CreateExternalApiDto dto)
    {
        var id = await _service.CreateAsync(dto);
        return Ok(ApiResult<object>.Ok(new { Id = id }, "创建成功"));
    }


    /// <summary>
    /// 更新对外接口。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <param name="dto">请求数据传输对象</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "externalapi:update")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Update(Guid id, UpdateExternalApiDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        if (result == 0) return NotFound(ApiResult<object>.Error("接口不存在", 404));
        return Ok(ApiResult<object>.Ok(null, "更新成功"));
    }


    /// <summary>
    /// 删除对外接口。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "externalapi:delete")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        if (result == 0) return NotFound(ApiResult<object>.Error("接口不存在", 404));
        return Ok(ApiResult<object>.Ok(null, "删除成功"));
    }


    /// <summary>
    /// 获取对外接口统计信息：近 30 天请求量曲线、成功/失败数、最近调用时间、已授权 Token 列表。
    /// </summary>
    /// <param name="id">接口唯一标识</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet("{id}/stats")]
    [Authorize(Policy = "externalapi:list")]
    public async Task<IActionResult> GetStats(Guid id)
    {
        var stats = await _service.GetStatsAsync(id);
        if (stats == null) return NotFound(ApiResult<object>.Error("接口不存在", 404));
        return Ok(ApiResult<ExternalApiStatsDto>.Ok(stats));
    }


    /// <summary>
    /// 获取已授权指定接口的 Token 数量。
    /// </summary>
    /// <param name="id">接口唯一标识</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet("{id}/authorized-token-count")]
    [Authorize(Policy = "externalapi:list")]
    public async Task<IActionResult> GetAuthorizedTokenCount(Guid id)
    {
        var count = await _service.GetAuthorizedTokenCountAsync(id);
        return Ok(ApiResult<int>.Ok(count));
    }


    /// <summary>
    /// 切换接口启用/停用状态，返回切换后的状态及受影响的 Token 数量。
    /// </summary>
    /// <param name="id">接口唯一标识</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPost("{id}/toggle")]
    [Authorize(Policy = "externalapi:update")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        try
        {
            var (newStatus, affectedCount) = await _service.ToggleStatusAsync(id);
            var action = newStatus ? "启用" : "停用";
            return Ok(ApiResult<object>.Ok(new { isEnabled = newStatus, affectedTokenCount = affectedCount }, $"{action}成功"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResult<object>.Error(ex.Message, 400));
        }
    }
}
