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
public class DepartmentsControllerTests : IntegrationTestBase
{
    public DepartmentsControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Crud_Department_Should_Work()
    {
        await LoginAsync();

        var createResponse = await PostAsJsonAsync("/api/v1/departments", new { deptName = "测试部门", deptCode = "TEST", sortOrder = 1 });
        var createResult = await DeserializeAsync<IdResponse>(createResponse);
        var id = createResult!.Id;

        var getResponse = await Client.GetAsync($"/api/v1/departments/{id}");
        var dept = await DeserializeAsync<DepartmentTreeDto>(getResponse);
        Assert.Equal("测试部门", dept!.DeptName);

        var updateResponse = await PutAsJsonAsync($"/api/v1/departments/{id}", new { deptName = "更新部门", deptCode = "UPD", sortOrder = 2 });
        await DeserializeAsync<object>(updateResponse);

        var treeResult = await Client.GetFromJsonAsync<ApiResult<List<DepartmentTreeDto>>>("/api/v1/departments");
        Assert.NotNull(treeResult);
        Assert.Single(treeResult!.Data!);

        var deleteResponse = await DeleteAsync($"/api/v1/departments/{id}");
        await DeserializeAsync<object>(deleteResponse);
    }

    private class IdResponse
    {
        public Guid Id { get; set; }
    }

    private class DepartmentTreeDto
    {
        public Guid Id { get; set; }
        public string DeptName { get; set; } = string.Empty;
        public string? DeptCode { get; set; }
        public List<DepartmentTreeDto> Children { get; set; } = new();
    }
}
