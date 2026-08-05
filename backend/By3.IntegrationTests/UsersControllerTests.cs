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
public class UsersControllerTests : IntegrationTestBase
{
    public UsersControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetList_Should_Return_Paged_Users()
    {
        await LoginAsync();
        var result = await Client.GetFromJsonAsync<ApiResult<PageResult<UserListDto>>>("/api/v1/users?page=1&pageSize=10");
        Assert.NotNull(result);
        Assert.Equal(200, result!.Code);
        Assert.Single(result.Data!.Items);
    }

    [Fact]
    public async Task Crud_User_Should_Work()
    {
        await LoginAsync();

        var createResponse = await PostAsJsonAsync("/api/v1/users", new { userName = "testuser", password = "Test123!", realName = "测试用户", email = "test@example.com", phone = "13800138000", roleIds = new List<Guid>() });
        var createResult = await DeserializeAsync<IdResponse>(createResponse);
        var id = createResult!.Id;

        var getResponse = await Client.GetAsync($"/api/v1/users/{id}");
        var user = await DeserializeAsync<UserDetailDto>(getResponse);
        Assert.Equal("testuser", user!.UserName);

        var updateResponse = await PutAsJsonAsync($"/api/v1/users/{id}", new { realName = "更新用户", email = "updated@example.com", phone = "13800138001", roleIds = new List<Guid>() });
        await DeserializeAsync<object>(updateResponse);

        var resetResponse = await PostAsJsonAsync<object>($"/api/v1/users/{id}/reset-password", new { newPassword = "NewPass123!" });
        await DeserializeAsync<object>(resetResponse);

        var deleteResponse = await DeleteAsync($"/api/v1/users/{id}");
        await DeserializeAsync<object>(deleteResponse);
    }

    private class IdResponse
    {
        public Guid Id { get; set; }
    }

    private class UserListDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string RealName { get; set; } = string.Empty;
    }

    private class UserDetailDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string RealName { get; set; } = string.Empty;
    }
}
