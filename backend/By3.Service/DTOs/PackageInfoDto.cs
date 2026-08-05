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
/// 依赖包信息。
/// </summary>
public class PackageInfoDto
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string License { get; set; } = string.Empty;
}

/// <summary>
/// 项目依赖包列表。
/// </summary>
public class ProjectPackagesDto
{
    public string Project { get; set; } = string.Empty;
    public List<PackageInfoDto> Packages { get; set; } = new();
}

/// <summary>
/// 前端依赖包列表。
/// </summary>
public class FrontendPackagesDto
{
    public List<PackageInfoDto> Dependencies { get; set; } = new();
    public List<PackageInfoDto> DevDependencies { get; set; } = new();
}

/// <summary>
/// 系统全部依赖包信息。
/// </summary>
public class SystemPackagesDto
{
    public List<ProjectPackagesDto> Backend { get; set; } = new();
    public FrontendPackagesDto Frontend { get; set; } = new();
}
