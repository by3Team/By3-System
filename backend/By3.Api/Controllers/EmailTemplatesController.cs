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
/// 邮件模板管理：提供邮件模板分页查询、详情、增删改及测试发送功能。
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class EmailTemplatesController : ControllerBase
{
    private readonly EmailService _service;

    public EmailTemplatesController(EmailService service)
    {
        _service = service;
    }


    /// <summary>
    /// 获取分页列表。
    /// </summary>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="keyword">搜索关键词</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet]
    [Authorize(Policy = "email:list")]
    public async Task<IActionResult> GetList(int page = 1, int pageSize = 10, string? keyword = null)
    {
        var result = await _service.GetTemplateListAsync(page, pageSize, keyword);
        return Ok(ApiResult<PageResult<EmailTemplateDto>>.Ok(result));
    }


    /// <summary>
    /// 根据 ID 获取详情。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "email:list")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var template = await _service.GetTemplateByIdAsync(id);
        if (template == null) return NotFound(ApiResult<object>.Error("模板不存在", 404));
        return Ok(ApiResult<EmailTemplateDto>.Ok(template));
    }


    /// <summary>
    /// 创建。
    /// </summary>
    /// <param name="dto">请求数据传输对象</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPost]
    [Authorize(Policy = "email:create")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Create(CreateEmailTemplateDto dto)
    {
        var id = await _service.CreateTemplateAsync(dto, GetCurrentUserId());
        return Ok(ApiResult<object>.Ok(new { Id = id }, "创建成功"));
    }


    /// <summary>
    /// 更新。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <param name="dto">请求数据传输对象</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "email:update")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Update(Guid id, UpdateEmailTemplateDto dto)
    {
        dto.Id = id;
        var result = await _service.UpdateTemplateAsync(dto);
        if (result == 0) return NotFound(ApiResult<object>.Error("模板不存在", 404));
        return Ok(ApiResult<object>.Ok(null, "更新成功"));
    }


    /// <summary>
    /// 删除。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "email:delete")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteTemplateAsync(id);
        if (result == 0) return NotFound(ApiResult<object>.Error("模板不存在", 404));
        return Ok(ApiResult<object>.Ok(null, "删除成功"));
    }


    /// <summary>
    /// 获取。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet("{id}/versions")]
    [Authorize(Policy = "email:list")]
    public async Task<IActionResult> GetVersions(Guid id)
    {
        var versions = await _service.GetVersionsByTemplateIdAsync(id);
        return Ok(ApiResult<List<EmailTemplateVersionDto>>.Ok(versions));
    }


    /// <summary>
    /// 创建。
    /// </summary>
    /// <param name="dto">请求数据传输对象</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPost("versions")]
    [Authorize(Policy = "email:create")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> CreateVersion(CreateEmailTemplateVersionDto dto)
    {
        var id = await _service.CreateVersionAsync(dto, GetCurrentUserId());
        return Ok(ApiResult<object>.Ok(new { Id = id }, "创建成功"));
    }


    /// <summary>
    /// 更新。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <param name="dto">请求数据传输对象</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPut("versions/{id}")]
    [Authorize(Policy = "email:update")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> UpdateVersion(Guid id, UpdateEmailTemplateVersionDto dto)
    {
        dto.Id = id;
        var result = await _service.UpdateVersionAsync(dto);
        if (result == 0) return NotFound(ApiResult<object>.Error("版本不存在", 404));
        return Ok(ApiResult<object>.Ok(null, "更新成功"));
    }


    /// <summary>
    /// 删除。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpDelete("versions/{id}")]
    [Authorize(Policy = "email:delete")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> DeleteVersion(Guid id)
    {
        var result = await _service.DeleteVersionAsync(id);
        if (result == 0) return NotFound(ApiResult<object>.Error("版本不存在", 404));
        return Ok(ApiResult<object>.Ok(null, "删除成功"));
    }


    /// <summary>
    /// 发送邮件。
    /// </summary>
    /// <param name="dto">请求数据传输对象</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPost("send")]
    [Authorize(Policy = "email:send")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Send(SendEmailDto dto)
    {
        await _service.SendBatchAsync(dto);
        return Ok(ApiResult<object>.Ok(null, "邮件已加入发送队列"));
    }


    /// <summary>
    /// Test。
    /// </summary>
    /// <param name="dto">请求数据传输对象</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPost("test")]
    [Authorize(Policy = "email:send")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Test(TestEmailDto dto)
    {
        await _service.SendTestAsync(dto);
        return Ok(ApiResult<object>.Ok(null, "测试邮件已发送"));
    }


    /// <summary>
    /// 获取。
    /// </summary>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="keyword">搜索关键词</param>
    /// <param name="status">status</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet("logs")]
    [Authorize(Policy = "email:list")]
    public async Task<IActionResult> GetLogs(int page = 1, int pageSize = 10, string? keyword = null, string? status = null)
    {
        var result = await _service.GetLogListAsync(page, pageSize, keyword, status);
        return Ok(ApiResult<PageResult<EmailLogDto>>.Ok(result));
    }

    private Guid? GetCurrentUserId()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userId, out var id) ? id : null;
    }
}
