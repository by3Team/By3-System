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
using System.Xml.Linq;
using By3.Service.DTOs;

namespace By3.Service.Services;

public class SystemInfoService
{
    /// <summary>
    /// 获取系统前后端依赖包及许可证信息。
    /// </summary>
    public SystemPackagesDto GetPackages()
    {
        var repoRoot = FindRepoRoot();
        var result = new SystemPackagesDto
        {
            Backend = repoRoot == null ? new List<ProjectPackagesDto>() : ReadBackendPackages(repoRoot),
            Frontend = repoRoot == null ? new FrontendPackagesDto() : ReadFrontendPackages(repoRoot)
        };
        return result;
    }

    /// <summary>
    /// 向上查找项目根目录（通过定位 frontend/package.json）。
    /// </summary>
    private static string? FindRepoRoot()
    {
        // 通过查找 frontend/package.json 定位项目根目录
        foreach (var startDir in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory, Path.GetDirectoryName(typeof(SystemInfoService).Assembly.Location) })
        {
            if (string.IsNullOrWhiteSpace(startDir)) continue;
            var dir = new DirectoryInfo(startDir);
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "frontend", "package.json")))
                    return dir.FullName;
                dir = dir.Parent;
            }
        }
        return null;
    }



    /// <summary>
    /// 读取后端各 .csproj 中的 NuGet 包引用及许可证信息。
    /// </summary>
    private static List<ProjectPackagesDto> ReadBackendPackages(string solutionRoot)
    {
        var backendDir = Path.Combine(solutionRoot, "backend");
        if (!Directory.Exists(backendDir)) return new List<ProjectPackagesDto>();

        var result = new List<ProjectPackagesDto>();
        foreach (var csproj in Directory.GetFiles(backendDir, "*.csproj", SearchOption.AllDirectories))
        {
            var projectName = Path.GetFileNameWithoutExtension(csproj);
            var packages = new List<PackageInfoDto>();
            try
            {
                var doc = XDocument.Load(csproj);
                var packageRefs = doc.Descendants("PackageReference");
                foreach (var pr in packageRefs)
                {
                    var name = pr.Attribute("Include")?.Value;
                    var version = pr.Attribute("Version")?.Value;
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        packages.Add(new PackageInfoDto { Name = name, Version = version ?? "", License = LicenseResolver.ResolveNuGetLicense(name) });
                    }
                }
            }
            catch
            {
                // 读取失败则跳过该项目
            }

            if (packages.Count > 0)
            {
                result.Add(new ProjectPackagesDto { Project = projectName, Packages = packages });
            }
        }

        return result.OrderBy(p => p.Project).ToList();
    }

    /// <summary>
    /// 读取前端 package.json 中的 npm 依赖及许可证信息。
    /// </summary>
    private static FrontendPackagesDto ReadFrontendPackages(string solutionRoot)
    {
        var packageJsonPath = Path.Combine(solutionRoot, "frontend", "package.json");
        var result = new FrontendPackagesDto();
        if (!File.Exists(packageJsonPath)) return result;

        try
        {
            var json = File.ReadAllText(packageJsonPath);
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("dependencies", out var deps))
            {
                result.Dependencies = deps.EnumerateObject()
                    .Select(p => new PackageInfoDto
                    {
                        Name = p.Name,
                        Version = p.Value.GetString() ?? "",
                        License = LicenseResolver.ResolveNpmLicense(p.Name, solutionRoot)
                    })
                    .OrderBy(p => p.Name)
                    .ToList();
            }
            if (doc.RootElement.TryGetProperty("devDependencies", out var devDeps))
            {
                result.DevDependencies = devDeps.EnumerateObject()
                    .Select(p => new PackageInfoDto
                    {
                        Name = p.Name,
                        Version = p.Value.GetString() ?? "",
                        License = LicenseResolver.ResolveNpmLicense(p.Name, solutionRoot)
                    })
                    .OrderBy(p => p.Name)
                    .ToList();
            }
        }
        catch
        {
            // 读取失败返回空
        }

        return result;
    }
}
