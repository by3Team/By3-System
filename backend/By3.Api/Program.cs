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

using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using Asp.Versioning;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using By3.Api.Authorization;
using By3.Api.Filters;
using By3.Api.Middleware;
using By3.Api.Options;
using By3.Service;
using By3.Service.DTOs;
using By3.Service.Services;
using By3.Service.Validators;

var builder = WebApplication.CreateBuilder(args);

// 内存缓存
builder.Services.AddMemoryCache();

// 响应压缩
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

var isIntegrationTest = builder.Environment.EnvironmentName == "IntegrationTests";

// 限流（固定窗口）
if (!isIntegrationTest)
{
    var rateLimiting = builder.Configuration.GetRequiredSection("RateLimiting").Get<RateLimitingOptions>()
        ?? throw new InvalidOperationException("RateLimiting 配置无效。");

    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = rateLimiting.GlobalPermitLimit,
                    Window = TimeSpan.FromMinutes(rateLimiting.GlobalWindowMinutes),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = rateLimiting.GlobalQueueLimit
                }));

        options.AddFixedWindowLimiter("default", opt =>
        {
            opt.PermitLimit = rateLimiting.DefaultPermitLimit;
            opt.Window = TimeSpan.FromMinutes(rateLimiting.DefaultWindowMinutes);
            opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = rateLimiting.DefaultQueueLimit;
        });

        // 登录接口单独限制，防止暴力破解
        options.AddFixedWindowLimiter("login", opt =>
        {
            opt.PermitLimit = rateLimiting.LoginPermitLimit;
            opt.Window = TimeSpan.FromMinutes(rateLimiting.LoginWindowMinutes);
            opt.QueueLimit = rateLimiting.LoginQueueLimit;
        });

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });
}

// FluentValidation 自动验证
builder.Services.AddValidatorsFromAssemblyContaining<LoginValidator>();
builder.Services.AddFluentValidationAutoValidation();

// HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// 分层 DI 注册
By3.Repository.DependencyInjection.AddBy3Repositories(builder.Services, builder.Configuration);
builder.Services.AddBy3Services();

// Filters
builder.Services.AddScoped<GlobalExceptionFilter>();
builder.Services.AddScoped<IdempotencyFilter>();
builder.Services.AddScoped<AuditLogFilter>();

// API 版本控制
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Controllers + Filters
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
    options.Filters.Add<RequestBodyCaptureFilter>();
    options.Filters.Add<AuditLogFilter>();
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "该字段不能为空");
})
.ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .SelectMany(e => e.Value!.Errors.Select(err => new { Field = e.Key, Message = err.ErrorMessage }));
        return new BadRequestObjectResult(ApiResult<object>.Error("请求参数错误", 400, errors));
    };
});

// 文件上传大小限制
var fileUpload = builder.Configuration.GetRequiredSection("FileUpload").Get<FileUploadOptions>()
    ?? throw new InvalidOperationException("FileUpload 配置无效。");

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = fileUpload.MaxRequestBodySize;
});
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = fileUpload.MultipartBodyLengthLimit;
    options.ValueLengthLimit = fileUpload.ValueLengthLimit;
});

// JWT
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("Jwt:Key 未配置，请在环境变量或 User Secrets 中设置。");
if (jwtKey.Length < 32)
    throw new InvalidOperationException("Jwt:Key 长度至少 32 字节。");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"]!,
            ValidAudience = builder.Configuration["Jwt:Audience"]!,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var authService = context.HttpContext.RequestServices.GetRequiredService<AuthService>();
                var token = context.Request.Headers.Authorization.FirstOrDefault()?.Substring("Bearer ".Length);
                if (!string.IsNullOrEmpty(token) && authService.IsTokenBlacklisted(token))
                {
                    context.Fail("Token 已失效");
                }
                await Task.CompletedTask;
            }
        };
    });

builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddSingleton<IConfigureOptions<AuthorizationOptions>, AuthorizationOptionsConfigurator>();
builder.Services.AddAuthorization();

// CORS
var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"];
var origins = string.IsNullOrWhiteSpace(allowedOrigins)
    ? Array.Empty<string>()
    : allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy =>
    {
        if (origins.Length > 0)
        {
            policy.WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
        else
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "By3 API",
        Version = "v1"
    });

    // 加载 XML 注释文件，使 Swagger 显示控制器和 Action 的说明
    var xmlFiles = new[]
    {
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml",
        "By3.Service.xml",
        "By3.Repository.xml"
    };
    foreach (var xmlFile in xmlFiles)
    {
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    }

    // Swagger 支持 JWT 认证
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "请输入 JWT Token，格式：Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// HTTPS 强制跳转（生产环境）
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();

// 安全响应头
app.UseMiddleware<SecurityHeadersMiddleware>();

// 请求计时与日志
app.UseMiddleware<RequestTimingMiddleware>();

// 限流
if (!isIntegrationTest)
{
    app.UseRateLimiter();
}

// Swagger 开关：默认仅 Development 启用，也可通过配置 Swagger:IsEnabled 强制开启或关闭
var swaggerEnabled = builder.Configuration.GetValue<bool?>("Swagger:IsEnabled")
    ?? app.Environment.IsDevelopment();

if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "By3 API v1");
    });
}

app.UseResponseCompression();
app.UseCors("DefaultCors");

// 对外 API 独立签名认证（路径 /api/external/**），放在 JWT 认证之前
app.UseMiddleware<ExternalApiAuthenticationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 自动迁移并初始化种子数据
await DatabaseInitializer.InitializeAsync(app.Services);

app.Run();
