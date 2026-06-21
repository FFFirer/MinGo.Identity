// Copyright (c) SimpleIdServer. AllClients rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Community.Microsoft.Extensions.Caching.PostgreSql;
using FormBuilder;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.SQLite;
using MassTransit;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NeoSmart.Caching.Sqlite;using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using SimpleIdServer.Configuration;
using SimpleIdServer.Configuration.Redis;
using SimpleIdServer.IdServer.Captcha;
using SimpleIdServer.IdServer.Domains;
using SimpleIdServer.IdServer.Options;
using MinGo.IdServer.Configurations;
using MinGo.Identity.Configuration;
using MinGo.IdServer.Consumers;
using SimpleIdServer.IdServer.TokenTypes;
using DataSeeder;
using MinGo.IdServer.Conf.Migrations.AfterDeployment;
using MinGo.IdServer.Conf.Migrations.BeforeDeployment;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Transactions;

namespace MinGo.IdServer.Conf;

public class SidServerSetup
{
    public static void ConfigureIdServer(WebApplicationBuilder webApplicationBuilder, IdentityServerConfiguration configuration)
    {
        webApplicationBuilder.Services.AddSingleton(configuration);
        var section = webApplicationBuilder.Configuration.GetSection(nameof(ScimClientOptions));
        var conf = section.Get<ScimClientOptions>();
        var openTelemetrySection = webApplicationBuilder.Configuration.GetSection(nameof(OpenTelemetryOptions));
        var openTelemetryOptions = openTelemetrySection.Get<OpenTelemetryOptions>() ?? throw new InvalidOperationException($"Configuration section '{nameof(OpenTelemetryOptions)}' is missing");
        var idServerBuilder = webApplicationBuilder.AddSidIdentityServer(c =>
            {
                c.ForceHttpsEnabled = configuration.ForceHttps;
                c.Authority = configuration.Authority;
                c.ScimClientOptions = conf;
                if (!string.IsNullOrWhiteSpace(configuration.SessionCookieNamePrefix))
                {
                    c.SessionCookieName = configuration.SessionCookieNamePrefix;
                }
            }, true)
            .IgnoreCertificateError()
            .ConfigureFormBuilder(c =>
            {
                c.AddTailwindcss();
            })
            // 注意：迁移现由 MinGo.Identity.Migrator 独立管理
            // 多节点部署时通过部署流水线执行，非启动时自动执行
            .AddDuendeMigration(a =>
            {
                ConfigureDuendeMigration(webApplicationBuilder, a);
            })
            .AddOpeniddictMigration(a =>
            {
                ConfigureOpeniddictMigration(webApplicationBuilder, a);
            })
            .EnableMigration()
            .AddPwdAuthentication(true)
            .AddEmailAuthentication()
            .AddOtpAuthentication()
            .AddSmsAuthentication()
            .AddMobileAuthentication(o =>
            {
                var authority = configuration.Authority;
                var url = new Uri(authority);
                o.ServerName = "SimpleIdServer";
                o.ServerDomain = url.Host;
                o.Origins = new HashSet<string> { authority };
            })
            .AddWebauthnAuthentication()
            .AddVpAuthentication()
            .AddSamlIdp()
            .AddWsFederation()
            .AddOpenidFederation(o =>
            {
                o.IsFederationEnabled = true;
            })
            .AddScimProvisioning()
            .AddLdapProvisioning()
            .AddFcmNotification()
            .AddGotifyNotification()
            .EnableV2Recaptcha(c =>
            {
                ConfigureV2ReCaptcha(webApplicationBuilder, c);
            })
            .AddSwagger(opt =>
            {
                opt.IncludeDocumentation<AccessTokenTypeService>();
            })
            .ConfigureHangfire((c) =>
            {
                ConfigureHangfire(webApplicationBuilder, c);
            })
            .ConfigureKeyValueStore((c) =>
            {
                ConfigureKeyValue(webApplicationBuilder, c);
            })
            .EnableMasstransit(o =>
            {
                o.AddConsumer<IdServerEventsConsumer>();
                o.ConfigureMigration();
                ConfigureMessageBroker(webApplicationBuilder, o);
            }, () => ConfigureMessageBrokerMigration(webApplicationBuilder))
            .SeedAdministrationData(
                new List<string> { $"{configuration.AdminUiUrl}/signout-callback-oidc" },
                new List<string> { $"{configuration.AdminUiUrl}/signout-callback-oidc" },
                string.Empty,
                new List<Scope>())
            .SeedSwagger(
                new List<string> { }
            )
            .UseEfStore((c) =>
            {
                ConfigureIdserverStorage(webApplicationBuilder, c);
            }, (c) =>
            {
                ConfigureFormbuilderStorage(webApplicationBuilder, c);
            })
            .EnableOpenTelemetry(m =>
            {
                ConfigureMetrics(m, openTelemetryOptions);
            }, t =>
            {
                ConfigureTraces(t, openTelemetryOptions);
            });
        if (configuration.IsForwardedEnabled)
        {
            idServerBuilder.ForwardHttpHeader();
        }

        if (configuration.IsFapiEnabled)
        {
            idServerBuilder.EnableFapiSecurityProfile(clientCertificate: configuration.ClientCertificateMode ?? ClientCertificateMode.AllowCertificate, callback: cb =>
            {
                cb.AllowedCertificateTypes = CertificateTypes.All;
                cb.RevocationMode = X509RevocationMode.NoCheck;
            });
        }

        if (configuration.IsRealmEnabled)
        {
            idServerBuilder.EnableRealm();
        }

        ConfigureDistributedCache(webApplicationBuilder);

        // ────────────────────────────────────────────────────────────
        // Health checks — config-driven, auto-registers checks for
        // configured storage, key-value store, and message broker.
        // ────────────────────────────────────────────────────────────
        webApplicationBuilder.Services.AddIdentityHealthChecks(webApplicationBuilder.Configuration);

        // ────────────────────────────────────────────────────────────
        // After-deployment seeder — 补全 SIDS-manager redirect URL 等
        // 由 MinGo.Identity.Migrator 统一执行 SeedData() 时调用
        // ────────────────────────────────────────────────────────────
        ConfigureDataseeder(webApplicationBuilder, configuration);
    }

