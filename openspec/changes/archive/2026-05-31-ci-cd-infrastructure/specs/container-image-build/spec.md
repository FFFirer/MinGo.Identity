## ADDED Requirements

### Requirement: Dockerfile defined for each deployable service
The system SHALL provide a multi-stage Dockerfile for each deployable service under `docker/Dockerfile.<service>`.

#### Scenario: IdServer Dockerfile exists
- **WHEN** listing files in `docker/`
- **THEN** `Dockerfile.idserver` SHALL exist

#### Scenario: Admin Dockerfile exists
- **WHEN** listing files in `docker/`
- **THEN** `Dockerfile.admin` SHALL exist

#### Scenario: Migrator Dockerfile exists
- **WHEN** listing files in `docker/`
- **THEN** `Dockerfile.migrator` SHALL exist

### Requirement: Multi-stage build pattern
Each Dockerfile SHALL use a multi-stage build pattern with at least two stages: `build` (using `mcr.microsoft.com/dotnet/sdk:10.0`) and `runtime` (using `mcr.microsoft.com/dotnet/aspnet:10.0-alpine`).

#### Scenario: Build stage compiles project
- **WHEN** Docker builds the `build` stage
- **THEN** `dotnet restore` and `dotnet publish -c Release -o /app/publish` SHALL succeed

#### Scenario: Runtime stage copies published output
- **WHEN** Docker builds the `runtime` stage
- **THEN** the published artifacts from the `build` stage SHALL be copied to `/app/`

### Requirement: Non-root user for runtime containers
Runtime containers for IdServer and Admin SHALL run as a non-root user (`APP_UID=1000`) with minimal permissions.

#### Scenario: Container runs as non-root
- **WHEN** the container starts
- **THEN** the process SHALL run as user `1000`

### Requirement: HEALTHCHECK for web services
IdServer and Admin Dockerfiles SHALL include a `HEALTHCHECK` instruction that verifies the service is responding.

#### Scenario: HEALTHCHECK configured
- **WHEN** inspecting the Dockerfile
- **THEN** a `HEALTHCHECK` instruction SHALL be present

### Requirement: .dockerignore exists
The root of the repository SHALL contain a `.dockerignore` file that excludes unnecessary files from the Docker build context.

#### Scenario: .dockerignore excludes development artifacts
- **WHEN** Docker build context is processed
- **THEN** `bin/`, `obj/`, `.git/`, `node_modules/`, `.opencode/`, `openspec/changes/archive/`, and `docs/` SHALL be excluded
