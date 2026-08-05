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
/// 系统信息：提供系统版本、依赖包列表等运行信息查询功能。
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class SystemInfoController : ControllerBase
{
    private readonly SystemInfoService _service;

    public SystemInfoController(SystemInfoService service)
    {
        _service = service;
    }

    /// <summary>
    /// 获取系统依赖包列表。
    /// </summary>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet("packages")]
    [Authorize(Policy = "setting:list")]
    public IActionResult GetPackages()
    {
        var result = _service.GetPackages();
        return Ok(ApiResult<SystemPackagesDto>.Ok(result));
    }
}
