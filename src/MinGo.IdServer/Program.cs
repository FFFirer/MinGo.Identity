// Copyright (c) SimpleIdServer. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using MinGo.IdServer;
using MinGo.IdServer.Conf;
using MinGo.Identity.Configuration;

var webApplicationBuilder = WebApplication.CreateBuilder(args);

// Serilog: 接管日志管线，配置由 appsettings 驱动
webApplicationBuilder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});
webApplicationBuilder.Services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()));
webApplicationBuilder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{webApplicationBuilder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();
var identityServerConfiguration = webApplicationBuilder.Configuration.Get<IdentityServerConfiguration>() ?? throw new InvalidOperationException("Configuration section 'IdentityServerConfiguration' is missing");
SidServerSetup.ConfigureIdServer(webApplicationBuilder, identityServerConfiguration);

// Override ForwardedHeadersOptions to include X-Forwarded-Host and trust container-internal proxies.
// SimpleIdServer's ForwardHttpHeader() only sets XForwardedFor | XForwardedProto, missing Host.
// See openspec/changes/fix-reverse-proxy-forwarded-host/design.md for root cause analysis.
if (identityServerConfiguration.IsForwardedEnabled)
{
    webApplicationBuilder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
                                 | ForwardedHeaders.XForwardedProto
                                 | ForwardedHeaders.XForwardedHost;
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
    });
}

var app = webApplicationBuilder.Build();

// ────────────────────────────────────────────────────────────────
// Health check probes (K8s style: /healthz, /readyz, /startupz)
// Mapped before business middleware so they remain accessible even
// when other middleware is degraded.
// ────────────────────────────────────────────────────────────────
app.MapIdentityHealthChecks();

// Seed 数据迁移至 MinGo.Identity.Migrator，部署时统一执行
app.UseCors("AllowAll");
app.UseSid()
    .UseSidSwagger()
    .UseSidSwaggerUi();
app.Run();