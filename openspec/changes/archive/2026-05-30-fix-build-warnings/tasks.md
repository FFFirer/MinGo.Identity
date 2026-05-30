## 1. Fix NuGet Package Warnings (Directory.Packages.props)

- [x] 1.1 Update Pomelo.EntityFrameworkCore.MySql from 8.0.2 to 9.0.0
- [x] 1.2 Update OpenTelemetry.Api from 1.12.0 to 1.15.3
- [x] 1.3 Update OpenTelemetry.Exporter.OpenTelemetryProtocol from 1.12.0 to 1.15.3
- [x] 1.4 Update OpenTelemetry.Exporter.Console from 1.11.0 to 1.15.3
- [x] 1.5 Update OpenTelemetry.Exporter.Prometheus.AspNetCore from 1.11.0-beta.1 to 1.15.3
- [x] 1.6 Update OpenTelemetry.Instrumentation.EntityFrameworkCore from 1.11.0-beta.2 to 1.15.3
- [x] 1.7 Add Microsoft.Bcl.Memory 10.0.4 to CPM (pin transitive fix)
- [x] 1.8 Add System.Security.Cryptography.Xml 10.0.6 to CPM (pin transitive fix)

## 2. Fix CS8618 — Non-nullable Configuration Class Properties

- [x] 2.1 Fix IdentityServerAdminConfiguration.cs (Admin): add `= null!;` to 7 string/reference properties
- [x] 2.2 Fix OpenTelemetryOptions.cs (IdServer): add `= null!;` to 3 string properties
- [x] 2.3 Fix OpenTelemetryOptions.cs (Admin — in IdentityServerAdminConfiguration.cs): add `= null!;` to 2 string properties
- [x] 2.4 Fix MessageBrokerOptions.cs: add `= null!;` to 4 string properties
- [x] 2.5 Fix IdentityServerConfiguration.cs: add `= null!;` to 4 string properties
- [x] 2.6 Fix OpenIdConnectLiteOptions.cs: add `= null!;` to 2 string properties
- [x] 2.7 Fix GoogleOptionsLite.cs: add `= null!;` to 2 string properties
- [x] 2.8 Fix FacebookOptionsLite.cs: add `= null!;` to 2 string properties
- [x] 2.9 Fix DuendeMigrationOptions.cs, OpeniddictMigrationOptions.cs, KeyValueConfiguration.cs: add `= null!;` to each ConnectionString
- [x] 2.10 Fix ClientTypeDataSeeder.cs: add `= null!;` to Id property
- [x] 2.11 Verify all CS8618 warnings are resolved

## 3. Fix CS8602/CS8604 — Null Dereference in SidServerSetup.cs

- [x] 3.1 Fix ConfigureDuendeMigration: null-guard `conf` after `.Get<DuendeMigrationOptions>()`
- [x] 3.2 Fix ConfigureOpeniddictMigration: null-guard `conf` after `.Get<OpeniddictMigrationOptions>()`
- [x] 3.3 Fix ConfigureHangfire: null-guard `conf` after `.Get<StorageConfiguration>()`
- [x] 3.4 Fix ConfigureKeyValue: null-guard `conf` after `.Get<KeyValueConfiguration>()`
- [x] 3.5 Fix ConfigureIdserverStorage: null-guard `conf` after `.Get<StorageConfiguration>()`
- [x] 3.6 Fix ConfigureFormbuilderStorage: null-guard `conf` after `.Get<StorageConfiguration>()`
- [x] 3.7 Fix ConfigureDistributedCache: null-guard `conf` after `.Get<DistributedCacheConfiguration>()`
- [x] 3.8 Fix ConfigureV2ReCaptcha: null-guard `conf` after `.Get<V2ReCaptchaOptions>()`
- [x] 3.9 Fix ConfigureMessageBroker: null-guard `conf` after `.Get<MessageBrokerOptions>()`
- [x] 3.10 Fix ConfigureMessageBrokerMigration: null-guard `conf` after `.Get<MessageBrokerOptions>()`
- [x] 3.11 Fix ConfigureMetrics/ConfigureTraces callers: null-guard `openTelemetryOptions` passed to these methods
- [x] 3.12 Verify all CS8602/CS8604 warnings in SidServerSetup.cs are resolved

## 4. Fix CS0618 — Obsolete API

- [x] 4.1 Replace `UsePostgreSqlStorage(conf.ConnectionString)` with `UsePostgreSqlStorage(o => o.ConnectionString = conf.ConnectionString)`
- [x] 4.2 Verify CS0618 warning is resolved

## 5. Fix CS8981 — Lowercase Alias

- [x] 5.1 Rename `using cl = System.Console;` to `using Console = System.Console;` in IdServerEventsConsumer.cs
- [x] 5.2 Verify CS8981 warning is resolved

## 6. Final Verification

- [x] 6.1 Run `dotnet build` and verify all warnings are eliminated
- [x] 6.2 Run `lsp_diagnostics` on all changed files
