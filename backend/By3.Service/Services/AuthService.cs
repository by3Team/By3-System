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

using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using By3.Repository.Entities;
using By3.Repository.Repositories;
using By3.Service.DTOs;

namespace By3.Service.Services;

public class AuthService
{
    private readonly UserRepository _userRepo;
    private readonly RoleRepository _roleRepo;
    private readonly MenuRepository _menuRepo;
    private readonly LoginLogRepository _logRepo;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _config;

    public AuthService(UserRepository userRepo, RoleRepository roleRepo, MenuRepository menuRepo,
        LoginLogRepository logRepo, IMemoryCache cache, IConfiguration config)
    {
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _menuRepo = menuRepo;
        _logRepo = logRepo;
        _cache = cache;
        _config = config;
    }

    public async Task<LoginResultDto?> LoginAsync(LoginDto dto, string? ip)
    {
        var user = await _userRepo.GetByUserNameAsync(dto.UserName);

        // 登录失败锁定检查：5分钟内连续失败5次则锁定15分钟
        var failKey = $"login_fail_{dto.UserName}";
        _cache.TryGetValue(failKey, out LoginFailInfo? failInfo);
        if (failInfo != null && failInfo.IsLocked)
        {
            await _logRepo.CreateAsync(new SysLoginLog
            {
                UserName = dto.UserName,
                IsSuccess = false,
                Message = $"账号已锁定，请在{failInfo.LockedUntil.Subtract(DateTime.UtcNow).Minutes}分钟后重试",
                IpAddress = ip
            });
            return null;
        }

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            // 记录失败次数
            RecordLoginFailure(dto.UserName, failKey, failInfo!);

            await _logRepo.CreateAsync(new SysLoginLog
            {
                UserName = dto.UserName,
                IsSuccess = false,
                Message = "用户名或密码错误",
                IpAddress = ip
            });
            return null;
        }

        if (!user.IsEnabled)
        {
            await _logRepo.CreateAsync(new SysLoginLog
            {
                UserId = user.Id,
                UserName = dto.UserName,
                IsSuccess = false,
                Message = "账号已禁用",
                IpAddress = ip
            });
            return null;
        }

        // 登录成功，清除失败计数
        _cache.Remove(failKey);

        var roleIds = await _userRepo.GetRoleIdsByUserIdAsync(user.Id);
        var permissions = await GetUserPermissionsAsync(user.Id);
        var menus = await GetUserMenusAsync(user.Id);

        var accessToken = GenerateAccessToken(user, roleIds, permissions, out var expiresAt);
        var refreshToken = GenerateRefreshToken(user.Id);

        await _logRepo.CreateAsync(new SysLoginLog
        {
            UserId = user.Id,
            UserName = dto.UserName,
            IsSuccess = true,
            Message = "登录成功",
            IpAddress = ip
        });