    private static void ConfigureDuendeMigration(WebApplicationBuilder builder, DbContextOptionsBuilder db)
    {
        var section = builder.Configuration.GetSection(nameof(DuendeMigrationOptions));
        var conf = section.Get<DuendeMigrationOptions>() ?? throw new InvalidOperationException($"Configuration section '{nameof(DuendeMigrationOptions)}' is missing");
        switch(conf.Transport)
        {
            case StorageTypes.MYSQL:
                throw new NotSupportedException("MySQL storage is not supported. Use PostgreSQL instead.");
                // db.UseMySql(conf.ConnectionString, ServerVersion.AutoDetect(conf.ConnectionString));
                // break;
            case StorageTypes.POSTGRE:
                db.UseNpgsql(conf.ConnectionString);
                break;
            case StorageTypes.SQLSERVER:
                db.UseSqlServer(conf.ConnectionString);
                break;
            case StorageTypes.SQLITE:
                db.UseSqlite(conf.ConnectionString);
                break;
            case StorageTypes.INMEMORY:
                db.UseInMemoryDatabase(conf.ConnectionString);
                break;
        }
    }

    private static void ConfigureOpeniddictMigration(WebApplicationBuilder builder, DbContextOptionsBuilder db)
    {
        var section = builder.Configuration.GetSection(nameof(OpeniddictMigrationOptions));
        var conf = section.Get<OpeniddictMigrationOptions>() ?? throw new InvalidOperationException($"Configuration section '{nameof(OpeniddictMigrationOptions)}' is missing");
        switch (conf.Transport)
        {
            case StorageTypes.MYSQL:
                throw new NotSupportedException("MySQL storage is not supported. Use PostgreSQL instead.");
                // db.UseMySql(conf.ConnectionString, ServerVersion.AutoDetect(conf.ConnectionString));
                // break;
            case StorageTypes.POSTGRE:
                db.UseNpgsql(conf.ConnectionString);
                break;
            case StorageTypes.SQLSERVER:
                db.UseSqlServer(conf.ConnectionString);
                break;
            case StorageTypes.SQLITE:
                db.UseSqlite(conf.ConnectionString);
                break;
            case StorageTypes.INMEMORY:
                db.UseInMemoryDatabase(conf.ConnectionString);
                break;
        }
    }

