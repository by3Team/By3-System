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
public class FilesControllerTests : IntegrationTestBase
{
    public FilesControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task SingleFile_Upload_And_Download_Should_Work()
    {
        await LoginAsync();

        var content = new MultipartFormDataContent();
        var fileContent = new StringContent("hello single file");
        content.Add(fileContent, "file", "test.txt");
        content.Add(new StringContent("documents"), "category");

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/singlefiles/upload")
        {
            Content = content
        };
        foreach (var h in GetHeaders())
            request.Headers.TryAddWithoutValidation(h.Key, h.Value);

        var uploadResponse = await Client.SendAsync(request);
        var uploadResult = await DeserializeAsync<FileUploadResultDto>(uploadResponse);
        Assert.Equal("test.txt", uploadResult!.OriginalFileName);

        var downloadResponse = await Client.GetAsync($"/api/v1/singlefiles/{uploadResult.Id}/download");
        downloadResponse.EnsureSuccessStatusCode();
        var downloaded = await downloadResponse.Content.ReadAsStringAsync();
        Assert.Equal("hello single file", downloaded);
    }

    [Fact]
    public async Task MultiFile_Upload_List_Delete_And_Export_Should_Work()
    {
        await LoginAsync();

        var content = new MultipartFormDataContent();
        content.Add(new StringContent("file a"), "files", "a.txt");
        content.Add(new StringContent("file b"), "files", "b.txt");
        content.Add(new StringContent("general"), "category");

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/multifiles/upload")
        {
            Content = content
        };
        foreach (var h in GetHeaders())
            request.Headers.TryAddWithoutValidation(h.Key, h.Value);

        var uploadResponse = await Client.SendAsync(request);
        var uploadResults = await DeserializeAsync<List<FileUploadResultDto>>(uploadResponse);
        Assert.Equal(2, uploadResults!.Count);

        var listResult = await Client.GetFromJsonAsync<ApiResult<PageResult<FileRecordDto>>>("/api/v1/multifiles?page=1&pageSize=10&category=general");
        Assert.NotNull(listResult);
        Assert.Equal(200, listResult!.Code);
        Assert.True(listResult.Data!.Items.Count >= 2);

        var exportResponse = await Client.GetAsync("/api/v1/multifiles/export?category=general");
        exportResponse.EnsureSuccessStatusCode();
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", exportResponse.Content.Headers.ContentType?.MediaType);
        var bytes = await exportResponse.Content.ReadAsByteArrayAsync();
        Assert.True(bytes.Length > 0);

        foreach (var r in uploadResults)
        {
            var deleteResponse = await DeleteAsync($"/api/v1/multifiles/{r.Id}");
            await DeserializeAsync<object>(deleteResponse);
        }
    }
}
