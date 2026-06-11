## ADDED Requirements

### Requirement: ExtStoreDbContext migration runs via IDbMigrateService
The Migrator SHALL provide an `ExtStoreDbMigrateService` implementing `IDbMigrateService` that wraps `ExtStoreDbContext.Database.MigrateAsync()`, so the custom extension tables' migrations are executed as part of `SeedData().Migrate()`.

#### Scenario: ExtStoreDbMigrateService registered
- **WHEN** `SeedDataAsync()` builds the `ServiceCollection`
- **THEN** `ExtStoreDbMigrateService` is registered as `IDbMigrateService` via `AddTransient`

#### Scenario: Ext migrations applied via SeedData
- **WHEN** `SeedData()` triggers `Migrate()`
- **THEN** `ExtStoreDbMigrateService.Migrate()` calls `ExtStoreDbContext.Database.MigrateAsync()`
- **AND** all pending custom extension migrations are applied to the `user_profiles` table

### Requirement: Ext store has custom IDataSeeder for seed data
The Migrator SHALL provide an `InitUserProfileDataSeeder` implementing `IDataSeeder` via `BaseAfterDeploymentDataSeeder`, for seeding initial data into custom extension tables.

#### Scenario: InitUserProfileDataSeeder registered and executed
- **WHEN** `SeedDataAsync()` builds the `ServiceCollection`
- **THEN** `InitUserProfileDataSeeder` is registered as `IDataSeeder` via `AddTransient`
- **AND** `SeedData().MigrateAfterDeployment()` executes `InitUserProfileDataSeeder.Apply()`

#### Scenario: InitUserProfileDataSeeder is idempotent
- **WHEN** `InitUserProfileDataSeeder.Execute()` runs
- **THEN** it checks if `UserProfiles` already has records via `AnyAsync()`
- **AND** skips insertion if data already exists

### Requirement: ExtStoreDbContext is registered in DI for SeedData
`ExtStoreDbContext` SHALL be registered in the same `ServiceCollection` used by `SeedDataAsync()`, with the same storage configuration as the existing Step 2 migration.

#### Scenario: ExtStoreDbContext registered with correct config
- **WHEN** `SeedDataAsync()` configures the `ServiceCollection`
- **THEN** `services.AddDbContext<ExtStoreDbContext>(o => ...)` uses `ConfigurationHelper.BuildDbContextOptions()` with migration assembly `"MinGo.Identity.Migrations"`
