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
    /// <summary>
    /// 字典名称
    /// </summary>
    public string DictName { get; set; } = string.Empty;

    /// <summary>
    /// 字典类型
    /// </summary>
    public string DictType { get; set; } = string.Empty;
}

/// <summary>
/// 更新字典类型请求。
/// </summary>
public class UpdateDictTypeDto
{
    /// <summary>
    /// 字典类型ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 字典名称
    /// </summary>
    public string? DictName { get; set; }

    /// <summary>
    /// 字典类型
    /// </summary>
    public string? DictType { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }
}

/// <summary>
/// 字典类型列表响应。
/// </summary>
public class DictTypeListDto
{
    /// <summary>
    /// 字典类型ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 字典名称
    /// </summary>
    public string DictName { get; set; } = string.Empty;

    /// <summary>
    /// 字典类型
    /// </summary>
    public string DictType { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 创建字典数据请求。
/// </summary>
public class CreateDictDataDto
{
    /// <summary>
    /// 字典类型ID
    /// </summary>
    public Guid DictTypeId { get; set; }

    /// <summary>
    /// 字典标签
    /// </summary>
    public string DictLabel { get; set; } = string.Empty;

    /// <summary>
    /// 字典值
    /// </summary>
    public string DictValue { get; set; } = string.Empty;

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// 是否默认
    /// </summary>
    public bool IsDefault { get; set; } = false;
}

/// <summary>
/// 更新字典数据请求。
/// </summary>
public class UpdateDictDataDto
{
    /// <summary>
    /// 字典数据ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 字典类型ID
    /// </summary>
    public Guid? DictTypeId { get; set; }

    /// <summary>
    /// 字典标签
    /// </summary>
    public string? DictLabel { get; set; }

    /// <summary>
    /// 字典值
    /// </summary>
    public string? DictValue { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    public int? SortOrder { get; set; }

    /// <summary>
    /// 是否默认
    /// </summary>
    public bool? IsDefault { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }
}

/// <summary>
/// 字典数据列表响应。
/// </summary>
public class DictDataListDto
{
    /// <summary>
    /// 字典数据ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 字典类型ID
    /// </summary>
    public Guid DictTypeId { get; set; }

    /// <summary>
    /// 字典标签
    /// </summary>
    public string DictLabel { get; set; } = string.Empty;

    /// <summary>
    /// 字典值
    /// </summary>
    public string DictValue { get; set; } = string.Empty;

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// 是否默认
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
