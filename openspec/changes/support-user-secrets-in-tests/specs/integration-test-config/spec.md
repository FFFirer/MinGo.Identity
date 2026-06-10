## ADDED Requirements

### Requirement: Integration test configuration

The integration test project SHALL support a three-tier configuration override chain for database connection:
1. Environment variable `MINGO_TEST_CONNECTION_STRING` (highest priority)
2. .NET UserSecrets key `StorageConfiguration:ConnectionString`
3. `appsettings.json` → `StorageConfiguration:ConnectionString` (default)

The parsed configuration SHALL be resolved in `DbFixture` and exposed as `StorageConfiguration` for reuse by `MigratorService`.

#### Scenario: Default config from appsettings.json

- **WHEN** no env var and no UserSecrets are set
- **THEN** the connection string in `appsettings.json` is used

#### Scenario: Override via UserSecrets

- **WHEN** `dotnet user-secrets set "StorageConfiguration:ConnectionString" "Host=..."` has been run
- **THEN** the UserSecrets value overrides appsettings.json

#### Scenario: Override via environment variable

- **WHEN** `$env:MINGO_TEST_CONNECTION_STRING = "Host=..."` is set
- **THEN** the env var value overrides both appsettings.json and UserSecrets

#### Scenario: Single config source for both DB lifecycle and migration

- **WHEN** DbFixture parses the configuration
- **THEN** `CreateMigratorService()` SHALL reuse the same `StorageConfiguration` from DbFixture
- **AND** not re-read appsettings.json independently
