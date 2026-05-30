## Why

MinGo.Identity currently has no automated CI/CD pipeline or containerized deployment artifacts. Developers must build and deploy manually, and the project's architecture (IdServer + Admin + Migrator + PostgreSQL) is designed for containerized deployment but lacks the Dockerfiles and CI workflows to realize it. Establishing a proper CI/CD pipeline ensures consistent builds, automated testing, and reproducible deployments for both development and production environments.

## What Changes

- **Dockerfiles**: Create multi-stage Dockerfiles for all 3 deployable services (IdServer, Admin, Migrator)
- **Docker Compose (Production)**: Create `docker-compose.prod.yml` for orchestrating the full stack including PostgreSQL
- **.dockerignore**: Add root-level `.dockerignore` to optimize build context
- **Gitea Workflows CI**: Create `.gitea/workflows/build.yml` with build, test, and Docker image publishing pipeline
- **Image Registry Configuration**: Configure Gitea container registry integration for image storage

## Capabilities

### New Capabilities
- `container-image-build`: Multi-stage Docker builds for IdServer, Admin, and Migrator images using .NET 10 SDK
- `ci-pipeline`: Gitea Workflows CI pipeline with restore/build/test/docker-push stages
- `production-deployment`: Docker Compose production deployment configuration with PostgreSQL dependency management

### Modified Capabilities
<!-- No existing specs are modified — this is new infrastructure -->

## Impact

- **New files**: `.dockerignore`, `docker/Dockerfile.idserver`, `docker/Dockerfile.admin`, `docker/Dockerfile.migrator`, `docker/docker-compose.prod.yml`, `.gitea/workflows/build.yml`
- **No source code changes**: All .NET project files remain unmodified
- **Build optimization**: Multi-stage builds produce minimal production images (~200MB each) by separating SDK and runtime stages
- **Dependencies**: Requires Gitea Act Runner with Docker capability for CI execution
