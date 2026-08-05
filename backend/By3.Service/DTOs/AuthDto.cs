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
/// 登录请求。
/// </summary>
public class LoginDto
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// 登录响应，包含 Token、权限和菜单数据。
/// </summary>
public class LoginResultDto
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? RealName { get; set; }
    public bool NeedChangePassword { get; set; }
    public List<string> Permissions { get; set; } = new();
    public List<MenuTreeDto> Menus { get; set; } = new();
}

/// <summary>
/// 刷新 Token 请求。
/// </summary>
public class RefreshTokenDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// 修改密码请求。
/// </summary>
public class ChangePasswordDto
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// 菜单树形结构。
/// </summary>
public class MenuTreeDto
{
    public Guid Id { get; set; }
    public string MenuName { get; set; } = string.Empty;
    public string? Permission { get; set; }
    public string? Route { get; set; }
    public string? Icon { get; set; }
    public string? Component { get; set; }
    public int MenuType { get; set; }
    public int SortOrder { get; set; }
    public Guid? ParentId { get; set; }
    public List<MenuTreeDto> Children { get; set; } = new();
}
