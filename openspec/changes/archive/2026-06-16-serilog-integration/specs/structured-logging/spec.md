## ADDED Requirements

### Requirement: Serilog replaces default ASP.NET Core logging pipeline
The system SHALL use Serilog as the logging backend for all ASP.NET Core services, replacing the default `ConsoleLoggerProvider`.

#### Scenario: UseSerilog registered before SimpleIdServer initialization
- **WHEN** the application starts
- **THEN** `UseSerilog()` MUST be called before `AddIdserverAdmin()` (Admin) or `SidServerSetup.ConfigureIdServer()` (IdServer)
- **AND** all `ILogger<T>` instances MUST be backed by Serilog

#### Scenario: ILogger<T> continues to work
- **WHEN** any service calls `ILogger<T>.Log()`
- **THEN** the log output MUST be processed by Serilog
- **AND** existing `ILogger<T>` injection code SHALL NOT require changes

### Requirement: Configuration-driven sink and format
The logging output format SHALL be controlled entirely by `appsettings.json` / `appsettings.{env}.json` configuration, with no runtime environment detection in code.

#### Scenario: Docker/production outputs structured JSON
- **WHEN** the application runs with no `appsettings.*.json` override for the Serilog WriteTo section
- **THEN** the Console sink SHALL use `RenderedCompactJsonFormatter`
- **AND** each log entry MUST be a single-line JSON object on stdout

#### Scenario: Development outputs readable text
- **WHEN** the application runs with `ASPNETCORE_ENVIRONMENT=Development`
- **THEN** the Console sink SHALL use the default Serilog text format (no JSON formatter)
- **AND** log entries MUST be human-readable plain text

### Requirement: Minimum level filtering per namespace
The system SHALL support per-namespace minimum log level overrides via `Serilog:MinimumLevel:Override` configuration.

#### Scenario: ASP.NET Framework noise suppressed
- **WHEN** the `Microsoft.AspNetCore` namespace logs at Debug or Information level
- **THEN** the Console sink SHALL NOT output these entries unless `Override` allows them
- **AND** the default minimum level SHALL be `Information`

### Requirement: Package management via CPM
All Serilog package versions SHALL be defined centrally in `Directory.Packages.props` and referenced without version in project files.

#### Scenario: Serilog packages resolved via CPM
- **WHEN** `dotnet restore` runs
- **THEN** Serilog packages MUST be resolved from the versions declared in `Directory.Packages.props`
- **AND** neither `Admin.csproj` nor `IdServer.csproj` SHALL declare explicit `<PackageVersion>` for Serilog

### Requirement: Migrator project unchanged
The `MinGo.Identity.Migrator` console application SHALL NOT be modified as part of this change. It SHALL continue using `Console.WriteLine`.

#### Scenario: Migrator output unchanged
- **WHEN** `dotnet run -- migrate` executes
- **THEN** log output SHALL remain identical to pre-Serilog behavior
