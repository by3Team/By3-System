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
/// 对外 API 部门数据：提供经过签名认证的外部系统可访问的部门信息接口。
/// </summary>
[ApiController]
[Route("api/external/v{version:apiVersion}/departments")]
[ApiVersion("1.0")]
public class ExternalDepartmentsController : ControllerBase
{
    private readonly DepartmentService _departmentService;

    public ExternalDepartmentsController(DepartmentService departmentService)
    {
        _departmentService = departmentService;
    }


    /// <summary>
    /// 获取部门树形列表。
    /// </summary>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet]
    public async Task<IActionResult> GetTree()
    {
        var result = await _departmentService.GetTreeAsync();
        return Ok(ApiResult<List<DepartmentTreeDto>>.Ok(result));
    }


    /// <summary>
    /// 根据 ID 获取单个部门详情。
    /// </summary>
    /// <param name="id">部门唯一标识</param>
    /// <returns>ApiResult 包装的操作结果</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var dept = await _departmentService.GetByIdAsync(id);
        if (dept == null) return NotFound(ApiResult<object>.Error("部门不存在", 404));
        return Ok(ApiResult<DepartmentTreeDto>.Ok(dept));
    }
}
