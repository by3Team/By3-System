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
/// 创建字典类型请求。
/// </summary>
public class CreateDictTypeDto
{
    public string DictName { get; set; } = string.Empty;
    public string DictType { get; set; } = string.Empty;
}

/// <summary>
/// 更新字典类型请求。
/// </summary>
public class UpdateDictTypeDto
{
    public Guid Id { get; set; }
    public string? DictName { get; set; }
    public string? DictType { get; set; }
    public bool? IsEnabled { get; set; }
}

/// <summary>
/// 字典类型列表响应。
/// </summary>
public class DictTypeListDto
{
    public Guid Id { get; set; }
    public string DictName { get; set; } = string.Empty;
    public string DictType { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 创建字典数据请求。
/// </summary>
public class CreateDictDataDto
{
    public Guid DictTypeId { get; set; }
    public string DictLabel { get; set; } = string.Empty;
    public string DictValue { get; set; } = string.Empty;
    public string? Remark { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsDefault { get; set; } = false;
}

/// <summary>
/// 更新字典数据请求。
/// </summary>
public class UpdateDictDataDto
{
    public Guid Id { get; set; }
    public Guid? DictTypeId { get; set; }
    public string? DictLabel { get; set; }
    public string? DictValue { get; set; }
    public string? Remark { get; set; }
    public int? SortOrder { get; set; }
    public bool? IsDefault { get; set; }
    public bool? IsEnabled { get; set; }
}

/// <summary>
/// 字典数据列表响应。
/// </summary>
public class DictDataListDto
{
    public Guid Id { get; set; }
    public Guid DictTypeId { get; set; }
    public string DictLabel { get; set; } = string.Empty;
    public string DictValue { get; set; } = string.Empty;
    public string? Remark { get; set; }
    public int SortOrder { get; set; }
    public bool IsDefault { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
}
