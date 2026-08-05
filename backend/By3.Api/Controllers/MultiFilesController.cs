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
/// 多文件上传：提供多文件上传、列表查询、下载、删除及导出功能。
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class MultiFilesController : ControllerBase
{
    private readonly IFileService _fileService;

    public MultiFilesController(IFileService fileService)
    {
        _fileService = fileService;
    }


    /// <summary>
    /// 上传文件。
    /// </summary>
    /// <param name="files">上传的文件列表</param>
    /// <param name="category">文件分类</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPost("upload")]
    [Authorize(Policy = "file:create")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Upload(List<IFormFile> files, string category = "general")
    {
        if (files == null || files.Count == 0)
            return BadRequest(ApiResult<object>.Error("请选择文件", 400));

        var userId = GetCurrentUserId();
        var results = await _fileService.UploadMultipleAsync(files, category, userId);
        return Ok(ApiResult<List<FileUploadResultDto>>.Ok(results, "上传成功"));
    }


    /// <summary>
    /// 获取分页列表。
    /// </summary>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="keyword">搜索关键词</param>
    /// <param name="category">文件分类</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet]
    [Authorize(Policy = "file:list")]
    public async Task<IActionResult> GetList(int page = 1, int pageSize = 10, string? keyword = null, string? category = null)
    {
        var result = await _fileService.GetListAsync(page, pageSize, keyword, category);
        return Ok(ApiResult<PageResult<FileRecordDto>>.Ok(result));
    }


    /// <summary>
    /// 下载文件。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet("{id}/download")]
    [Authorize(Policy = "file:list")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Download(Guid id)
    {
        var record = await _fileService.GetByIdAsync(id);
        if (record == null) return NotFound(ApiResult<object>.Error("文件不存在", 404));

        var stream = await _fileService.GetFileStreamAsync(id);
        if (stream == null) return NotFound(ApiResult<object>.Error("文件已丢失", 404));

        return File(stream, record.ContentType, record.OriginalFileName);
    }


    /// <summary>
    /// 删除。
    /// </summary>
    /// <param name="id">记录唯一标识</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "file:delete")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _fileService.DeleteAsync(id);
        if (result == 0) return NotFound(ApiResult<object>.Error("文件不存在", 404));
        return Ok(ApiResult<object>.Ok(null, "删除成功"));
    }


    /// <summary>
    /// 导出 Excel。
    /// </summary>
    /// <param name="category">文件分类</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet("export")]
    [Authorize(Policy = "file:list")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> ExportExcel(string? category = null)
    {
        var bytes = await _fileService.ExportExcelAsync(category);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"files_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
    }

    private Guid? GetCurrentUserId()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userId, out var id) ? id : null;
    }
}
