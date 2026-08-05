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

using System.Net.Http.Json;
using By3.Service.DTOs;

namespace By3.IntegrationTests;

[Collection("Integration Tests")]
public class RolesControllerTests : IntegrationTestBase
{
    public RolesControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Crud_Role_Should_Work()
    {
        await LoginAsync();

        var createResponse = await PostAsJsonAsync("/api/v1/roles", new { roleName = "测试角色", description = "测试", menuIds = new List<Guid>() });
        var createResult = await DeserializeAsync<IdResponse>(createResponse);
        var id = createResult!.Id;

        var getResponse = await Client.GetAsync($"/api/v1/roles/{id}");
        var role = await DeserializeAsync<RoleDto>(getResponse);
        Assert.Equal("测试角色", role!.RoleName);

        var updateResponse = await PutAsJsonAsync($"/api/v1/roles/{id}", new { roleName = "更新角色", description = "已更新", menuIds = new List<Guid>() });
        await DeserializeAsync<object>(updateResponse);

        var deleteResponse = await DeleteAsync($"/api/v1/roles/{id}");
        await DeserializeAsync<object>(deleteResponse);
    }

    [Fact]
    public async Task GetAll_Should_Return_Roles()
    {
        await LoginAsync();
        var result = await Client.GetFromJsonAsync<ApiResult<List<RoleDto>>>("/api/v1/roles/all");
        Assert.NotNull(result);
        Assert.Equal(200, result!.Code);
        Assert.NotEmpty(result.Data!);
    }

    private class IdResponse
    {
        public Guid Id { get; set; }
    }

    private class RoleDto
    {
        public Guid Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
