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
public class DictTypesControllerTests : IntegrationTestBase
{
    public DictTypesControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Crud_DictType_Should_Work()
    {
        await LoginAsync();

        var createResponse = await PostAsJsonAsync("/api/v1/dicttypes", new { dictName = "测试字典", dictType = "test_dict" });
        var createResult = await DeserializeAsync<IdResponse>(createResponse);
        var id = createResult!.Id;

        var getResponse = await Client.GetAsync($"/api/v1/dicttypes/{id}");
        var type = await DeserializeAsync<DictTypeDto>(getResponse);
        Assert.Equal("测试字典", type!.DictName);

        var updateResponse = await PutAsJsonAsync($"/api/v1/dicttypes/{id}", new { dictName = "更新字典", dictType = "test_dict_updated" });
        await DeserializeAsync<object>(updateResponse);

        var listResult = await Client.GetFromJsonAsync<ApiResult<PageResult<DictTypeDto>>>("/api/v1/dicttypes?page=1&pageSize=10");
        Assert.NotNull(listResult);
        Assert.Single(listResult!.Data!.Items);

        var deleteResponse = await DeleteAsync($"/api/v1/dicttypes/{id}");
        await DeserializeAsync<object>(deleteResponse);
    }

    private class IdResponse
    {
        public Guid Id { get; set; }
    }

    private class DictTypeDto
    {
        public Guid Id { get; set; }
        public string DictName { get; set; } = string.Empty;
        public string DictType { get; set; } = string.Empty;
    }
}
