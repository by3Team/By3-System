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
using Microsoft.AspNetCore.Mvc;
using By3.Service.DTOs;
using By3.Service.Services;

namespace By3.Api.Controllers.External;


/// <summary>
/// 对外 API 用户数据：提供经过签名认证的外部系统可访问的用户数据接口。
/// </summary>
[ApiController]
[Route("api/external/v{version:apiVersion}/users")]
[ApiVersion("1.0")]
public class ExternalUsersController : ControllerBase
{
    private readonly UserService _userService;

    public ExternalUsersController(UserService userService)
    {
        _userService = userService;
    }


    /// <summary>
    /// 获取分页列表。
    /// </summary>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="keyword">搜索关键词</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet]
    public async Task<IActionResult> GetList(int page = 1, int pageSize = 10, string? keyword = null)
    {
        var result = await _userService.GetListAsync(page, pageSize, keyword);
        return Ok(ApiResult<PageResult<UserListDto>>.Ok(result));
    }
}
