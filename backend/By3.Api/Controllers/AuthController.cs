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
using Microsoft.AspNetCore.RateLimiting;
using By3.Api.Filters;
using By3.Service.DTOs;
using By3.Service.Services;

namespace By3.Api.Controllers;


/// <summary>
/// 认证授权：提供登录、刷新 Token、登出及获取当前用户信息功能。
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[EnableRateLimiting("default")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    public AuthController(AuthService authService) => _authService = authService;


    /// <summary>
    /// 登录并获取 Token。
    /// </summary>
    /// <param name="dto">请求数据传输对象</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPost("login")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto, HttpContext.Connection.RemoteIpAddress?.ToString());
        if (result == null)
            return BadRequest(ApiResult<object>.Error("用户名或密码错误", 400));
        return Ok(ApiResult<LoginResultDto>.Ok(result));
    }


    /// <summary>
    /// 刷新访问 Token。
    /// </summary>
    /// <param name="dto">请求数据传输对象</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPost("refresh")]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> RefreshToken(RefreshTokenDto dto)
    {
        var result = await _authService.RefreshTokenAsync(dto.RefreshToken);
        if (result == null)
            return BadRequest(ApiResult<object>.Error("刷新令牌无效或已过期", 400));
        return Ok(ApiResult<LoginResultDto>.Ok(result));
    }

    /// <summary>
    /// 登出，将当前 Token 加入黑名单。
    /// </summary>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader.Substring("Bearer ".Length);
            _authService.BlacklistToken(token);
        }
        return Ok(ApiResult<object>.Ok(null, "登出成功"));
    }


    /// <summary>
    /// 获取。
    /// </summary>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet("info")]
    [Authorize]
    public async Task<IActionResult> GetInfo()
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var permissions = await _authService.GetUserPermissionsAsync(userId);
        var menus = await _authService.GetUserMenusWithTreeAsync(userId);
        return Ok(ApiResult<object>.Ok(new
        {
            UserId = userId,
            UserName = User.Identity!.Name,
            RealName = User.FindFirst("realName")?.Value,
            Permissions = permissions,
            Menus = menus
        }));
    }

    /// <summary>
    /// 修改当前用户密码。
    /// </summary>
    /// <param name="dto">请求数据传输对象</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpPost("change-password")]
    [Authorize]
    [ServiceFilter(typeof(IdempotencyFilter))]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var result = await _authService.ChangePasswordAsync(userId, dto.OldPassword, dto.NewPassword);
        if (!result)
            return BadRequest(ApiResult<object>.Error("旧密码错误", 400));
        return Ok(ApiResult<object>.Ok(null, "密码修改成功"));
    }
}
