## ADDED Requirements

### Requirement: CI pipeline defined
The system SHALL provide a Gitea Workflows CI pipeline at `.gitea/workflows/build.yml`.

#### Scenario: Workflow file exists
- **WHEN** checking repository contents
- **THEN** `.gitea/workflows/build.yml` SHALL exist

### Requirement: Trigger on push and tags
The workflow SHALL trigger on:
- Push to `main` or `master` branches
- Tags matching `v*` (e.g., `v1.0.0`, `v2.3.4-rc1`)
- Pull requests to `main` or `master`

#### Scenario: Push to main triggers workflow
- **WHEN** a push is made to the `main` branch
- **THEN** the workflow SHALL execute

#### Scenario: Version tag triggers workflow
- **WHEN** a tag matching `v*` is pushed
- **THEN** the workflow SHALL execute

### Requirement: Build and test stage
The workflow SHALL have a `build` job that restores NuGet packages, builds all projects in Release configuration, and runs all tests.

#### Scenario: Build succeeds
- **WHEN** `dotnet build -c Release` is executed
- **THEN** it SHALL exit with code 0

#### Scenario: Tests pass
- **WHEN** `dotnet test -c Release --no-build` is executed
- **THEN** it SHALL exit with code 0

### Requirement: NuGet cache
The workflow SHALL cache the NuGet packages directory (`~/.nuget/packages`) to speed up subsequent runs, keyed by `Directory.Packages.props` hash.

#### Scenario: Cache hit on second run
- **WHEN** `Directory.Packages.props` has not changed
- **THEN** NuGet packages SHALL be restored from cache

### Requirement: Docker image build and push
The workflow SHALL build and push Docker images for IdServer, Admin, and Migrator when triggered by a push to main/master or a version tag. The images SHALL be pushed to the Gitea Container Registry.

#### Scenario: Docker build on main push
- **WHEN** a push to `main` occurs
- **THEN** Docker images for all 3 services SHALL be built and pushed with tags `main` and commit SHA

#### Scenario: Docker build on tag push
- **WHEN** a tag `v1.0.0` is pushed
- **THEN** Docker images SHALL be built and pushed with tags `v1.0.0`, `latest`, and commit SHA

### Requirement: Parallel image builds
The Docker image builds for the 3 services SHALL run in parallel using a job matrix strategy.

#### Scenario: Matrix strategy employed
- **WHEN** examining the workflow YAML
- **THEN** `jobs.docker.strategy.matrix.service` SHALL contain `[idserver, admin, migrator]`
