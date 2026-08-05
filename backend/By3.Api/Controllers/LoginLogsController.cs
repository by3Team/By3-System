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
using By3.Service.DTOs;
using By3.Service.Services;

namespace By3.Api.Controllers;


/// <summary>
/// 登录日志查询：提供登录日志分页查询及详情查看功能。
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class LoginLogsController : ControllerBase
{
    private readonly LoginLogService _service;
    public LoginLogsController(LoginLogService service) => _service = service;

    /// <summary>
    /// 获取分页列表。
    /// </summary>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="userName">userName</param>
    /// <param name="isSuccess">isSuccess</param>
    /// <param name="keyword">搜索关键词</param>
    /// <param name="startTime">startTime</param>
    /// <param name="endTime">endTime</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet]
    [Authorize(Policy = "loginlog:list")]
    public async Task<IActionResult> GetList(
        int page = 1,
        int pageSize = 20,
        string? userName = null,
        bool? isSuccess = null,
        string? keyword = null,
        DateTime? startTime = null,
        DateTime? endTime = null)
    {
        var query = new LoginLogQueryDto
        {
            UserName = userName,
            IsSuccess = isSuccess,
            Keyword = keyword,
            StartTime = startTime,
            EndTime = endTime
        };
        var result = await _service.GetListAsync(page, pageSize, query);
        return Ok(ApiResult<PageResult<LoginLogDto>>.Ok(result));
    }
}
