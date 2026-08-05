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

using System.Text.Json;

namespace By3.Service.Services;

public static class LicenseResolver
{
    // 常见 NuGet 包许可证映射
    private static readonly Dictionary<string, string> NuGetLicenseMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Microsoft.EntityFrameworkCore"] = "MIT",
        ["Microsoft.EntityFrameworkCore.Abstractions"] = "MIT",
        ["Microsoft.EntityFrameworkCore.Analyzers"] = "MIT",
        ["Microsoft.EntityFrameworkCore.Design"] = "MIT",
        ["Microsoft.EntityFrameworkCore.Relational"] = "MIT",
        ["Microsoft.AspNetCore.Authentication.JwtBearer"] = "MIT",
        ["Microsoft.AspNetCore.Mvc.Testing"] = "MIT",
        ["Microsoft.AspNetCore.TestHost"] = "MIT",
        ["Microsoft.Extensions.Caching.Abstractions"] = "MIT",
        ["Microsoft.Extensions.Caching.Memory"] = "MIT",
        ["Microsoft.Extensions.Configuration"] = "MIT",
        ["Microsoft.Extensions.Configuration.Abstractions"] = "MIT",
        ["Microsoft.Extensions.Configuration.Binder"] = "MIT",
        ["Microsoft.Extensions.Configuration.CommandLine"] = "MIT",
        ["Microsoft.Extensions.Configuration.EnvironmentVariables"] = "MIT",
        ["Microsoft.Extensions.Configuration.FileExtensions"] = "MIT",
        ["Microsoft.Extensions.Configuration.Json"] = "MIT",
        ["Microsoft.Extensions.Configuration.UserSecrets"] = "MIT",
        ["Microsoft.Extensions.DependencyInjection"] = "MIT",
        ["Microsoft.Extensions.DependencyInjection.Abstractions"] = "MIT",
        ["Microsoft.Extensions.DependencyModel"] = "MIT",
        ["Microsoft.Extensions.Diagnostics"] = "MIT",
        ["Microsoft.Extensions.Diagnostics.Abstractions"] = "MIT",
        ["Microsoft.Extensions.ApiDescription.Server"] = "MIT",
        ["Microsoft.IdentityModel.Abstractions"] = "MIT",
        ["Microsoft.IdentityModel.JsonWebTokens"] = "MIT",
        ["Microsoft.IdentityModel.Logging"] = "MIT",
        ["Microsoft.IdentityModel.Protocols"] = "MIT",
        ["Microsoft.IdentityModel.Protocols.OpenIdConnect"] = "MIT",
        ["Microsoft.IdentityModel.Tokens"] = "MIT",
        ["System.Security.Cryptography.Pkcs"] = "MIT",
        ["System.IO.Packaging"] = "MIT",
        ["System.CodeDom"] = "MIT",
        ["System.Composition"] = "MIT",
        ["System.Composition.AttributedModel"] = "MIT",
        ["System.Composition.Convention"] = "MIT",
        ["System.Composition.Hosting"] = "MIT",
        ["System.Composition.Runtime"] = "MIT",
        ["System.Composition.TypedParts"] = "MIT",
        ["Microsoft.Bcl.AsyncInterfaces"] = "MIT",
        ["Microsoft.Build.Framework"] = "MIT",
        ["Microsoft.Build.Locator"] = "MIT",
        ["Microsoft.CodeAnalysis.Analyzers"] = "MIT",
        ["Microsoft.CodeAnalysis.Common"] = "MIT",
        ["Microsoft.CodeAnalysis.CSharp"] = "MIT",
        ["Microsoft.CodeAnalysis.CSharp.Workspaces"] = "MIT",
        ["Microsoft.CodeAnalysis.Workspaces.Common"] = "MIT",
        ["Microsoft.CodeAnalysis.Workspaces.MSBuild"] = "MIT",
        ["Microsoft.CodeCoverage"] = "MIT",
        ["Microsoft.NET.Test.Sdk"] = "MIT",
        ["Microsoft.OpenApi"] = "MIT",
        ["FluentValidation"] = "Apache-2.0",
        ["FluentValidation.AspNetCore"] = "Apache-2.0",
        ["FluentValidation.DependencyInjectionExtensions"] = "Apache-2.0",
        ["MailKit"] = "MIT",
        ["MimeKit"] = "MIT",
        ["Quartz"] = "Apache-2.0",
        ["SkiaSharp"] = "MIT",
        ["SkiaSharp.NativeAssets.macOS"] = "MIT",
        ["SkiaSharp.NativeAssets.Win32"] = "MIT",
        ["Quartz.Extensions.DependencyInjection"] = "Apache-2.0",
        ["Quartz.Extensions.Hosting"] = "Apache-2.0",
        ["BCrypt.Net-Next"] = "BSD-3-Clause",
        ["ClosedXML"] = "MIT",
        ["ClosedXML.Parser"] = "MIT",
        ["DocumentFormat.OpenXml"] = "MIT",
        ["DocumentFormat.OpenXml.Framework"] = "MIT",
        ["ExcelNumberFormat"] = "MIT",
        ["Npgsql"] = "PostgreSQL",
        ["Npgsql.EntityFrameworkCore.PostgreSQL"] = "PostgreSQL",
        ["Swashbuckle.AspNetCore"] = "MIT",
        ["Swashbuckle.AspNetCore.Swagger"] = "MIT",
        ["Swashbuckle.AspNetCore.SwaggerGen"] = "MIT",
        ["Swashbuckle.AspNetCore.SwaggerUI"] = "MIT",
        ["Asp.Versioning.Abstractions"] = "MIT",
        ["Asp.Versioning.Http"] = "MIT",
        ["Asp.Versioning.Mvc"] = "MIT",
        ["Asp.Versioning.Mvc.ApiExplorer"] = "MIT",
        ["BouncyCastle.Cryptography"] = "MIT",
        ["Humanizer.Core"] = "MIT",
        ["RBush"] = "MIT",
        ["SixLabors.Fonts"] = "Apache-2.0",
        ["coverlet.collector"] = "MIT",
        ["xunit"] = "Apache-2.0",
        ["xunit.abstractions"] = "MIT",
        ["xunit.analyzers"] = "Apache-2.0",
        ["xunit.assert"] = "Apache-2.0",
        ["xunit.core"] = "Apache-2.0",
        ["xunit.extensibility.core"] = "Apache-2.0",
        ["xunit.extensibility.execution"] = "Apache-2.0",
        ["xunit.runner.visualstudio"] = "MIT",
        ["Mono.TextTemplating"] = "MIT"
    };

    // 常见 npm 包许可证映射
    private static readonly Dictionary<string, string> NpmLicenseMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["vue"] = "MIT",
        ["vue-router"] = "MIT",
        ["pinia"] = "MIT",
        ["axios"] = "MIT",
        ["element-plus"] = "MIT",
        ["@element-plus/icons-vue"] = "MIT",
        ["marked"] = "MIT",
        ["uuid"] = "MIT",
        ["@types/uuid"] = "MIT",
        ["@types/node"] = "MIT",
        ["vite"] = "MIT",
        ["@vitejs/plugin-vue"] = "MIT",
        ["typescript"] = "Apache-2.0",
        ["vue-tsc"] = "MIT",
        ["eslint"] = "MIT",
        ["eslint-config-prettier"] = "MIT",
        ["eslint-plugin-vue"] = "MIT",
        ["@typescript-eslint/eslint-plugin"] = "MIT",
        ["@typescript-eslint/parser"] = "MIT",
        ["prettier"] = "MIT",
        ["vitest"] = "MIT",
        ["happy-dom"] = "MIT",
        ["@vue/tsconfig"] = "MIT",
        ["vue-eslint-parser"] = "MIT"
    };

    public static string ResolveNuGetLicense(string packageName)
    {
        if (NuGetLicenseMap.TryGetValue(packageName, out var license))
            return license;
        return "未知";
    }

    public static string ResolveNpmLicense(string packageName, string? repoRoot)
    {
        if (NpmLicenseMap.TryGetValue(packageName, out var license))
            return license;

        // 尝试从 node_modules/package.json 读取
        if (!string.IsNullOrWhiteSpace(repoRoot))
        {
            // 对 packageName 做路径净化，防止传入如 "../../etc/passwd" 等恶意值
            var safePackageName = Path.GetFileName(packageName.Trim().Replace("\\", "/").Replace("/", string.Empty));
            // repoRoot 由调用方传入受控的应用内容根目录，额外校验目标路径必须位于 repoRoot 之下
            var pkgJsonPath = Path.GetFullPath(Path.Combine(repoRoot, "frontend", "node_modules", safePackageName, "package.json"));
            var rootFullPath = Path.GetFullPath(repoRoot);
            if (pkgJsonPath.StartsWith(rootFullPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) && File.Exists(pkgJsonPath))
            {
                try
                {
                    // 目标路径已在上一步校验位于 repoRoot 下，safePackageName 也已净化
                    // nosemgrep: csharp.lang.security.filesystem.unsafe-path-combine.unsafe-path-combine
                    var json = File.ReadAllText(pkgJsonPath);
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("license", out var licenseProp) && licenseProp.ValueKind == JsonValueKind.String)
                        return licenseProp.GetString() ?? "未知";
                    if (doc.RootElement.TryGetProperty("licenses", out var licensesProp) && licensesProp.ValueKind == JsonValueKind.Array && licensesProp.GetArrayLength() > 0)
                    {
                        var first = licensesProp[0];
                        if (first.TryGetProperty("type", out var typeProp) && typeProp.ValueKind == JsonValueKind.String)
                            return typeProp.GetString() ?? "未知";
                    }
                }
                catch
                {
                    // ignore
                }
            }
        }

        return "未知";
    }
}
