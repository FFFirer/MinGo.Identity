## Why

The project currently emits **112 compile-time warnings** (44 NuGet + ~68 C# compiler), including 28 known security vulnerabilities (2 high-severity) and version constraint conflicts that block clean builds. These warnings hide real issues, create noise in CI/CD pipelines, and represent actual security risks in production dependencies. Resolving them improves code quality, eliminates security advisories, and establishes a clean baseline for future development.

## What Changes

- **Pomelo.EntityFrameworkCore.MySql**: Upgrade from 8.0.2 to 9.0.0 to resolve NU1608 version constraint conflict with EFCore 9.x
- **Vulnerability package upgrades**: Bump OpenTelemetry packages (1.12.0 → 1.15.3), Microsoft.Bcl.Memory (9.0.0 → 10.0.4), System.Security.Cryptography.Xml (8.0.2 → 10.0.6) to latest stable versions
- **OpenTelemetry ancillary packages**: Upgrade Exporter.Console, Exporter.Prometheus.AspNetCore, Instrumentation.EntityFrameworkCore from beta/older to 1.15.3 stable
- **CS8618 — Non-nullable property initialization**: Add `= null!;` initializers to ~30 configuration class string/reference properties
- **CS8602/CS8604 — Null dereference in SidServerSetup.cs**: Add null-guard patterns (`?? throw`) after all `IConfiguration.Get<T>()` calls
- **CS8600/CS8601/CS8602/CS8604 — Razor View nullability**: Fix ~26 null reference warnings across all Area Views
- **CS0618 — Obsolete API**: Replace `UsePostgreSqlStorage(string)` with `UsePostgreSqlStorage(Action<PostgreSqlBootstrapperOptions>)` overload
- **CS8981 — Lowercase alias**: Rename `using cl = System.Console` to `using Console = System.Console`

## Capabilities

### New Capabilities
- `package-dependency-management`: Centralized NuGet package version governance, vulnerability patching strategy, and EFCore/Pomelo compatibility
- `nullable-reference-types`: Code-wide pattern for handling non-nullable configuration properties and null-safe IConfiguration binding
- `null-safe-configuration-pattern`: Defensive coding pattern for `IConfiguration.Get<T>()` calls to eliminate null reference warnings

### Modified Capabilities
<!-- No existing specs are affected — this is purely an implementation-quality/infrastructure change -->

## Impact

- **Dependencies**: 12+ package version updates in `Directory.Packages.props`
- **Source files**: ~30 configuration classes, 1 infrastructure setup file (SidServerSetup.cs), 13+ Razor Views, 1 consumer file
- **No breaking changes**: All changes are internal code quality improvements — no API surface changes, no behavioral changes at runtime
- **Risk**: Low — each change is mechanical and clearly scoped
