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
/// 邮件发送设置：提供邮件服务端配置的查询与更新功能。
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class EmailSettingsController : ControllerBase
{
    private readonly EmailSettingService _service;

    public EmailSettingsController(EmailSettingService service)
    {
        _service = service;
    }


    /// <summary>
    /// 获取。
    /// </summary>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet]
    [Authorize(Policy = "email:list")]
    public async Task<IActionResult> Get()
    {
        var result = await _service.GetAsync();
        return Ok(ApiResult<EmailSettingDto>.Ok(result));
    }


    /// <summary>
    /// 更新。
    /// </summary>
    /// <param name="dto">请求数据传输对象</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPut]
    [Authorize(Policy = "email:update")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> Update(EmailSettingDto dto)
    {
        await _service.SaveAsync(dto);
        return Ok(ApiResult<object>.Ok(null, "保存成功"));
    }


    /// <summary>
    /// 测试邮件发送端连接，仅连接并验证，不发送邮件。
    /// </summary>
    /// <param name="dto">邮件发送端配置</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPost("test")]
    [Authorize(Policy = "email:update")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> TestConnection(EmailSettingDto dto)
    {
        var (success, message) = await _service.TestConnectionAsync(dto);
        if (!success)
            return BadRequest(ApiResult<object>.Error(message, 400));
        return Ok(ApiResult<object>.Ok(null, message));
    }
}
