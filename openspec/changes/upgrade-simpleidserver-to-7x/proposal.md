## Why

SimpleIdServer 6.0.4 targets .NET 8 and its packages are hosted on MyGet (not NuGet.org), causing build failures (NU1101) in our .NET 10 project. SimpleIdServer 7.0.0 was released March 2026 targeting .NET 10, with all packages published to NuGet.org. Upgrading unblocks our build, aligns framework versions, and provides access to new features (OpenTelemetry, Swagger, FormBuilder, Blazor-based Admin UI).

## What Changes

- **BREAKING**: Replace `AddPostgreSQL(connectionString)` with `UseEfStore()` dual DbContext pattern (IdServer DB + FormBuilder DB)
- **BREAKING**: Replace `AddEmailNotification(true)` with `AddEmailAuthentication()`
- **BREAKING**: Admin project migrates from MVC to Blazor Interactive Server; replace `AddSidAdmin()` with `AddIdserverAdmin()`
- **BREAKING**: Seed data pattern changes from `app.Services.SeedData(lambda)` to builder `.SeedAdministrationData(...)` + parameterless `app.Services.SeedData()`
- Replace package `SimpleIdServer.IdServer.PostgreSQLMigrations` with `SimpleIdServer.IdServer.PostgreMigrations`
- Replace package `SimpleIdServer.IdServer.Admin` with `SimpleIdServer.IdServer.Website`
- Add package `SimpleIdServer.IdServer.Store.EF`
- Remove MyGet feed dependency (all 7.x packages on NuGet.org)
- Add `.EnableMigration()` for automatic EF Core migration
- Add optional `.EnableOpenTelemetry()` for observability
- Update OIDC client configuration in Admin to use `UpdateOpenid()`

## Capabilities

### New Capabilities
- `idserver-core`: SimpleIdServer identity server with OIDC/OAuth2, password auth, email auth, PostgreSQL storage
- `admin-portal`: Blazor-based admin management UI for IdServer

### Modified Capabilities
<!-- No existing specs to modify - this is the initial spec setup -->

## Impact

- `src/MinGo.IdServer/` — Program.cs rewritten, csproj updated
- `src/MinGo.IdServer.Admin/` — Program.cs rewritten, csproj updated, architecture migration from MVC to Blazor
- `Directory.Packages.props` — all SimpleIdServer package versions bumped
- `nuget.config` — may no longer need MyGet source
- Database — applying 7.0.0 EF migrations may require schema updates
