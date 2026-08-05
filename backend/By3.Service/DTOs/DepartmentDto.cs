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

namespace By3.Service.DTOs;

/// <summary>
/// 创建部门请求。
/// </summary>
public class CreateDepartmentDto
{
    public string DeptName { get; set; } = string.Empty;
    public string? DeptCode { get; set; }
    public Guid? ParentId { get; set; }
    public int SortOrder { get; set; } = 0;
}

/// <summary>
/// 更新部门请求。
/// </summary>
public class UpdateDepartmentDto
{
    public Guid Id { get; set; }
    public string? DeptName { get; set; }
    public string? DeptCode { get; set; }
    public Guid? ParentId { get; set; }
    public int? SortOrder { get; set; }
    public bool? IsEnabled { get; set; }
}

/// <summary>
/// 部门树形结构响应。
/// </summary>
public class DepartmentTreeDto
{
    public Guid Id { get; set; }
    public string DeptName { get; set; } = string.Empty;
    public string? DeptCode { get; set; }
    public Guid? ParentId { get; set; }
    public int SortOrder { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<DepartmentTreeDto> Children { get; set; } = new();
}
