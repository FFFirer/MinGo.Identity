## ADDED Requirements

### Requirement: Docker build context exclusion
The `.dockerignore` file SHALL exclude all files and directories that are not required for Docker image builds, minimizing the build context sent to the Docker daemon.

#### Scenario: Branch build without local artifacts
- **WHEN** `docker build` is executed from the project root
- **THEN** the build context SHALL exclude `.git/`, `.gitea/`, `**/bin/`, `**/obj/`, `**/publish/`, `openspec/`, `.opencode/`, IDE directories, and OS metadata files
- **THEN** the build context SHALL include `Directory.Build.props`, `Directory.Packages.props`, `nuget.config`, `src/`, and `docker/`

#### Scenario: Unrelated large files excluded
- **WHEN** MSBuild binary logs (`*.binlog`) or NuGet packages (`*.nupkg`) exist in the project tree
- **THEN** they SHALL be excluded from the Docker build context

### Requirement: Maintainability
The `.dockerignore` file SHALL be organized into labeled sections with clear comments, making it easy to understand and modify.

#### Scenario: Developer reads .dockerignore
- **WHEN** a developer opens `.dockerignore`
- **THEN** each exclusion rule SHALL be grouped by category with a section comment
- **THEN** the purpose of each group SHALL be self-evident from the comment

### Requirement: No impact on Docker build success
Exclusion rules SHALL NOT remove any file that is explicitly referenced by Dockerfile `COPY` commands during the build stage.

#### Scenario: Build succeeds with optimized ignore
- **WHEN** `docker build -f docker/Dockerfile.idserver ..` is executed
- **THEN** the build SHALL complete successfully with exit code 0
- **THEN** the resulting image SHALL contain the expected application binaries

#### Scenario: Admin build succeeds
- **WHEN** `docker build -f docker/Dockerfile.admin ..` is executed
- **THEN** the build SHALL complete successfully with exit code 0

#### Scenario: Migrator build succeeds
- **WHEN** `docker build -f docker/Dockerfile.migrator ..` is executed
- **THEN** the build SHALL complete successfully with exit code 0
