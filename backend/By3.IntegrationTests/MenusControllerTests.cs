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
public class MenusControllerTests : IntegrationTestBase
{
    public MenusControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAll_Should_Return_Menu_Tree()
    {
        await LoginAsync();
        var result = await Client.GetFromJsonAsync<ApiResult<List<MenuTreeDto>>>("/api/v1/menus");
        Assert.NotNull(result);
        Assert.Equal(200, result!.Code);
        Assert.NotEmpty(result.Data!);

        var roots = result.Data!;
        Assert.Equal(4, roots.Count);
        Assert.Contains(roots, r => r.MenuName == "系统管理");
        Assert.Contains(roots, r => r.MenuName == "文件管理");
        Assert.Contains(roots, r => r.MenuName == "邮件管理");
        Assert.Contains(roots, r => r.MenuName == "日志管理");

        var fileRoot = roots.Single(r => r.MenuName == "文件管理");
        Assert.Equal(3, fileRoot.Children.Count);
        Assert.All(fileRoot.Children, c => Assert.Equal(fileRoot.Id, c.ParentId));

        var emailRoot = roots.Single(r => r.MenuName == "邮件管理");
        Assert.Equal(6, emailRoot.Children.Count);
        Assert.All(emailRoot.Children, c => Assert.Equal(emailRoot.Id, c.ParentId));
    }

    [Fact]
    public async Task Crud_Menu_Should_Work()
    {
        await LoginAsync();

        var createResponse = await PostAsJsonAsync("/api/v1/menus", new { menuName = "测试菜单", menuType = 2, route = "/test", component = "test/index", permission = "test:list", sortOrder = 99 });
        var createResult = await DeserializeAsync<IdResponse>(createResponse);
        var id = createResult!.Id;

        var getResponse = await Client.GetAsync($"/api/v1/menus/{id}");
        var menu = await DeserializeAsync<MenuTreeDto>(getResponse);
        Assert.Equal("测试菜单", menu!.MenuName);

        var updateResponse = await PutAsJsonAsync($"/api/v1/menus/{id}", new { menuName = "更新菜单" });
        await DeserializeAsync<object>(updateResponse);

        var deleteResponse = await DeleteAsync($"/api/v1/menus/{id}");
        await DeserializeAsync<object>(deleteResponse);
    }

    private class IdResponse
    {
        public Guid Id { get; set; }
    }

    private class MenuTreeDto
    {
        public Guid Id { get; set; }
        public string MenuName { get; set; } = string.Empty;
        public Guid? ParentId { get; set; }
        public List<MenuTreeDto> Children { get; set; } = new();
    }
}
