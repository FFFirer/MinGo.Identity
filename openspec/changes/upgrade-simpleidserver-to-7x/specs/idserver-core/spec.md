## ADDED Requirements

### Requirement: SimpleIdServer identity server on .NET 10
The system SHALL run SimpleIdServer 7.0.0 as an OIDC/OAuth2 identity provider on .NET 10 with PostgreSQL storage, targeting `net10.0`.

#### Scenario: Build succeeds after upgrade
- **WHEN** `dotnet build` is executed on the solution
- **THEN** all NuGet packages resolve (no NU1101 errors) and compilation passes

#### Scenario: OIDC Discovery endpoint responds
- **WHEN** GET request is sent to `/.well-known/openid-configuration`
- **THEN** the endpoint returns a valid OpenID Connect discovery document

### Requirement: PostgreSQL storage with EF Core
The system SHALL use PostgreSQL 16 as its database, accessed via Entity Framework Core with Npgsql. The `UseEfStore()` dual-DbContext pattern SHALL be used to configure both the IdServer store and FormBuilder store.

#### Scenario: Database connection configured
- **WHEN** IdServer starts and .EnableMigration() runs
- **THEN** EF Core applies pending migrations to the PostgreSQL database specified in `ConnectionStrings:IdServer`

#### Scenario: FormBuilder schema created
- **WHEN** IdServer starts and migrations run
- **THEN** FormBuilder-related tables are created alongside IdServer tables

### Requirement: Password authentication
The system SHALL support password-based authentication with a default `administrator` account.

#### Scenario: Administrator login succeeds
- **WHEN** POST `/connect/token` is called with `grant_type=password`, username `administrator`, and password `password`
- **THEN** the endpoint returns a valid access token

#### Scenario: Seed data creates admin user
- **WHEN** the application starts with an empty database
- **THEN** the `administrator` user is created via a custom `IDataSeeder` implementation

### Requirement: Email notification support
The system SHALL support email-based authentication notifications as an option.

#### Scenario: Email authentication configured
- **WHEN** `AddEmailAuthentication()` is called in the builder
- **THEN** email authentication flows are available in the OIDC provider

### Requirement: Port binding
The IdServer SHALL listen on `https://localhost:5001`.

#### Scenario: Port is bound
- **WHEN** the application starts
- **THEN** it listens on `https://localhost:5001`
