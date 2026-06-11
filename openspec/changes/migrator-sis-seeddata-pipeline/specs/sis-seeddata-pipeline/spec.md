## ADDED Requirements

### Requirement: Migrator executes SimpleIdServer official seeders
MinGo.Identity.Migrator SHALL invoke SimpleIdServer's built-in `SeedData()` extension method to execute all registered `IDataSeeder` and `IDbMigrateService` implementations.

#### Scenario: SeedData executes all 13 official IDataSeeder on first run
- **WHEN** `MigratorService.SeedDataAsync()` is called on a fresh database
- **THEN** all 13 `IDataSeeder` implementations from `ConfigureDataSeeders` execute in order
- **AND** `InitLanguageDataSeeder` inserts Language records (`en`, `fr`) into the `Languages` table
- **AND** `InitLanguageDataSeeder` inserts Translation records into the `Translations` table
- **AND** `DataSeederExecutionHistory` table records each seeder's execution

#### Scenario: SeedData is idempotent on subsequent runs
- **WHEN** `MigratorService.SeedDataAsync()` is called again
- **THEN** seeders already recorded in `DataSeederExecutionHistory` are skipped
- **AND** no duplicate Language or Translation records are inserted

### Requirement: Migrator executes database migrations via IDbMigrateService
MinGo.Identity.Migrator SHALL register `IDbMigrateService` implementations for all DbContexts, so `SeedData().Migrate()` applies pending migrations.

#### Scenario: SIS migrations applied
- **WHEN** `SeedData()` triggers `Migrate()`
- **THEN** `IdServerDbMigrateService.Migrate()` calls `StoreDbContext.Database.MigrateAsync()`
- **AND** all pending SIS migrations are applied

#### Scenario: FormBuilder migrations applied
- **WHEN** `SeedData()` triggers `Migrate()`
- **THEN** `FormbuilderDbMigrateService.Migrate()` calls FormBuilder DbContext's `Database.MigrateAsync()`
- **AND** all pending FormBuilder migrations are applied

### Requirement: Migrator registers all 13 official IDataSeeder
MigratorService SHALL register the same 13 `IDataSeeder` types that SimpleIdServer's `ConfigureDataSeeders()` registers, plus `AddMigrationsScopeDataSeeder` from `EnableMigration()`.

#### Scenario: All seeders registered
- **WHEN** `SeedDataAsync()` builds the `ServiceCollection`
- **THEN** the following `IDataSeeder` types are registered via `AddTransient`:
  - `InitRealmDataSeeder`
  - `InitSerializedFileKeyDataSeeder`
  - `InitCertificateAuthoritiesDataseeder`
  - `InitLanguageDataSeeder`
  - `InitScopeDataSeeder`
  - `InitConsoleAuthDataseeder`
  - `InitUserLockingConfigurationDefDataseeder`
  - `InitConsoleConfigurationDefDataseeder`
  - `UpdateAuthorizationCodeLifetimeDataSeeder`
  - `UpdateDeviceCodeLitetimeDataSeeder`
  - `UpdateParLitetimeDataSeeder`
  - `UpdateOtherLifetimeDataSeeder`
  - `UpdateClientSecretDataSeeder`
  - `UpdateTargetsConsoleWorkflowsDataseeder`
  - `AddMigrationsScopeDataSeeder`

### Requirement: Migrator references SimpleIdServer.IdServer NuGet package
MinGo.Identity.Migrator.csproj SHALL add a `PackageReference` to `SimpleIdServer.IdServer` to access `IDataSeeder`, `IDbMigrateService`, `SeedData()` extension, and all seeder types.

#### Scenario: Build succeeds with new dependency
- **WHEN** `dotnet build` is run on the Migrator project
- **THEN** the build succeeds with the new `SimpleIdServer.IdServer` reference
