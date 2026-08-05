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

using By3.Repository.Entities;
using By3.Repository.Repositories;
using By3.Service.DTOs;

namespace By3.Service.Services;

public class LoginLogService
{
    private readonly LoginLogRepository _repo;
    public LoginLogService(LoginLogRepository repo) => _repo = repo;

    public async Task<PageResult<LoginLogDto>> GetListAsync(int page, int pageSize, LoginLogQueryDto? query = null)
    {
        var repoQuery = query == null ? null : new LoginLogQuery
        {
            UserName = query.UserName,
            IsSuccess = query.IsSuccess,
            Keyword = query.Keyword,
            StartTime = query.StartTime,
            EndTime = query.EndTime
        };
        var items = await _repo.GetListAsync(page, pageSize, repoQuery);
        var total = await _repo.GetCountAsync(repoQuery);
        return new PageResult<LoginLogDto>
        {
            Total = total,
            Items = items.Select(MapToDto).ToList(),
            Page = page,
            PageSize = pageSize
        };
    }

    private static LoginLogDto MapToDto(SysLoginLog log) => new()
    {
        Id = log.Id,
        UserName = log.UserName,
        IsSuccess = log.IsSuccess,
        Message = log.Message,
        IpAddress = log.IpAddress,
        CreatedAt = log.CreatedAt
    };
}
