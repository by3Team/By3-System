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

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using By3.Repository.Entities;
using By3.Repository.Repositories;
using By3.Service.DTOs;

namespace By3.Service.Services;

public class UserService
{
    private readonly UserRepository _repo;
    private readonly RoleRepository _roleRepo;
    private readonly DepartmentRepository _deptRepo;
    private readonly PositionRepository _positionRepo;
    private readonly IMemoryCache _cache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly DataProtectionService _dataProtection;

    public UserService(UserRepository repo, RoleRepository roleRepo, DepartmentRepository deptRepo, PositionRepository positionRepo, IMemoryCache cache, IHttpContextAccessor httpContextAccessor, DataProtectionService dataProtection)
    {
        _repo = repo;
        _roleRepo = roleRepo;
        _deptRepo = deptRepo;
        _positionRepo = positionRepo;
        _cache = cache;
        _httpContextAccessor = httpContextAccessor;
        _dataProtection = dataProtection;
    }

    private Guid? CurrentUserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userId, out var id) ? id : null;
        }
    }

    /// <summary>
    /// 将数据库中的手机号解密并掩码显示。
    /// </summary>
    private string GetMaskedPhone(string? storedPhone)
    {
        if (string.IsNullOrEmpty(storedPhone))
            return string.Empty;
        var plain = _dataProtection.IsEncrypted(storedPhone)
            ? _dataProtection.Decrypt(storedPhone)
            : storedPhone;
        return _dataProtection.MaskPhone(plain);
    }

    /// <summary>
    /// 邮箱掩码显示。
    /// </summary>
    private string GetMaskedEmail(string? email)
    {
        return _dataProtection.MaskEmail(email);
    }

    /// <summary>
    /// 保存前对手机号加密。若已经是加密形态则保持不变。
    /// </summary>
    private string ProtectPhone(string? phone)
    {
        if (string.IsNullOrEmpty(phone))
            return phone ?? string.Empty;
        if (_dataProtection.IsEncrypted(phone))
            return phone;
        return _dataProtection.Encrypt(phone);
    }

    /// <summary>
    /// 分页查询用户列表。
    /// </summary>
    public async Task<PageResult<UserListDto>> GetListAsync(int page, int pageSize, string? keyword)
    {
        var users = await _repo.GetListAsync(page, pageSize, keyword);
        var total = await _repo.GetCountAsync(keyword);
        var roles = await _roleRepo.GetListAsync();
        var departments = await _deptRepo.GetAllAsync();
        var positions = await _positionRepo.GetListAsync(1, int.MaxValue, null);

        var items = new List<UserListDto>();
        foreach (var u in users)
        {
            var rids = await _repo.GetRoleIdsByUserIdAsync(u.Id);
            var roleNames = roles.Where(r => rids.Contains(r.Id)).Select(r => r.RoleName).ToList();
            var dept = departments.FirstOrDefault(d => d.Id == u.DepartmentId);
            var position = positions.FirstOrDefault(p => p.Id == u.PositionId);
            items.Add(new UserListDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = GetMaskedEmail(u.Email),
                Phone = GetMaskedPhone(u.Phone),
                RealName = u.RealName,
                Gender = u.Gender,
                DepartmentId = u.DepartmentId,
                DepartmentName = dept?.DeptName,
                PositionId = u.PositionId,
                PositionName = position?.PositionName,
                IsEnabled = u.IsEnabled,
                CreatedAt = u.CreatedAt,
                RoleIds = rids,
                RoleNames = roleNames
            });
        }

        return new PageResult<UserListDto> { Total = total, Items = items, Page = page, PageSize = pageSize };
    }

    /// <summary>
    /// 根据ID获取用户详情。
    /// </summary>
    public async Task<UserListDto?> GetByIdAsync(Guid id)
    {
        var u = await _repo.GetByIdAsync(id);
        if (u == null) return null;
        var rids = await _repo.GetRoleIdsByUserIdAsync(id);
        var roles = await _roleRepo.GetListAsync();
        var dept = await _deptRepo.GetByIdAsync(u.DepartmentId ?? Guid.Empty);
        var position = await _positionRepo.GetByIdAsync(u.PositionId ?? Guid.Empty);
        return new UserListDto
        {
            Id = u.Id,
            UserName = u.UserName,
            Email = GetMaskedEmail(u.Email),
            Phone = GetMaskedPhone(u.Phone),
            RealName = u.RealName,
            Gender = u.Gender,
            DepartmentId = u.DepartmentId,
            DepartmentName = dept?.DeptName,
            PositionId = u.PositionId,
            PositionName = position?.PositionName,
            IsEnabled = u.IsEnabled,
            CreatedAt = u.CreatedAt,
            RoleIds = rids,
            RoleNames = roles.Where(r => rids.Contains(r.Id)).Select(r => r.RoleName).ToList()
        };
    }

    /// <summary>
    /// 创建用户。
    /// </summary>
    public async Task<Guid> CreateAsync(CreateUserDto dto)
    {
        var user = new SysUser
        {
            UserName = dto.UserName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Email = dto.Email,
            Phone = ProtectPhone(dto.Phone),
            RealName = dto.RealName,
            Gender = dto.Gender,
            DepartmentId = dto.DepartmentId,
            PositionId = dto.PositionId,
            CreatedBy = CurrentUserId,
            CreatedAt = DateTime.UtcNow
        };
        var id = await _repo.CreateAsync(user);
        await _repo.SetUserRolesAsync(user.Id, dto.RoleIds);
        return user.Id;
    }

    /// <summary>
    /// 更新用户信息。
    /// </summary>
    public async Task<int> UpdateAsync(UpdateUserDto dto)
    {
        var user = await _repo.GetByIdAsync(dto.Id);
        if (user == null) return 0;

        if (dto.Email != null) user.Email = dto.Email;
        if (dto.Phone != null) user.Phone = ProtectPhone(dto.Phone);
        if (dto.RealName != null) user.RealName = dto.RealName;
        user.Gender = dto.Gender;
        user.DepartmentId = dto.DepartmentId;
        user.PositionId = dto.PositionId;
        if (dto.IsEnabled.HasValue) user.IsEnabled = dto.IsEnabled.Value;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = CurrentUserId;

        var result = await _repo.UpdateAsync(user);
        if (dto.RoleIds.Count > 0)
            await _repo.SetUserRolesAsync(dto.Id, dto.RoleIds);

        _cache.Remove($"user_permissions_{dto.Id}");
        _cache.Remove($"user_menus_{dto.Id}");
        return result;
    }

    /// <summary>
    /// 删除用户。
    /// </summary>
    public async Task<int> DeleteAsync(Guid id)
    {
        _cache.Remove($"user_permissions_{id}");
        _cache.Remove($"user_menus_{id}");
        return await _repo.DeleteAsync(id);
    }

    /// <summary>
    /// 获取用户关联的角色ID列表。
    /// </summary>
    public async Task<List<Guid>> GetUserRoleIdsAsync(Guid userId)
        => await _repo.GetRoleIdsByUserIdAsync(userId);

    /// <summary>
    /// 重置用户密码。
    /// </summary>
    public async Task<int> ResetPasswordAsync(Guid userId, string newPassword)
    {
        var user = await _repo.GetByIdAsync(userId);
        if (user == null) return 0;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.PasswordChangedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = CurrentUserId;
        return await _repo.UpdateAsync(user);
    }

    /// <summary>
    /// 一次性迁移：将数据库中尚未加密的手机号加密存储。
    /// </summary>
    public async Task<int> MigratePlaintextPhonesAsync()
    {
        var users = await _repo.GetAllWithPhoneAsync();
        var count = 0;
        foreach (var user in users)
        {
            if (string.IsNullOrWhiteSpace(user.Phone))
                continue;
            if (_dataProtection.IsEncrypted(user.Phone))
                continue;

            user.Phone = _dataProtection.Encrypt(user.Phone);
            await _repo.UpdateAsync(user);
            count++;
        }
        return count;
    }
}
