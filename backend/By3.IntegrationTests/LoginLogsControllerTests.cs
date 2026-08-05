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
public class LoginLogsControllerTests : IntegrationTestBase
{
    public LoginLogsControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetList_Should_Return_Paged_Logs()
    {
        await LoginAsync();
        var result = await Client.GetFromJsonAsync<ApiResult<PageResult<LoginLogDto>>>("/api/v1/loginlogs?page=1&pageSize=10");
        Assert.NotNull(result);
        Assert.Equal(200, result!.Code);
    }

    private class LoginLogDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
    }
}
