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
public class EmailTemplatesControllerTests : IntegrationTestBase
{
    public EmailTemplatesControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Crud_Template_And_Version_Should_Work()
    {
        await LoginAsync();

        var createResponse = await PostAsJsonAsync("/api/v1/emailtemplates", new
        {
            templateCode = $"TPL{Guid.NewGuid():N}",
            templateName = "测试模板",
            description = "测试描述"
        });
        var createResult = await DeserializeAsync<IdResponse>(createResponse);
        var templateId = createResult!.Id;

        var getResponse = await Client.GetAsync($"/api/v1/emailtemplates/{templateId}");
        var template = await DeserializeAsync<EmailTemplateDto>(getResponse);
        Assert.Equal("测试模板", template!.TemplateName);

        var updateResponse = await PutAsJsonAsync($"/api/v1/emailtemplates/{templateId}", new { templateName = "更新模板" });
        await DeserializeAsync<object>(updateResponse);

        var versionResponse = await PostAsJsonAsync("/api/v1/emailtemplates/versions", new
        {
            templateId = templateId,
            version = "v1.0",
            subject = "测试邮件 {{name}}",
            body = "<h1>你好 {{name}}</h1>"
        });
        var versionResult = await DeserializeAsync<IdResponse>(versionResponse);
        var versionId = versionResult!.Id;

        var versionsResponse = await Client.GetAsync($"/api/v1/emailtemplates/{templateId}/versions");
        var versions = await DeserializeAsync<List<EmailTemplateVersionDto>>(versionsResponse);
        Assert.Single(versions!);
        Assert.Equal("v1.0", versions![0].Version);

        var versionUpdateResponse = await PutAsJsonAsync($"/api/v1/emailtemplates/versions/{versionId}", new { subject = "更新主题 {{name}}" });
        await DeserializeAsync<object>(versionUpdateResponse);

        var versionDeleteResponse = await DeleteAsync($"/api/v1/emailtemplates/versions/{versionId}");
        await DeserializeAsync<object>(versionDeleteResponse);

        var deleteResponse = await DeleteAsync($"/api/v1/emailtemplates/{templateId}");
        await DeserializeAsync<object>(deleteResponse);
    }

    [Fact]
    public async Task Send_Batch_Should_Create_Logs_When_Smtp_Fails()
    {
        await LoginAsync();

        var createResponse = await PostAsJsonAsync("/api/v1/emailtemplates", new
        {
            templateCode = $"TPL{Guid.NewGuid():N}",
            templateName = "发送测试模板",
            description = "用于测试发送"
        });
        var createResult = await DeserializeAsync<IdResponse>(createResponse);
        var templateId = createResult!.Id;

        await PostAsJsonAsync("/api/v1/emailtemplates/versions", new
        {
            templateId = templateId,
            version = "v1.0",
            subject = "测试 {{name}}",
            body = "你好 {{name}}"
        });

        var sendRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/emailtemplates/send")
        {
            Content = JsonContent.Create(new
            {
                templateId = templateId,
                version = "v1.0",
                toAddresses = new List<string> { "test1@example.com", "test2@example.com" },
                variables = new Dictionary<string, string> { ["name"] = "Tester" }
            })
        };
        foreach (var h in GetHeaders())
            sendRequest.Headers.TryAddWithoutValidation(h.Key, h.Value);

        var sendResponse = await Client.SendAsync(sendRequest);
        // SMTP 未配置时会失败，但日志应该已被创建
        if (!sendResponse.IsSuccessStatusCode)
        {
            var logsResult = await Client.GetFromJsonAsync<ApiResult<PageResult<EmailLogDto>>>($"/api/v1/emailtemplates/logs?page=1&pageSize=10&keyword={templateId}");
            Assert.NotNull(logsResult);
            Assert.Equal(200, logsResult!.Code);
            Assert.True(logsResult.Data!.Items.Count >= 2);
        }

        var deleteResponse = await DeleteAsync($"/api/v1/emailtemplates/{templateId}");
        await DeserializeAsync<object>(deleteResponse);
    }

    private class IdResponse
    {
        public Guid Id { get; set; }
    }
}
