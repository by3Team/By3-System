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

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using By3.Repository;
using By3.Repository.Data;

namespace By3.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestConnectionString = "Host=localhost;Port=5432;Database=by3_test;Username=postgres;Password=123456";

    public CustomWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", TestConnectionString, EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable("Jwt__Key", "your-super-secret-key-at-least-32-bytes-long!", EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable("Jwt__Issuer", "By3", EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable("Jwt__Audience", "By3Client", EnvironmentVariableTarget.Process);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTests");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = TestConnectionString,
                ["TablePrefix"] = "by3_",
                ["Jwt:Key"] = "your-super-secret-key-at-least-32-bytes-long!",
                ["Jwt:Issuer"] = "By3",
                ["Jwt:Audience"] = "By3Client",
                ["Cors:AllowedOrigins"] = "http://localhost"
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(TestConnectionString));
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        await db.EnsureSeedDataAsync();
    }
}
