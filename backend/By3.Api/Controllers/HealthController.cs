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

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using By3.Repository;

namespace By3.Api.Controllers;

/// <summary>
/// 健康检查：用于容器编排和负载均衡器探测应用及数据库状态。
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _db;

    public HealthController(AppDbContext db) => _db = db;

    /// <summary>
    /// 健康检查端点，验证数据库连接。
    /// </summary>
    /// <returns>应用及数据库状态</returns>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Checks = new[]
            {
                new { Name = "Database", Status = await CheckDatabaseAsync() }
            }
        };

        var isHealthy = result.Checks.All(c => c.Status == "Healthy");
        return isHealthy ? Ok(result) : StatusCode(503, result);
    }

    private async Task<string> CheckDatabaseAsync()
    {
        try
        {
            await _db.Database.ExecuteSqlRawAsync("SELECT 1");
            return "Healthy";
        }
        catch
        {
            return "Unhealthy";
        }
    }
}
