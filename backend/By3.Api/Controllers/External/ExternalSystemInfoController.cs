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
/// 对外 API 系统信息：提供经过签名认证的外部系统可访问的系统信息接口。
/// </summary>
[ApiController]
[Route("api/external/v{version:apiVersion}/systeminfo")]
[ApiVersion("1.0")]
public class ExternalSystemInfoController : ControllerBase
{
    private readonly SystemInfoService _systemInfoService;

    public ExternalSystemInfoController(SystemInfoService systemInfoService)
    {
        _systemInfoService = systemInfoService;
    }

    /// <summary>
    /// 获取系统依赖包列表（外部 API）。
    /// </summary>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet("packages")]
    public IActionResult GetPackages()
    {
        var result = _systemInfoService.GetPackages();
        return Ok(ApiResult<object>.Ok(result));
    }
}
