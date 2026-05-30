## Context

MinGo.Identity targets .NET 10.0 with `<Nullable>enable</Nullable>` and Central Package Management (CPM). The solution produces 112 warnings during build:

- **44 NuGet warnings**: 16× NU1608 (Pomelo/EFCore version mismatch), 28× NU1902/NU1903 (known security vulnerabilities)
- **~68 C# warnings**: Predominantly CS8618 (non-nullable properties uninitialized), CS8602 (null dereference), CS8604/CS8601 (null propagation) across configuration classes, infrastructure setup, and Razor Views

All warnings are in `MinGo.IdServer` (heaviest), `MinGo.IdServer.Admin`, `MinGo.Identity.Migrator`, and shared `MinGo.Identity.Configuration` projects.

## Goals / Non-Goals

**Goals:**
- Resolve all NU1608 version constraint conflicts (Pomelo 8.0.2 → 9.0.0 aligning with EFCore 9.0.4)
- Eliminate all NU1902/NU1903 security vulnerability warnings by upgrading to patched versions
- Fix all CS8618 warnings in configuration/options classes with proper null-safety patterns
- Fix all CS8602/CS8604 null dereference warnings in SidServerSetup.cs (the primary hot spot)
- Fix CS0618 (obsolete API usage) and CS8981 (lowercase alias) warnings
- Reduce total warning count to 0

**Non-Goals:**
- No behavioral changes at runtime — all changes are purely warning suppression
- No API surface changes or breaking changes
- No refactoring beyond what's needed to fix warnings
- Razor View null warnings (CS8600/CS8601/CS8602) are lower priority — they require ViewModel changes that may need SimpleIdServer source access

## Decisions

### Decision 1: Pomelo → 9.0.0, not 10.x
- **Choice**: Upgrade Pomelo.EntityFrameworkCore.MySql from 8.0.2 to 9.0.0 (latest stable)
- **Rationale**: Pomelo 10.0.0 is not yet released (still in test-fix phase per issue #2007). Pomelo 9.0.0 is fully compatible with EFCore 9.0.x and .NET 10 runtime. EFCore packages stay at 9.0.4.
- **Alternatives considered**: Microting community fork (10.0.5) — rejected due to non-official maintenance risk. Waiting for Pomelo 10.0.0 — would block all other changes.

### Decision 2: Vulnerability packages → latest patched versions
- **OpenTelemetry**: All OTel packages → 1.15.3 (fixes both GHSA-g94r and GHSA-4625)
- **Microsoft.Bcl.Memory**: 9.0.0 → 10.0.4 (fixes GHSA-73j8; net10.0 gets the 10.0.x patch)
- **System.Security.Cryptography.Xml**: 8.0.2 → 10.0.6 (fixes both GHSA-37gx and GHSA-w3x6; net10.0 gets the 10.0.x patch)
- **Rationale**: All three are transitive dependencies pulled in by SimpleIdServer / EFCore. Direct CPM entries ensure the fixed versions are resolved.

### Decision 3: CS8618 fix pattern → `= null!;` for configuration classes
- **Choice**: Add `= null!;` initializers to all non-nullable reference properties in `*Options` / `*Configuration` classes
- **Rationale**: These classes are populated via `IConfiguration.Get<T>()` / `.Bind()` which sets properties after construction. The `required` modifier doesn't help because the binder doesn't use it. `= string.Empty` would mask missing configuration. `= null!;` correctly expresses "the framework will set this at runtime."
- **Alternatives considered**: `required` keyword (doesn't work with `Get<T>`), `= string.Empty` (hides config errors), making properties nullable (shifts problem).

### Decision 4: CS8602 fix pattern → null-guard with `?? throw`
- **Choice**: After each `section.Get<T>()` call in SidServerSetup.cs, add:
  ```csharp
  var conf = section.Get<T>() ?? throw new InvalidOperationException(
      $"Configuration section '{nameof(T)}' is missing");
  ```
- **Rationale**: These configuration sections are required for correct operation. If they're missing, failing fast with a clear error message is the correct behavior.
- **Alternatives considered**: Null-forgiving operator `!` (hides real issues), null-check with early return (silent failure).

### Decision 5: CS0618 fix → Use new overload API
- **Choice**: Replace `UsePostgreSqlStorage(string)` with `UsePostgreSqlStorage(o => o.ConnectionString = conf.ConnectionString)`
- **Rationale**: The old API is marked for removal in Hangfire 2.0. Adopting the modern overload now avoids future breakage.

### Decision 6: CS8981 fix → Capitalize alias
- **Choice**: Change `using cl = System.Console;` to `using Console = System.Console;`
- **Rationale**: Lowercase-only aliases may become reserved in future C# versions. Using the original type name as the alias removes the warning trivially.

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| OpenTelemetry 1.15.3 may have API changes from 1.12.0 | Changelog reviewed — no breaking changes in public API between these versions. All changes are internal bug/vulnerability fixes. |
| Pomelo 9.0.0 may behave differently for MySQL queries | This is the official stable release, passing all existing tests. No functional change expected. |
| `= null!;` pattern requires discipline for new properties | Pattern documented in design; code review gate. |
| Some Razor View warnings may require SimpleIdServer source changes | These are de-prioritized. Runtime behavior is unaffected. |
| Microsoft.Bcl.Memory 10.0.4 may conflict with transitive deps from SimpleIdServer 7.0.0 (which targets net9.0) | CPM with `<CentralPackageVersionOverrideEnabled>true</CentralPackageVersionOverrideEnabled>` forces the version. Tested to resolve cleanly. |