        return new LoginResultDto
        {
            UserId = user.Id,
            Token = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            UserName = user.UserName,
            RealName = user.RealName,
            NeedChangePassword = user.PasswordChangedAt == null,
            Permissions = permissions,
            Menus = BuildMenuTree(menus)
        };
    }

    private void RecordLoginFailure(string userName, string failKey, LoginFailInfo existingInfo)
    {
        var now = DateTime.UtcNow;
        var info = existingInfo.FailCount > 0 && (now - existingInfo.FirstFailAt).TotalMinutes < 5
            ? existingInfo
            : new LoginFailInfo();

        info.FailCount++;
        info.FirstFailAt = info.FailCount == 1 ? now : info.FirstFailAt;

        if (info.FailCount >= 5)
        {
            info.IsLocked = true;
            info.LockedUntil = now.AddMinutes(15);
            _cache.Set(failKey, info, TimeSpan.FromMinutes(15));
        }
        else
        {
            _cache.Set(failKey, info, TimeSpan.FromMinutes(5));
        }
    }

    public async Task<LoginResultDto?> RefreshTokenAsync(string refreshToken)
    {
        var principal = await ValidateRefreshToken(refreshToken);
        if (principal == null) return null;

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return null;

        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null || !user.IsEnabled) return null;

        var roleIds = await _userRepo.GetRoleIdsByUserIdAsync(user.Id);
        var permissions = await GetUserPermissionsAsync(user.Id);
        var menus = await GetUserMenusAsync(user.Id);

        var accessToken = GenerateAccessToken(user, roleIds, permissions, out var expiresAt);
        var newRefreshToken = GenerateRefreshToken(user.Id);

        // 刷新后旧 Refresh Token 加入黑名单
        BlacklistToken(refreshToken);

        return new LoginResultDto
        {
            UserId = user.Id,
            Token = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = expiresAt,
            UserName = user.UserName,
            RealName = user.RealName,
            NeedChangePassword = user.PasswordChangedAt == null,
            Permissions = permissions,
            Menus = BuildMenuTree(menus)
        };
    }

    public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
    {
        string cacheKey = $"user_permissions_{userId}";
        if (_cache.TryGetValue(cacheKey, out List<string>? perms) && perms != null)
            return perms;

        var roleIds = await _userRepo.GetRoleIdsByUserIdAsync(userId);
        perms = await _menuRepo.GetPermissionsByRoleIdsAsync(roleIds);

        _cache.Set(cacheKey, perms, TimeSpan.FromMinutes(5));
        return perms;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) return false;

        if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
            return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.PasswordChangedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepo.UpdateAsync(user);

        _cache.Remove($"user_permissions_{userId}");
        _cache.Remove($"user_menus_{userId}");
        return true;
    }

    internal async Task<List<SysMenu>> GetUserMenusAsync(Guid userId)
    {
        string cacheKey = $"user_menus_{userId}";
        if (_cache.TryGetValue(cacheKey, out List<SysMenu>? menus) && menus != null)
            return menus;

        var roleIds = await _userRepo.GetRoleIdsByUserIdAsync(userId);
        menus = await _menuRepo.GetMenusByRoleIdsAsync(roleIds);

        _cache.Set(cacheKey, menus, TimeSpan.FromMinutes(5));
        return menus;
    }

    public async Task<List<MenuTreeDto>> GetUserMenusWithTreeAsync(Guid userId)
    {
        var menus = await GetUserMenusAsync(userId);
        return BuildMenuTree(menus);
    }

    public void ClearUserCache(Guid userId)
    {
        _cache.Remove($"user_permissions_{userId}");
        _cache.Remove($"user_menus_{userId}");
    }

    public void BlacklistToken(string token)
    {
        var jti = GetTokenJti(token);
        if (!string.IsNullOrEmpty(jti))
        {
            var expiry = GetTokenExpiry(token) ?? DateTime.UtcNow.AddHours(8);
            var ttl = expiry - DateTime.UtcNow;
            if (ttl > TimeSpan.Zero)
                _cache.Set($"token_blacklist_{jti}", true, ttl);
        }
    }

    public bool IsTokenBlacklisted(string token)
    {
        var jti = GetTokenJti(token);
        return !string.IsNullOrEmpty(jti) && _cache.TryGetValue($"token_blacklist_{jti}", out _);
    }

    private string GenerateAccessToken(SysUser user, List<Guid> roleIds, List<string> permissions, out DateTime expiresAt)
    {
        var jwtKey = _config["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey))
            throw new InvalidOperationException("Jwt:Key 未配置，无法生成 Token。");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var accessTokenExpireHours = _config.GetValue<int>("Jwt:AccessTokenExpireHours", 8);
        expiresAt = DateTime.UtcNow.AddHours(accessTokenExpireHours);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new("realName", user.RealName ?? ""),
            new("permissions", string.Join(',', permissions))
        };
        foreach (var rid in roleIds)
            claims.Add(new Claim("roleId", rid.ToString()));

        var handler = new JsonWebTokenHandler();
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _config["Jwt:Issuer"]!,
            Audience = _config["Jwt:Audience"]!,
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            SigningCredentials = creds
        };

        return handler.CreateToken(descriptor);
    }

    private string GenerateRefreshToken(Guid userId)
    {
        var jwtKey = _config["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey))
            throw new InvalidOperationException("Jwt:Key 未配置，无法生成 Token。");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new("tokenType", "refresh")
        };

        var handler = new JsonWebTokenHandler();
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _config["Jwt:Issuer"]!,
            Audience = _config["Jwt:Audience"]!,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(_config.GetValue<int>("Jwt:RefreshTokenExpireDays", 7)),
            SigningCredentials = creds
        };

        return handler.CreateToken(descriptor);
    }

    private async Task<ClaimsPrincipal?> ValidateRefreshToken(string refreshToken)
    {
        try
        {
            var jwtKey = _config["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(jwtKey)) return null;

            var tokenHandler = new JsonWebTokenHandler();
            var result = await tokenHandler.ValidateTokenAsync(refreshToken, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _config["Jwt:Issuer"]!,
                ValidAudience = _config["Jwt:Audience"]!,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.Zero
            });

            if (!result.IsValid) return null;

            var principal = new ClaimsPrincipal(result.ClaimsIdentity);

            var tokenType = principal.FindFirst("tokenType")?.Value;
            if (tokenType != "refresh") return null;

            if (IsTokenBlacklisted(refreshToken)) return null;

            return principal;
        }
        catch
        {
            return null;
        }
    }

    private string? GetTokenJti(string? token)
    {
        if (string.IsNullOrEmpty(token)) return null;
        try
        {
            var handler = new JsonWebTokenHandler();
            var jwt = handler.ReadJsonWebToken(token);
            return jwt.Id;
        }
        catch
        {
            return null;
        }
    }

    private DateTime? GetTokenExpiry(string? token)
    {
        if (string.IsNullOrEmpty(token)) return null;
        try
        {
            var handler = new JsonWebTokenHandler();
            var jwt = handler.ReadJsonWebToken(token);
            return jwt.ValidTo;
        }
        catch
        {
            return null;
        }
    }

    private static List<MenuTreeDto> BuildMenuTree(List<SysMenu> menus)
    {
        var dict = menus.ToDictionary(m => m.Id, m => new MenuTreeDto
        {
            Id = m.Id,
            MenuName = m.MenuName,
            Permission = m.Permission,
            Route = m.Route,
            Icon = m.Icon,
            Component = m.Component,
            MenuType = m.MenuType,
            SortOrder = m.SortOrder,
            ParentId = m.ParentId
        });

        var roots = new List<MenuTreeDto>();
        foreach (var item in dict.Values)
        {
            if (item.ParentId == null || !dict.ContainsKey(item.ParentId.Value))
                roots.Add(item);
            else if (dict.TryGetValue(item.ParentId.Value, out var parent))
                parent.Children.Add(item);
        }
        return roots.OrderBy(r => r.SortOrder).ToList();
    }
}

internal class LoginFailInfo
{
    public int FailCount { get; set; }
    public DateTime FirstFailAt { get; set; } = DateTime.UtcNow;
    public bool IsLocked { get; set; }
    public DateTime LockedUntil { get; set; }
}
