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
/// 对外 API Token 管理：提供对外 API Token 的生成、查询、启用禁用、重生成、删除及操作日志功能。
/// 删除为逻辑删除；已删除的 Token 不允许修改、重生成或再次删除。
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class ExternalApiTokensController : ControllerBase
{
    private readonly ExternalApiTokenService _service;

    public ExternalApiTokensController(ExternalApiTokenService service)
    {
        _service = service;
    }


    /// <summary>
    /// 获取分页列表。
    /// </summary>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="keyword">搜索关键词</param>
    /// <param name="isEnabled">启用状态：enabled/disabled，默认全部</param>
    /// <param name="includeDeleted">是否包含已删除，默认 false</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet]
    [Authorize(Policy = "externalapi:list")]
    public async Task<IActionResult> GetList(int page = 1, int pageSize = 10, string? keyword = null, string? isEnabled = null, bool? includeDeleted = null)
    {
        var result = await _service.GetListAsync(page, pageSize, keyword, isEnabled, includeDeleted);
        return Ok(ApiResult<PageResult<ExternalApiTokenDto>>.Ok(result));
    }


    /// <summary>
    /// 导出 CSV。
    /// </summary>
    /// <param name="keyword">搜索关键词</param>
    /// <param name="isEnabled">启用状态：enabled/disabled，默认全部</param>
    /// <param name="includeDeleted">是否包含已删除，默认 false</param>
    /// <returns>CSV 文件</returns>
    [HttpGet("export")]
    [Authorize(Policy = "externalapi:list")]
    public async Task<IActionResult> ExportCsv(string? keyword = null, string? isEnabled = null, bool? includeDeleted = null)
    {
        var bytes = await _service.ExportCsvAsync(keyword, isEnabled, includeDeleted);
        return File(bytes, "text/csv; charset=utf-8", $"external-api-tokens-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
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
        var token = await _service.GetByIdAsync(id);
        if (token == null) return NotFound(ApiResult<object>.Error("Token 不存在", 404));
        return Ok(ApiResult<ExternalApiTokenDto>.Ok(token));
    }


    /// <summary>
    /// 获取指定 Token 的操作日志。
    /// </summary>
    /// <param name="id">Token 唯一标识</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet("{id}/logs")]
    [Authorize(Policy = "externalapi:list")]
    public async Task<IActionResult> GetLogs(Guid id, int page = 1, int pageSize = 10)
    {
        var result = await _service.GetLogsAsync(id, page, pageSize);
        return Ok(ApiResult<PageResult<ExternalApiTokenLogDto>>.Ok(result));
    }


    /// <summary>
    /// 获取指定 Token 的历史 Secret Key 列表。
    /// </summary>
    /// <param name="id">Token 唯一标识</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="status">筛选状态：all/valid/invalid</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet("{id}/history")]
    [Authorize(Policy = "externalapi:list")]
    public async Task<IActionResult> GetHistory(Guid id, int page = 1, int pageSize = 10, string? status = null)
    {
        var result = await _service.GetHistoryAsync(id, page, pageSize, status);
        return Ok(ApiResult<PageResult<ExternalApiTokenHistoryDto>>.Ok(result));
    }


    /// <summary>
    /// 手动作废某条历史 Secret Key。
    /// </summary>
    /// <param name="id">Token 唯一标识</param>
    /// <param name="historyId">历史记录唯一标识</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPost("{id}/history/{historyId}/invalidate")]
    [Authorize(Policy = "externalapi:update")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> InvalidateHistory(Guid id, Guid historyId)
    {
        var result = await _service.InvalidateHistoryAsync(historyId);
        if (result == 0) return NotFound(ApiResult<object>.Error("历史记录不存在或已作废", 404));
        return Ok(ApiResult<object>.Ok(null, "已作废"));
    }


    /// <summary>
    /// 创建。
    /// </summary>
    /// <param name="dto">请求数据传输对象</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPost]
    [Authorize(Policy = "externalapi:create")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Create(CreateExternalApiTokenDto dto)
    {
        var token = await _service.CreateAsync(dto);
        return Ok(ApiResult<ExternalApiTokenDto>.Ok(token, "创建成功"));
    }


    /// <summary>
    /// 更新。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <param name="dto">请求数据传输对象</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "externalapi:update")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Update(Guid id, UpdateExternalApiTokenDto dto)
    {
        try
        {
            var result = await _service.UpdateAsync(id, dto);
            if (result == 0) return NotFound(ApiResult<object>.Error("Token 不存在", 404));
            return Ok(ApiResult<object>.Ok(null, "更新成功"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResult<object>.Error(ex.Message, 400));
        }
    }


    /// <summary>
    /// 删除（逻辑删除）。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "externalapi:delete")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);
            if (result == 0) return NotFound(ApiResult<object>.Error("Token 不存在", 404));
            return Ok(ApiResult<object>.Ok(null, "删除成功"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResult<object>.Error(ex.Message, 400));
        }
    }


    /// <summary>
    /// 重新生成 Key/Secret。
    /// 可选择旧 Key 立即失效或在指定时间后失效（缓冲期）。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <param name="dto">重生成参数</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPost("{id}/regenerate")]
    [Authorize(Policy = "externalapi:update")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Regenerate(Guid id, [FromBody] RegenerateExternalApiTokenDto dto)
    {
        try
        {
            var token = await _service.RegenerateAsync(id, dto);
            if (token == null) return NotFound(ApiResult<object>.Error("Token 不存在", 404));
            return Ok(ApiResult<ExternalApiTokenDto>.Ok(token, "已重新生成 Key/Secret"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResult<object>.Error(ex.Message, 400));
        }
    }
}
