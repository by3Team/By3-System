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

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using By3.Service.Services;

namespace By3.Api.Authorization;

public class AuthorizationOptionsConfigurator : IConfigureOptions<AuthorizationOptions>
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationOptionsConfigurator"/> class.
    /// </summary>
    public AuthorizationOptionsConfigurator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 从数据库加载权限并注册为授权策略。
    /// </summary>
    public void Configure(AuthorizationOptions options)
    {
        using var scope = _serviceProvider.CreateScope();
        var menuService = scope.ServiceProvider.GetRequiredService<MenuService>();
        var permissions = menuService.GetAllPermissionsAsync().GetAwaiter().GetResult();
        foreach (var permission in permissions)
        {
            options.AddPolicy(permission, policy => policy.Requirements.Add(new PermissionRequirement(permission)));
        }
    }
}
