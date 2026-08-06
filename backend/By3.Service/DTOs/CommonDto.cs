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
/// 分页查询结果。
/// </summary>
public class PageResult<T>
{
    /// <summary>
    /// 总记录数
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// 数据列表
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// 当前页码
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// 每页条数
    /// </summary>
    public int PageSize { get; set; }
}

/// <summary>
/// 统一 API 响应包装。
/// </summary>
public class ApiResult<T>
{
    /// <summary>
    /// 状态码
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 响应消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 响应数据
    /// </summary>
    public T? Data { get; set; }

    public static ApiResult<T> Ok(T? data, string message = "success")
        => new() { Code = 200, Message = message, Data = data };

    public static ApiResult<object> Error(string message, int code = 500)
        => new() { Code = code, Message = message };

    public static ApiResult<object> Error(string message, int code, object data)
        => new() { Code = code, Message = message, Data = data };
}
