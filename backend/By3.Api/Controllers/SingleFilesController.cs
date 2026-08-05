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
/// 单文件上传：提供单文件上传、下载、删除及导出功能。
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class SingleFilesController : ControllerBase
{
    private readonly IFileService _fileService;

    public SingleFilesController(IFileService fileService)
    {
        _fileService = fileService;
    }


    /// <summary>
    /// 上传文件。
    /// </summary>
    /// <param name="file">上传的文件</param>
    /// <param name="category">文件分类</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPost("upload")]
    [Authorize(Policy = "file:create")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Upload(IFormFile file, string category = "general")
    {
        var userId = GetCurrentUserId();
        var result = await _fileService.UploadAsync(file, category, "single", userId);
        return Ok(ApiResult<FileUploadResultDto>.Ok(result, "上传成功"));
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

    private Guid? GetCurrentUserId()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userId, out var id) ? id : null;
    }
}
