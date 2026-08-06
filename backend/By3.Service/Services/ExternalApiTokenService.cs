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
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using By3.Repository.Data;
using By3.Repository.Entities;
using By3.Repository.Repositories;
using By3.Service.DTOs;

namespace By3.Service.Services;

public class ExternalApiTokenService
{
    private readonly ExternalApiTokenRepository _tokenRepo;
    private readonly ExternalApiTokenHistoryRepository _historyRepo;
    private readonly ExternalApiTokenLogRepository _tokenLogRepo;
    private readonly ExternalApiAccessLogRepository _logRepo;
    private readonly ExternalApiRepository _apiRepo;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ExternalApiTokenService(
        ExternalApiTokenRepository tokenRepo,
        ExternalApiTokenHistoryRepository historyRepo,
        ExternalApiTokenLogRepository tokenLogRepo,
        ExternalApiAccessLogRepository logRepo,
        ExternalApiRepository apiRepo,
        IServiceScopeFactory serviceScopeFactory,
        IHttpContextAccessor httpContextAccessor)
    {
        _tokenRepo = tokenRepo;
        _historyRepo = historyRepo;
        _tokenLogRepo = tokenLogRepo;
        _logRepo = logRepo;
        _apiRepo = apiRepo;
        _serviceScopeFactory = serviceScopeFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    private Guid? CurrentUserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userId, out var id) ? id : null;
        }
    }

    private string? CurrentUserName
        => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

    private string? ClientIp
        => _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

    /// <summary>
    /// 分页查询对外 API Token 列表。
    /// </summary>
    public async Task<PageResult<ExternalApiTokenDto>> GetListAsync(int page, int pageSize, string? keyword = null, string? isEnabled = null, bool? includeDeleted = null)
    {
        var includeDeletedValue = includeDeleted ?? false;
        var isEnabledValue = ParseIsEnabled(isEnabled);
        var items = await _tokenRepo.GetListAsync(page, pageSize, keyword, includeDeletedValue, isEnabledValue);
        var total = await _tokenRepo.GetCountAsync(keyword, includeDeletedValue, isEnabledValue);

        return new PageResult<ExternalApiTokenDto>
        {
            Total = total,
            Page = page,
            PageSize = pageSize,
            Items = items.Select(t => MaskSecrets(MapToDto(t))).ToList()
        };
    }

    /// <summary>
    /// 导出 CSV。
    /// 列：应用名称、负责人邮箱、可访问接口、有效期至、状态、创建时间。
    /// </summary>
    public async Task<byte[]> ExportCsvAsync(string? keyword = null, string? isEnabled = null, bool? includeDeleted = null)
    {
        var includeDeletedValue = includeDeleted ?? false;
        var isEnabledValue = ParseIsEnabled(isEnabled);
        var items = await _tokenRepo.GetAllAsync(keyword, includeDeletedValue, isEnabledValue);
        var apiMap = (await _apiRepo.GetAllAsync())
            .ToDictionary(a => a.Id, a => a.ApiName);

        var lines = new List<string>
        {
            "\uFEFF应用名称,负责人邮箱,可访问接口,有效期至,状态,创建时间"
        };

        foreach (var token in items)
        {
            var dto = MapToDto(token);
            lines.Add(string.Join(",",
                EscapeCsv(dto.AppName),
                EscapeCsv(dto.ContactEmail),
                EscapeCsv(FormatAllowedApisForExport(dto, apiMap)),
                EscapeCsv(dto.ExpireTime.HasValue ? dto.ExpireTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : ""),
                EscapeCsv(FormatStatusForExport(dto)),
                EscapeCsv(dto.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"))));
        }

        return Encoding.UTF8.GetBytes(string.Join("\r\n", lines));
    }

    /// <summary>
    /// 解析启用状态字符串为布尔值。
    /// </summary>
    private static bool? ParseIsEnabled(string? isEnabled)
    {
        return isEnabled?.ToLowerInvariant() switch
        {
            "enabled" => true,
            "disabled" => false,
            _ => null
        };
    }

    /// <summary>
    /// 格式化允许访问的接口名称用于 CSV 导出。
    /// </summary>
    private static string FormatAllowedApisForExport(ExternalApiTokenDto dto, Dictionary<Guid, string> apiMap)
    {
        if (dto.AllowedApiIds == null || dto.AllowedApiIds.Count == 0) return "全部接口";
        var names = dto.AllowedApiIds
            .Select(id => apiMap.TryGetValue(id, out var name) ? name : id.ToString())
            .ToList();
        return string.Join("、", names);
    }

    /// <summary>
    /// 格式化 Token 状态用于 CSV 导出。
    /// </summary>
    private static string FormatStatusForExport(ExternalApiTokenDto dto)
    {
        if (dto.IsDeleted) return "已删除";
        return dto.IsEnabled ? "启用" : "停用";
    }

    /// <summary>
    /// CSV 字段转义（处理逗号、引号、换行）。
    /// </summary>
    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        var text = value.Replace("\"", "\"\"");
        if (text.Contains(',') || text.Contains('"') || text.Contains('\r') || text.Contains('\n'))
            text = $"\"{text}\"";
        return text;
    }

    /// <summary>
    /// 根据 ID 获取 Token 详情（敏感字段脱敏）。
    /// </summary>
    public async Task<ExternalApiTokenDto?> GetByIdAsync(Guid id, bool includeDeleted = false)
    {
        var token = await _tokenRepo.GetByIdAsync(id, includeDeleted);
        return token == null ? null : MaskSecrets(MapToDto(token));
    }

    /// <summary>
    /// 根据 ApiKey 查询有效 Token（含缓冲期内的旧 Key）。
    /// 返回的 DTO 中 ApiSecret 为实际用于签名的 Secret（当前 Key 或旧 Key）。
    /// </summary>
    public async Task<ExternalApiTokenDto?> GetByApiKeyAsync(string apiKey)
    {
        var token = await _tokenRepo.GetByApiKeyAsync(apiKey);
        if (token == null) return null;

        var dto = MapToDto(token);
        // 如果请求使用的是旧 Key，则把签名用的 Secret 切换为旧 Secret
        if (!string.IsNullOrEmpty(token.PreviousApiKey) &&
            token.PreviousApiKey.Equals(apiKey, StringComparison.Ordinal) &&
            token.PreviousValidUntil.HasValue &&
            token.PreviousValidUntil.Value >= DateTime.UtcNow)
        {
            dto.ApiSecret = token.PreviousApiSecret ?? string.Empty;
        }

        return dto;
    }

    /// <summary>
    /// 创建对外 API Token。
    /// </summary>
    public async Task<ExternalApiTokenDto> CreateAsync(CreateExternalApiTokenDto dto)
    {
        var expireType = NormalizeExpireType(dto.ExpireType);
        var expireTime = ToUtc(CalcExpireTime(expireType, dto.ExpireTime));

        var token = new SysExternalApiToken
        {
            Id = Guid.NewGuid(),
            AppName = dto.AppName,
            ApiKey = GenerateApiKey(),
            ApiSecret = GenerateApiSecret(),
            Description = dto.Description,
            ExpireTime = expireTime,
            ExpireType = expireType,
            AllowedApiIds = SerializeAllowedApiIds(dto.AllowedApiIds),
            ContactEmail = dto.ContactEmail ?? string.Empty,
            IsEnabled = dto.IsEnabled,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = CurrentUserId
        };

        await _tokenRepo.CreateAsync(token);
        await AddLogAsync(token, "Create", $"创建 Token，应用名称：{token.AppName}，有效期类型：{expireType}");
        return MapToDto(token);
    }

    /// <summary>
    /// 更新 Token 信息。
    /// </summary>
    public async Task<int> UpdateAsync(Guid id, UpdateExternalApiTokenDto dto)
    {
        var token = await _tokenRepo.GetByIdAsync(id, includeDeleted: true);
        if (token == null) return 0;
        EnsureNotDeleted(token);

        var expireType = NormalizeExpireType(dto.ExpireType);
        var expireTime = ToUtc(CalcExpireTime(expireType, dto.ExpireTime));

        token.AppName = dto.AppName;
        token.Description = dto.Description;
        token.ExpireTime = expireTime;
        token.ExpireType = expireType;
        token.AllowedApiIds = SerializeAllowedApiIds(dto.AllowedApiIds);
        token.ContactEmail = dto.ContactEmail ?? string.Empty;
        token.IsEnabled = dto.IsEnabled;
        token.UpdatedAt = DateTime.UtcNow;
        token.UpdatedBy = CurrentUserId;

        var result = await _tokenRepo.UpdateAsync(token);
        await AddLogAsync(token, "Update", $"更新 Token 信息，有效期类型：{expireType}");
        return result;
    }

    /// <summary>
    /// 逻辑删除 Token。
    /// </summary>
    public async Task<int> DeleteAsync(Guid id)
    {
        var token = await _tokenRepo.GetByIdAsync(id, includeDeleted: true);
        if (token == null) return 0;
        EnsureNotDeleted(token);

        token.UpdatedAt = DateTime.UtcNow;
        token.UpdatedBy = CurrentUserId;

        var result = await _tokenRepo.DeleteAsync(id);
        if (result > 0)
        {
            await AddLogAsync(token, "Delete", "逻辑删除 Token");
        }
        return result;
    }

    /// <summary>
    /// 重新生成 Token 的 ApiKey/ApiSecret，旧 Key 可设缓冲期。
    /// </summary>
    public async Task<ExternalApiTokenDto?> RegenerateAsync(Guid id, RegenerateExternalApiTokenDto dto)
    {
        var token = await _tokenRepo.GetByIdAsync(id, includeDeleted: true);
        if (token == null) return null;
        EnsureNotDeleted(token);

        // 自定义有效期已过期时禁止重新生成
        if (token.ExpireType == "custom" && token.ExpireTime.HasValue && token.ExpireTime.Value <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("自定义有效期已过期，无法重新生成 Key/Secret");
        }

        var oldKey = token.ApiKey;
        var oldSecret = token.ApiSecret;
        var oldExpireTime = token.ExpireTime;
        var now = DateTime.UtcNow;
        var oldKeyValidUntil = dto.OldKeyExpireType == 1 && dto.OldKeyExpireAt.HasValue
            ? ToUtc(dto.OldKeyExpireAt.Value)
            : now;

        // 重新生成前，先将该 Token 下所有仍在缓冲期内的历史凭证失效，
        // 确保同一应用最多只有“当前 Key + 上一个缓冲 Key”两个有效凭证。
        await _historyRepo.InvalidateAllActiveAsync(token.Id, CurrentUserId);

        // 将当前凭证归档到历史表
        await _historyRepo.CreateAsync(new SysExternalApiTokenHistory
        {
            TokenId = token.Id,
            AppName = token.AppName,
            ApiKey = oldKey,
            ApiSecret = oldSecret,
            ExpireTime = oldExpireTime,
            ValidUntil = oldKeyValidUntil,
            InvalidatedAt = dto.OldKeyExpireType == 0 ? now : null,
            CreatedAt = now
        });

        // 保存旧凭证到缓冲字段（兼容现有签名验证逻辑）
        token.PreviousApiKey = oldKey;
        token.PreviousApiSecret = oldSecret;
        token.PreviousValidUntil = oldKeyValidUntil;

        // 根据原有效期类型计算新的过期时间
        token.ExpireTime = ToUtc(CalcExpireTime(token.ExpireType, token.ExpireTime));
        token.ApiKey = GenerateApiKey();
        token.ApiSecret = GenerateApiSecret();
        token.UpdatedAt = DateTime.UtcNow;
        token.UpdatedBy = CurrentUserId;

        await _tokenRepo.UpdateAsync(token);

        var graceRemark = token.PreviousValidUntil.HasValue
            ? $"旧 Key 缓冲至 {token.PreviousValidUntil.Value:yyyy-MM-dd HH:mm:ss} UTC"
            : "旧 Key 立即失效";
        await AddLogAsync(token, "Regenerate", $"重新生成 Key/Secret。{graceRemark}，新有效期类型：{token.ExpireType}");

        // 邮件通知改为后台异步执行，避免 SMTP 连接超时阻塞接口响应
        var operatorId = CurrentUserId;
        _ = Task.Run(async () => await SendRegenerateNotificationAsync(token, operatorId, graceRemark));

        return MapToDto(token);
    }

    /// <summary>
    /// 后台异步发送 Token 重新生成的邮件通知。
    /// </summary>
    private async Task SendRegenerateNotificationAsync(SysExternalApiToken token, Guid? operatorId, string graceRemark)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var userRepo = scope.ServiceProvider.GetRequiredService<UserRepository>();
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

            string? adminEmail = null;
            if (operatorId.HasValue)
            {
                var user = await userRepo.GetByIdAsync(operatorId.Value);
                adminEmail = user?.Email;
            }

            var contactEmails = token.ContactEmail
                .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim())
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (contactEmails.Count == 0 && string.IsNullOrWhiteSpace(adminEmail))
                return;

            var variables = new Dictionary<string, string>
            {
                ["AppName"] = token.AppName,
                ["ApiKey"] = token.ApiKey,
                ["ApiSecret"] = token.ApiSecret,
                ["ExpireType"] = token.ExpireType,
                ["GraceRemark"] = graceRemark,
                ["OperateTime"] = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC"
            };

            var allRecipients = new List<string>(contactEmails);
            if (!string.IsNullOrWhiteSpace(adminEmail) && !allRecipients.Contains(adminEmail, StringComparer.OrdinalIgnoreCase))
                allRecipients.Add(adminEmail);

            foreach (var address in allRecipients)
            {
                await emailService.SendBatchAsync(new SendEmailDto
                {
                    TemplateId = DbSeeder.TokenNotifyTemplateId,
                    Version = "v1",
                    ToAddresses = new List<string> { address },
                    CcAddresses = new List<string>(),
                    Variables = variables
                });
            }
        }
        catch
        {
            // 邮件通知失败不应影响 Token 重新生成结果，异常已在邮件日志中记录
        }
    }

    /// <summary>
    /// 分页查询 Token 历史凭证记录。
    /// </summary>
    public async Task<PageResult<ExternalApiTokenHistoryDto>> GetHistoryAsync(Guid tokenId, int page, int pageSize, string? status = null)
    {
        var items = await _historyRepo.GetListAsync(tokenId, page, pageSize, status);
        var total = await _historyRepo.GetCountAsync(tokenId, status);

        return new PageResult<ExternalApiTokenHistoryDto>
        {
            Total = total,
            Page = page,
            PageSize = pageSize,
            Items = items.Select(MapHistoryToDto).ToList()
        };
    }

    /// <summary>
    /// 立即使指定历史凭证失效。
    /// </summary>
    public async Task<int> InvalidateHistoryAsync(Guid historyId)
    {
        return await _historyRepo.InvalidateAsync(historyId, CurrentUserId);
    }

    /// <summary>
    /// 分页查询 Token 操作日志。
    /// </summary>
    public async Task<PageResult<ExternalApiTokenLogDto>> GetLogsAsync(Guid tokenId, int page, int pageSize)
    {
        var items = await _tokenLogRepo.GetListAsync(tokenId, page, pageSize);
        var total = await _tokenLogRepo.GetCountAsync(tokenId);

        return new PageResult<ExternalApiTokenLogDto>
        {
            Total = total,
            Page = page,
            PageSize = pageSize,
            Items = items.Select(MapLogToDto).ToList()
        };
    }

    /// <summary>
    /// 记录对外接口访问日志。
    /// </summary>
    public async Task LogAccessAsync(CreateExternalApiAccessLogDto dto)
    {
        var log = new SysExternalApiAccessLog
        {
            Id = Guid.NewGuid(),
            ApiKey = dto.ApiKey,
            RequestPath = dto.RequestPath,
            RequestMethod = dto.RequestMethod,
            RequestParams = dto.RequestParams,
            IpAddress = dto.IpAddress,
            Status = dto.Status,
            ErrorMessage = dto.ErrorMessage,
            IdempotencyKey = dto.IdempotencyKey,
            CreatedAt = DateTime.UtcNow
        };
        await _logRepo.CreateAsync(log);
    }

    /// <summary>
    /// 记录 Token 操作日志。
    /// </summary>
    private async Task AddLogAsync(SysExternalApiToken token, string action, string remark)
    {
        var log = new SysExternalApiTokenLog
        {
            Id = Guid.NewGuid(),
            TokenId = token.Id,
            Action = action,
            ApiKey = token.ApiKey,
            IpAddress = ClientIp,
            OperatorId = CurrentUserId,
            OperatorName = CurrentUserName,
            Remark = remark,
            CreatedAt = DateTime.UtcNow
        };
        await _tokenLogRepo.CreateAsync(log);
    }

    /// <summary>
    /// 校验 Token 未被逻辑删除，否则抛出异常。
    /// </summary>
    private static void EnsureNotDeleted(SysExternalApiToken token)
    {
        if (token.IsDeleted)
            throw new InvalidOperationException("Token 已删除，禁止操作");
    }

    /// <summary>
    /// 将时间统一转换为 UTC。
    /// </summary>
    private static DateTime? ToUtc(DateTime? value)
    {
        if (!value.HasValue) return null;
        var dt = value.Value;
        return dt.Kind switch
        {
            DateTimeKind.Utc => dt,
            DateTimeKind.Local => dt.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
        };
    }

    /// <summary>
    /// 生成 ApiKey（by3_ 前缀 + 32 位随机十六进制）。
    /// </summary>
    private static string GenerateApiKey()
    {
        return $"by3_{Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLower()}";
    }

    /// <summary>
    /// 生成 ApiSecret（64 位随机十六进制）。
    /// </summary>
    private static string GenerateApiSecret()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLower();
    }

    /// <summary>
    /// 标准化有效期类型（30/60/90/custom）。
    /// </summary>
    private static string NormalizeExpireType(string? expireType)
    {
        return expireType switch
        {
            "60" => "60",
            "90" => "90",
            "custom" => "custom",
            _ => "30"
        };
    }

    /// <summary>
    /// 根据有效期类型计算过期时间。
    /// </summary>
    private static DateTime? CalcExpireTime(string expireType, DateTime? customExpireTime)
    {
        return expireType switch
        {
            "60" => DateTime.UtcNow.AddDays(60),
            "90" => DateTime.UtcNow.AddDays(90),
            "custom" => customExpireTime,
            _ => DateTime.UtcNow.AddDays(30)
        };
    }

    /// <summary>
    /// 将 Token 实体映射为 DTO。
    /// </summary>
    private static ExternalApiTokenDto MapToDto(SysExternalApiToken token) => new()
    {
        Id = token.Id,
        AppName = token.AppName,
        ApiKey = token.ApiKey,
        ApiSecret = token.ApiSecret,
        Description = token.Description,
        ExpireTime = token.ExpireTime,
        ExpireType = token.ExpireType,
        AllowedApiIds = ParseAllowedApiIds(token.AllowedApiIds),
        ContactEmail = token.ContactEmail,
        IsEnabled = token.IsEnabled,
        IsDeleted = token.IsDeleted,
        PreviousValidUntil = token.PreviousValidUntil,
        PreviousApiKey = token.PreviousApiKey,
        PreviousApiSecret = token.PreviousApiSecret,
        CreatedAt = token.CreatedAt,
        UpdatedAt = token.UpdatedAt
    };

    /// <summary>
    /// 将历史凭证实体映射为 DTO。
    /// </summary>
    private static ExternalApiTokenHistoryDto MapHistoryToDto(SysExternalApiTokenHistory history) => new()
    {
        Id = history.Id,
        TokenId = history.TokenId,
        AppName = history.AppName,
        ApiKey = history.ApiKey,
        ApiSecret = history.ApiSecret,
        ExpireTime = history.ExpireTime,
        ValidUntil = history.ValidUntil,
        InvalidatedAt = history.InvalidatedAt,
        InvalidatedBy = history.InvalidatedBy,
        CreatedAt = history.CreatedAt
    };

    /// <summary>
    /// 脱敏处理：清空 ApiSecret 和 PreviousApiSecret。
    /// </summary>
    private static ExternalApiTokenDto MaskSecrets(ExternalApiTokenDto dto)
    {
        dto.ApiSecret = string.Empty;
        dto.PreviousApiSecret = null;
        return dto;
    }

    /// <summary>
    /// 将操作日志实体映射为 DTO。
    /// </summary>
    private static ExternalApiTokenLogDto MapLogToDto(SysExternalApiTokenLog log) => new()
    {
        Id = log.Id,
        TokenId = log.TokenId,
        Action = log.Action,
        ApiKey = log.ApiKey,
        IpAddress = log.IpAddress,
        OperatorId = log.OperatorId,
        OperatorName = log.OperatorName,
        Remark = log.Remark,
        CreatedAt = log.CreatedAt
    };

    /// <summary>
    /// 序列化允许访问的接口 ID 列表为 JSON。
    /// </summary>
    private static string? SerializeAllowedApiIds(List<Guid> ids)
    {
        if (ids == null || ids.Count == 0) return null;
        return JsonSerializer.Serialize(ids);
    }

    /// <summary>
    /// 反序列化允许访问的接口 ID 列表。
    /// </summary>
    private static List<Guid> ParseAllowedApiIds(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<Guid>();
        try
        {
            return JsonSerializer.Deserialize<List<Guid>>(json) ?? new List<Guid>();
        }
        catch
        {
            return new List<Guid>();
        }
    }
}
