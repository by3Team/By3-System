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

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using By3.Service.DTOs;

namespace By3.IntegrationTests;

public abstract class IntegrationTestBase : IDisposable
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    private string? _token;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        Factory.ResetDatabaseAsync().GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        Client.Dispose();
    }

    protected async Task<string> LoginAsync(string userName = "admin", string password = "Demo123!")
    {
        if (_token != null) return _token;

        var response = await PostAsJsonAsync("/api/v1/auth/login", new { UserName = userName, Password = password });
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Login failed: {response.StatusCode}, {body}");
        }
        using var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();
        var root = doc!.RootElement.GetProperty("data");
        _token = root.GetProperty("token").GetString()!;
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        return _token;
    }

    protected Dictionary<string, string> GetHeaders(string? idempotencyKey = null)
    {
        var headers = new Dictionary<string, string>
        {
            ["Idempotency-Key"] = idempotencyKey ?? Guid.NewGuid().ToString()
        };
        return headers;
    }

    protected async Task<T?> DeserializeAsync<T>(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        var apiResult = await response.Content.ReadFromJsonAsync<ApiResult<T>>();
        Assert.NotNull(apiResult);
        Assert.Equal(200, apiResult!.Code);
        return apiResult.Data;
    }

    protected async Task<HttpResponseMessage> PostAsJsonAsync<T>(string url, T data, string? idempotencyKey = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(data)
        };
        foreach (var h in GetHeaders(idempotencyKey))
            request.Headers.TryAddWithoutValidation(h.Key, h.Value);
        return await Client.SendAsync(request);
    }

    protected async Task<HttpResponseMessage> PutAsJsonAsync<T>(string url, T data, string? idempotencyKey = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = JsonContent.Create(data)
        };
        foreach (var h in GetHeaders(idempotencyKey))
            request.Headers.TryAddWithoutValidation(h.Key, h.Value);
        return await Client.SendAsync(request);
    }

    protected async Task<HttpResponseMessage> DeleteAsync(string url, string? idempotencyKey = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        foreach (var h in GetHeaders(idempotencyKey))
            request.Headers.TryAddWithoutValidation(h.Key, h.Value);
        return await Client.SendAsync(request);
    }

}
