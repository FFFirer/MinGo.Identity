## Context

Project MinGo.Identity runs on .NET 10 with SimpleIdServer 6.0.4 for OIDC/OAuth2. Current state:
- IdServer (`:5001`) — uses `AddPostgreSQL()`, `AddPwdAuthentication()`, `AddEmailNotification()`, manual SeedData
- Admin (`:5002`) — uses `AddSidAdmin()` from `SimpleIdServer.IdServer.Admin` package with manual OIDC middleware config
- Build currently fails (NU1101) because 6.x packages are on MyGet, not NuGet.org
- SimpleIdServer 7.0.0 targets .NET 10 and is fully published to NuGet.org

Investigation with `dotnet new idserver` (7.0.0 template) revealed the new API patterns documented below.

## Goals / Non-Goals

**Goals:**
- Upgrade IdServer project to SimpleIdServer 7.0.0 with minimal functional change
- Upgrade Admin project to SimpleIdServer 7.0.0 using the new `SimpleIdServer.IdServer.Website` package
- Restore project build ability
- Maintain same ports, same seed data (administrator user), same authentication flows
- All packages sourced from NuGet.org only (remove MyGet dependency)

**Non-Goals:**
- Adding new features (FIDO2, SAML, CIBA, OpenTelemetry) — these are opt-in additions for future changes
- Changing database provider (staying on PostgreSQL)
- Changing UI customization beyond what the template provides
- Production deployment configuration

## Decisions

### Decision 1: Use `UseEfStore()` with dual DbContext over `AddPostgreSQL()`

**Choice**: Replace `AddPostgreSQL(connectionString)` with `UseEfStore()` that configures both the IdServer DbContext and FormBuilder DbContext.

**Rationale**: In 7.0.0, `AddPostgreSQL()` was removed. The new `UseEfStore()` requires two callbacks — one for the identity store and one for the new FormBuilder store. Both use the same PostgreSQL connection string and the same migrations assembly (`SimpleIdServer.IdServer.PostgreMigrations`).

```csharp
// 6.x (old)
builder.AddSidIdentityServer()
    .AddPostgreSQL(connectionString);

// 7.x (new)
builder.AddSidIdentityServer(c => { c.Authority = "https://localhost:5001"; }, true)
    .EnableMigration()
    .UseEfStore(
        idStore => idStore.UseNpgsql(connectionString, o =>
        {
            o.MigrationsAssembly("SimpleIdServer.IdServer.PostgreMigrations");
            o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        }),
        fbStore => fbStore.UseNpgsql(connectionString, o =>
        {
            o.MigrationsAssembly("SimpleIdServer.IdServer.PostgreMigrations");
            o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        })
    );
```

**Note**: The package `SimpleIdServer.IdServer.PostgreSQLMigrations` is renamed to `SimpleIdServer.IdServer.PostgreMigrations`. A new package `SimpleIdServer.IdServer.Store.EF` is also required.

### Decision 2: Admin migrates from MVC to Blazor Interactive Server

**Choice**: Replace MVC-based Admin with Blazor Interactive Server using `SimpleIdServer.IdServer.Website` package.

**Rationale**: The 7.0.0 Admin template (`idserveradmin`) is a Blazor Interactive Server application. The old `SimpleIdServer.IdServer.Admin` package no longer exists. The new `AddIdserverAdmin()` builder replaces `AddSidAdmin()`, and OIDC configuration is handled by `UpdateOpenid()` instead of manual cookie + OIDC middleware.

```csharp
// 6.x (old)
builder.Services.AddSidAdmin(options => { options.IdServerBaseUrl = baseUrl; });
// + manual AddCookie + AddOpenIdConnect

// 7.x (new)
var adminBuilder = builder.AddIdserverAdmin(baseUrl, o => { o.ScimUrl = "..."; });
adminBuilder.UpdateOpenid(clientId, clientSecret, scopes, ignoreCertError);
```

**Impact**: Admin project structure changes from MVC (Controllers/Views) to Blazor (Components/Pages). The Program.cs is significantly different. OIDC flow is managed internally.

### Decision 3: Seed data via IDataSeeder pattern

**Choice**: Replace inline lambda `app.Services.SeedData((sp, db) => { ... })` with the `IDataSeeder` service pattern.

**Rationale**: 7.0.0's `app.Services.SeedData()` (parameterless) discovers and runs all registered `IDataSeeder` implementations. Custom seed logic must be registered as `IDataSeeder` services in the DI container.

```csharp
// 6.x (old)
app.Services.SeedData((serviceProvider, dbContext) =>
{
    var userRepo = serviceProvider.GetRequiredService<IUserRepository>();
    // add admin user...
});

// 7.x (new) - Register in builder:
builder.Services.AddTransient<IDataSeeder, AdminUserDataSeeder>();
// Then in app:
app.Services.SeedData();
```

### Decision 4: Keep same port bindings and configuration structure

**Choice**: IdServer stays on `:5001`, Admin stays on `:5002`. Use `appsettings.json` for connection strings and URLs as before.

**Rationale**: No reason to change ports. The appsettings format for IdServer changes to use the `StorageConfiguration` section pattern from the template.

### Decision 5: Only include necessary packages, not the full template

**Choice**: Add only the packages we actually use (IdServer core, Pwd, Email, PostgreMigrations, Store.EF, Website), not all ~40 packages from the template.

**Rationale**: The template includes many optional packages (FIDO2, SAML, SMS, LDAP, Federation, etc.) that we don't need. Only pull what we use.

## Risks / Trade-offs

| Risk | Impact | Mitigation |
|------|--------|------------|
| **Database schema changes** between 6.x and 7.x | Migration may fail or data may be lost | Enable `.EnableMigration()` for auto-migration; test on a copy of production data first |
| **Admin UI behavior differs** in Blazor version | Team may need time to adapt to new UI | Keep old MVC Admin project as a backup; run side-by-side comparison |
| **FormBuilder migrations** are new — add another set of EF migrations | Extra migration step on startup | Both DbContexts use same PostgreSQL connection; FormBuilder schema is additive, not breaking |
| **Custom Areas/Views** in current project may not work with new template | Overridden views need updating | Our current Areas/Password/Pages were override templates from 6.x. Review if they still apply in 7.x |
| **Blazor Interactive Server** requires SignalR | Admin may have different perf characteristics | Keep on same network as IdServer; SignalR is standard ASP.NET Core |
