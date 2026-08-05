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
public class DictDataControllerTests : IntegrationTestBase
{
    public DictDataControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Crud_DictData_Should_Work()
    {
        await LoginAsync();

        var typeResponse = await PostAsJsonAsync("/api/v1/dicttypes", new { dictName = "数据字典", dictType = "data_dict" });
        var typeResult = await DeserializeAsync<IdResponse>(typeResponse);
        var typeId = typeResult!.Id;

        var createResponse = await PostAsJsonAsync("/api/v1/dictdata", new { dictTypeId = typeId, dictLabel = "选项一", dictValue = "1", sortOrder = 1, isDefault = true });
        var createResult = await DeserializeAsync<IdResponse>(createResponse);
        var id = createResult!.Id;

        var getResponse = await Client.GetAsync($"/api/v1/dictdata/{id}");
        var data = await DeserializeAsync<DictDataDto>(getResponse);
        Assert.Equal("选项一", data!.DictLabel);

        var updateResponse = await PutAsJsonAsync($"/api/v1/dictdata/{id}", new { dictTypeId = typeId, dictLabel = "选项二", dictValue = "2", sortOrder = 2 });
        await DeserializeAsync<object>(updateResponse);

        var byTypeResult = await Client.GetFromJsonAsync<ApiResult<List<DictDataDto>>>($"/api/v1/dictdata/by-type/{typeId}");
        Assert.NotNull(byTypeResult);
        Assert.Single(byTypeResult!.Data!);

        var deleteResponse = await DeleteAsync($"/api/v1/dictdata/{id}");
        await DeserializeAsync<object>(deleteResponse);

        var deleteTypeResponse = await DeleteAsync($"/api/v1/dicttypes/{typeId}");
        await DeserializeAsync<object>(deleteTypeResponse);
    }

    private class IdResponse
    {
        public Guid Id { get; set; }
    }

    private class DictDataDto
    {
        public Guid Id { get; set; }
        public Guid DictTypeId { get; set; }
        public string DictLabel { get; set; } = string.Empty;
        public string DictValue { get; set; } = string.Empty;
    }
}