    private static void ConfigureMetrics(MeterProviderBuilder builder, OpenTelemetryOptions options)
    {
        if(options.EnableConsoleExporter)
        {
            builder.AddConsoleExporter();
        }

        if(options.EnableOtpExported)
        {
            builder.AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri(options.MetricsEndpoint);
                o.Headers = options.Headers;
                o.Protocol = options.Protocol;
            });
        }
    }

    private static void ConfigureTraces(TracerProviderBuilder builder, OpenTelemetryOptions options)
    {
        if(options.EnableEfCoreTracing)
        {
            builder.AddEntityFrameworkCoreInstrumentation(o =>
            {
                o.EnrichWithIDbCommand = (activity, command) =>
                {
                    var stateDisplayName = $"{command.CommandType} main";
                    activity.DisplayName = stateDisplayName;
                    activity.SetTag("db.name", stateDisplayName);
                    activity.SetTag("db.text", command.CommandText);
                };
            });
        }

        if (options.EnableConsoleExporter)
        {
            builder.AddConsoleExporter();
        }

        if (options.EnableOtpExported)
        {
            builder.AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri(options.TracesEndpoint);
                o.Headers = options.Headers;
                o.Protocol = options.Protocol;
            });
        }
    }

    private static void ConfigureHangfire(WebApplicationBuilder webApplicationBuilder, IGlobalConfiguration configuration)
    {
        var section = webApplicationBuilder.Configuration.GetSection(nameof(StorageConfiguration));
        var conf = section.Get<StorageConfiguration>() ?? throw new InvalidOperationException($"Configuration section '{nameof(StorageConfiguration)}' is missing");
        switch (conf.Type)
        {
            case StorageTypes.INMEMORY:
                configuration.UseInMemoryStorage();
                break;
            case StorageTypes.SQLSERVER:
                configuration.UseSqlServerStorage(conf.ConnectionString);
                break;
            case StorageTypes.POSTGRE:
                configuration.UsePostgreSqlStorage(o => o.UseNpgsqlConnection(conf.ConnectionString));
                break;
            case StorageTypes.MYSQL:
                throw new NotSupportedException("MySQL Hangfire storage is not supported. Use PostgreSQL instead.");
                // configuration.UseStorage(new MySqlStorage(conf.ConnectionString, new MySqlStorageOptions
                // {
                //     TransactionIsolationLevel = IsolationLevel.ReadCommitted,
                //     QueuePollInterval = TimeSpan.FromSeconds(15),
                //     JobExpirationCheckInterval = TimeSpan.FromHours(1),
                //     CountersAggregateInterval = TimeSpan.FromMinutes(5),
                //     PrepareSchemaIfNecessary = true,
                //     DashboardJobListLimit = 50000,
                //     TransactionTimeout = TimeSpan.FromMinutes(1),
                //     TablesPrefix = "Hangfire"
                // }));
                // break;
            case StorageTypes.SQLITE:
                configuration.UseSQLiteStorage(conf.ConnectionString);
                break;
        }
    }

    private static void ConfigureKeyValue(WebApplicationBuilder webApplicationBuilder, AutomaticConfigurationOptions options)
    {
        var section = webApplicationBuilder.Configuration.GetSection(nameof(KeyValueConfiguration));
        var conf = section.Get<KeyValueConfiguration>() ?? throw new InvalidOperationException($"Configuration section '{nameof(KeyValueConfiguration)}' is missing");
        switch(conf.Type)
        {
            case KeyValueTypes.REDIS:
                options.UseRedisConnector(conf.ConnectionString);
                break;
        }
    }

    private static void ConfigureIdserverStorage(WebApplicationBuilder builder, DbContextOptionsBuilder b)
    {
        var section = builder.Configuration.GetSection(nameof(StorageConfiguration));
        var conf = section.Get<StorageConfiguration>() ?? throw new InvalidOperationException($"Configuration section '{nameof(StorageConfiguration)}' is missing");
        ConfigurationHelper.BuildDbContextOptions(b, conf, ConfigurationHelper.GetSisMigrationsAssembly(conf.Type));
        if (conf.Type == StorageTypes.SQLITE)
        {
            b.ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.AmbientTransactionWarning));
        }
    }

    private static void ConfigureFormbuilderStorage(WebApplicationBuilder builder, DbContextOptionsBuilder b)
    {
        var section = builder.Configuration.GetSection(nameof(StorageConfiguration));
        var conf = section.Get<StorageConfiguration>() ?? throw new InvalidOperationException($"Configuration section '{nameof(StorageConfiguration)}' is missing");
        ConfigurationHelper.BuildDbContextOptions(b, conf, ConfigurationHelper.GetFormBuilderMigrationsAssembly(conf.Type));
        if (conf.Type == StorageTypes.SQLITE)
        {
            b.ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.AmbientTransactionWarning));
        }
    }

    private static void ConfigureDistributedCache(WebApplicationBuilder builder)
    {
        var section = builder.Configuration.GetSection(nameof(DistributedCacheConfiguration));
        var conf = section.Get<DistributedCacheConfiguration>() ?? throw new InvalidOperationException($"Configuration section '{nameof(DistributedCacheConfiguration)}' is missing");
        switch (conf.Type)
        {
            case DistributedCacheTypes.SQLSERVER:
                builder.Services.AddDistributedSqlServerCache(opts =>
                {
                    opts.ConnectionString = conf.ConnectionString;
                    opts.SchemaName = "dbo";
                    opts.TableName = "DistributedCache";
                });
                break;
            case DistributedCacheTypes.REDIS:
                builder.Services.AddStackExchangeRedisCache(opts =>
                {
                    opts.Configuration = conf.ConnectionString;
                    opts.InstanceName = conf.InstanceName;
                });
                break;
            case DistributedCacheTypes.POSTGRE:
                builder.Services.AddDistributedPostgreSqlCache(opts =>
                {
                    opts.ConnectionString = conf.ConnectionString;
                    opts.SchemaName = "public";
                    opts.TableName = "DistributedCache";
                });
                break;
            case DistributedCacheTypes.MYSQL:
                throw new NotSupportedException("MySQL distributed cache is not supported. Use PostgreSQL instead.");
                // builder.Services.AddDistributedMySqlCache(opts =>
                // {
                //     opts.ConnectionString = conf.ConnectionString;
                //     opts.SchemaName = "idserver";
                //     opts.TableName = "DistributedCache";
                // });
                // break;
            case DistributedCacheTypes.SQLITE:
                builder.Services.AddSqliteCache(options =>
                {
                    options.CachePath = conf.ConnectionString!;
                });
                break;
        }
    }

    private static void ConfigureMessageBroker(WebApplicationBuilder builder, IBusRegistrationConfigurator configurator)
    {
        var section = builder.Configuration.GetSection(nameof(MessageBrokerOptions));
        var conf = section.Get<MessageBrokerOptions>() ?? throw new InvalidOperationException($"Configuration section '{nameof(MessageBrokerOptions)}' is missing");
        switch (conf.Transport)
        {
            case TransportTypes.SQLSERVER:
                configurator.UsingSqlServer((ctx, cfg) =>
                {
                    cfg.UsePublishMessageScheduler();
                    cfg.ConfigureEndpoints(ctx);
                });
                break;
            default:
                configurator.UsingInMemory((ctx, cfg) =>
                {
                    cfg.UsePublishMessageScheduler();
                    cfg.ConfigureEndpoints(ctx);
                });
                break;
        }
    }

    private static void ConfigureV2ReCaptcha(WebApplicationBuilder builder, V2ReCaptchaOptions options)
    {
        var section = builder.Configuration.GetSection(nameof(V2ReCaptchaOptions));
        var conf = section.Get<V2ReCaptchaOptions>() ?? throw new InvalidOperationException($"Configuration section '{nameof(V2ReCaptchaOptions)}' is missing");
        options.Secret = conf.Secret;
    }

    private static void ConfigureMessageBrokerMigration(WebApplicationBuilder builder)
    {
        var section = builder.Configuration.GetSection(nameof(MessageBrokerOptions));
        var conf = section.Get<MessageBrokerOptions>() ?? throw new InvalidOperationException($"Configuration section '{nameof(MessageBrokerOptions)}' is missing");
        switch (conf.Transport)
        {
            case TransportTypes.SQLSERVER:
                builder.Services.AddSqlServerMigrationHostedService(create: true, delete: false);
                builder.Services.AddOptions<SqlTransportOptions>()
                    .Configure(options =>
                    {
                        options.ConnectionString = conf.ConnectionString;
                        options.Database = conf.Database;
                    });
                break;
        }
    }

    private static void ConfigureDataseeder(WebApplicationBuilder builder, IdentityServerConfiguration config)
    {
        // BeforeDeployment — 在迁移后立即执行
        builder.Services.AddTransient<IDataSeeder, DropDistributedCacheDataSeeder>();
        builder.Services.AddTransient<IDataSeeder, ClientTypeDataSeeder>();

        // AfterDeployment — 在种子数据后执行
        builder.Services.AddTransient<IDataSeeder, ConfigureAuthSchemeProviderDataSeeder>();
        builder.Services.AddTransient<IDataSeeder, ConfigureCredentialIssuerDataSeeder>();
        builder.Services.AddTransient<IDataSeeder, ConfigureFastfedDataSeeder>();
        builder.Services.AddTransient<IDataSeeder, ConfigureGotifyDataseeder>();
        builder.Services.AddTransient<IDataSeeder, ConfigureFederationDataseeder>();
        builder.Services.AddTransient<IDataSeeder, ConfigureAdminWebsiteRedirectUrlsDataSeeder>();
        builder.Services.AddTransient<IDataSeeder, ConfigureSwaggerClientRedirectUrlsDataSeeder>();

        // 自定义 — 额外扩展
        builder.Services.AddTransient<IDataSeeder, ConfigureAdminUserDataSeeder>();
    }
}
