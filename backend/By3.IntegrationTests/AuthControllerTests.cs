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

namespace By3.IntegrationTests;

[Collection("Integration Tests")]
public class AuthControllerTests : IntegrationTestBase
{
    public AuthControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Login_With_Valid_Credentials_Should_Return_Token()
    {
        var response = await PostAsJsonAsync("/api/v1/auth/login", new { userName = "admin", password = "admin123" });
        var result = await DeserializeAsync<TokenResponse>(response);
        Assert.False(string.IsNullOrEmpty(result!.Token));
    }

    [Fact]
    public async Task Login_With_Invalid_Credentials_Should_Return_400()
    {
        var response = await PostAsJsonAsync("/api/v1/auth/login", new { userName = "admin", password = "wrong" });
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetUserInfo_Should_Return_Current_User()
    {
        await LoginAsync();
        var response = await Client.GetAsync("/api/v1/auth/info");
        var result = await DeserializeAsync<UserInfoResponse>(response);
        Assert.Equal("admin", result!.UserName);
        Assert.Contains("user:list", result.Permissions);
    }

    private class TokenResponse
    {
        public string Token { get; set; } = string.Empty;
    }

    private class UserInfoResponse
    {
        public string UserName { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
    }
}
