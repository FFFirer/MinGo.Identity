## ADDED Requirements

### Requirement: IConfiguration.Get<T>() results must be null-guarded
Every call to `IConfiguration.GetSection(nameof(T)).Get<T>()` SHALL be followed by a null-guard that throws an `InvalidOperationException` with a descriptive message if the result is null. This applies when the configuration section is required for correct operation.

#### Scenario: Missing configuration throws at startup
- **WHEN** a required configuration section is missing from `appsettings.json`
- **THEN** the application SHALL throw `InvalidOperationException` at startup with a message indicating which configuration section is missing

### Requirement: Obsolete API replacements must adopt the recommended overload
When an API is marked `[Obsolete]` with a documented replacement, the code SHALL use the recommended replacement overload instead.

#### Scenario: UsePostgreSqlStorage uses new overload
- **WHEN** `UsePostgreSqlStorage` is called in `SidServerSetup.cs`
- **THEN** it SHALL use the `Action<PostgreSqlBootstrapperOptions>` overload, not the deprecated `string` overload

### Requirement: Lowercase-only type aliases must be avoided
Type aliases consisting entirely of lowercase ASCII characters SHALL NOT be used, as they may become reserved for the language in future C# versions.

#### Scenario: Console alias uses PascalCase
- **WHEN** `System.Console` is aliased
- **THEN** the alias SHALL use PascalCase (e.g., `Console` not `cl`)
