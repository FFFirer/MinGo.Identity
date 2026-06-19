## ADDED Requirements

### Requirement: ConfigurationHelper.GetSisMigrationsAssembly 按 storage type 返回正确程序集

The system SHALL return the correct migration assembly name for each storage type.

#### Scenario: SQLSERVER returns SqlServerMigrations
- **WHEN** `GetSisMigrationsAssembly(SQLSERVER)` is called
- **THEN** it returns `"SimpleIdServer.IdServer.SqlServerMigrations"`

#### Scenario: POSTGRE returns PostgreMigrations
- **WHEN** `GetSisMigrationsAssembly(POSTGRE)` is called
- **THEN** it returns `"SimpleIdServer.IdServer.PostgreMigrations"`

#### Scenario: MYSQL returns MySQLMigrations
- **WHEN** `GetSisMigrationsAssembly(MYSQL)` is called
- **THEN** it returns `"SimpleIdServer.IdServer.MySQLMigrations"`

#### Scenario: SQLITE returns SqliteMigrations
- **WHEN** `GetSisMigrationsAssembly(SQLITE)` is called
- **THEN** it returns `"SimpleIdServer.IdServer.SqliteMigrations"`

#### Scenario: INMEMORY returns null
- **WHEN** `GetSisMigrationsAssembly(INMEMORY)` is called
- **THEN** it returns `null`

#### Scenario: Invalid value throws ArgumentOutOfRangeException
- **WHEN** `GetSisMigrationsAssembly((StorageTypes)999)` is called
- **THEN** it throws `ArgumentOutOfRangeException`

### Requirement: ConfigurationHelper.GetFormBuilderMigrationsAssembly 按 storage type 返回正确程序集

The system SHALL return the correct FormBuilder migration assembly name for each storage type.

#### Scenario: SQLSERVER returns SqlServerMigrations
- **WHEN** `GetFormBuilderMigrationsAssembly(SQLSERVER)` is called
- **THEN** it returns `"SidFormBuilder.SqlServerMigrations"`

#### Scenario: POSTGRE returns PostgreMigrations
- **WHEN** `GetFormBuilderMigrationsAssembly(POSTGRE)` is called
- **THEN** it returns `"SidFormBuilder.PostgreMigrations"`

#### Scenario: MYSQL returns MySQLMigrations
- **WHEN** `GetFormBuilderMigrationsAssembly(MYSQL)` is called
- **THEN** it returns `"SidFormBuilder.MySQLMigrations"`

#### Scenario: SQLITE returns SqliteMigrations
- **WHEN** `GetFormBuilderMigrationsAssembly(SQLITE)` is called
- **THEN** it returns `"SidFormBuilder.SqliteMigrations"`

#### Scenario: INMEMORY returns null
- **WHEN** `GetFormBuilderMigrationsAssembly(INMEMORY)` is called
- **THEN** it returns `null`

#### Scenario: Invalid value throws ArgumentOutOfRangeException
- **WHEN** `GetFormBuilderMigrationsAssembly((StorageTypes)999)` is called
- **THEN** it throws `ArgumentOutOfRangeException`

### Requirement: ConfigurationHelper.BuildDbContextOptions 不因 null migrationsAssembly 抛出异常

The system SHALL tolerate null migrationsAssembly for INMEMORY storage type.

#### Scenario: INMEMORY with null assembly does not throw
- **WHEN** `BuildDbContextOptions(builder, config with INMEMORY, null)` is called
- **THEN** it configures in-memory database and returns builder without exception
