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
/// 对外 API 岗位数据：提供经过签名认证的外部系统可访问的岗位信息接口。
/// </summary>
[ApiController]
[Route("api/external/v{version:apiVersion}/positions")]
[ApiVersion("1.0")]
public class ExternalPositionsController : ControllerBase
{
    private readonly PositionService _positionService;

    public ExternalPositionsController(PositionService positionService)
    {
        _positionService = positionService;
    }


    /// <summary>
    /// 获取岗位分页列表。
    /// </summary>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="keyword">搜索关键词</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet]
    public async Task<IActionResult> GetList(int page = 1, int pageSize = 10, string? keyword = null)
    {
        var result = await _positionService.GetListAsync(page, pageSize, keyword);
        return Ok(ApiResult<PageResult<PositionListDto>>.Ok(result));
    }


    /// <summary>
    /// 根据 ID 获取单个岗位详情。
    /// </summary>
    /// <param name="id">岗位唯一标识</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var position = await _positionService.GetByIdAsync(id);
        if (position == null) return NotFound(ApiResult<object>.Error("岗位不存在", 404));
        return Ok(ApiResult<PositionListDto>.Ok(position));
    }
}
