## Context

MinGo.Identity consists of 3 deployable .NET 10.0 services (IdServer, Admin, Migrator) plus a PostgreSQL database. The project currently only has a `docker-compose.yml` for local PostgreSQL. There are no Dockerfiles, no CI pipeline, and no automated build process. Deployment is manual via `dotnet run` or `dotnet publish`.

The migration guide already documents the intended deployment pattern (Init Container / Entrypoint script), and the architecture separates migration from runtime concerns. This design implements the containerization and CI infrastructure to realize that architecture.

## Goals / Non-Goals

**Goals:**
- Create multi-stage Dockerfiles for all 3 deployable services (IdServer, Admin, Migrator)
- Create `docker-compose.prod.yml` for full-stack production deployment
- Create `.dockerignore` to optimize Docker build context
- Create Gitea Workflows CI pipeline (`.gitea/workflows/build.yml`) that:
  - Runs on push to main/master and version tags
  - Restores NuGet packages (with caching)
  - Builds all projects in Release configuration
  - Runs tests (when they exist)
  - Builds and pushes Docker images to Gitea Container Registry
- Use .NET 10.0 SDK/Runtime images as base (matching project TFM)

**Non-Goals:**
- Not creating K8s manifests (Helm/Kustomize) — future work
- Not setting up Gitea Act Runner itself — assumes runner exists
- Not modifying any existing .csproj or source files
- Not configuring CD (continuous deployment) — CI only for now
- Not adding health checks or readiness probes beyond basic Docker HEALTHCHECK

## Decisions

### Decision 1: Multi-stage Dockerfiles per service
- **Choice**: One Dockerfile per deployable service under `docker/`, named `Dockerfile.idserver`, `Dockerfile.admin`, `Dockerfile.migrator`
- **Rationale**: Clear separation; each service has different publish profiles and entrypoints. Naming convention avoids confusion with the project directory.
- **Structure**:
  ```
  docker/
  ├── Dockerfile.idserver    # ASP.NET Core, port 5001
  ├── Dockerfile.admin       # Blazor SSR, port 5002
  ├── Dockerfile.migrator    # Console, migrate/script commands
  └── docker-compose.prod.yml
  ```
- **Alternatives**: Single Dockerfile with build args — rejected because it complicates CI parallel builds and readability.

### Decision 2: Alpine-based runtime images
- **Choice**: `mcr.microsoft.com/dotnet/aspnet:10.0-alpine` for runtime, `mcr.microsoft.com/dotnet/sdk:10.0` for build
- **Rationale**: Alpine images are ~100MB vs 200MB+ for Ubuntu-based. Minimal attack surface. .NET 10 on Alpine is production-ready.
- **Alternatives**: Ubuntu (`jammy`) — larger but more familiar debugging tools.

### Decision 3: Gitea Container Registry for image storage
- **Choice**: Use Gitea built-in Container Registry (`gitea/<owner>/<repo>/mingo-identity-<service>:<tag>`)
- **Rationale**: Gitea has native OCI registry support. No external dependency on Docker Hub or GHCR. Images stay in the same org/repo namespace as source code.
- **Tag strategy**: `{commit-sha}` (unique), `main` (latest on main branch), `v*.*.*` (semver releases)

### Decision 4: CI triggers — push to main + version tags
- **Choice**: CI runs on push to `main`, `master`, and tags matching `v*`
- **Rationale**: Feature branch CI runs build+test only (no Docker push). Main/master and tags trigger full pipeline including Docker push.
- **Alternatives**: Run Docker on every push — wasteful for WIP branches.

### Decision 5: Parallel Docker builds in CI
- **Choice**: Build all 3 Docker images in parallel using Gitea workflow job matrix strategy
- **Rationale**: 3 images are independent; parallel builds reduce total CI time from ~15min to ~6min.
- **Implementation**: Gitea workflow job matrix over `service: [idserver, admin, migrator]`

### Decision 6: NuGet cache optimization
- **Choice**: Cache `~/.nuget/packages` between CI runs using Gitea actions cache
- **Rationale**: `dotnet restore` takes 30-60s on first run; caching reduces to ~5s on subsequent runs.
- **Trade-off**: Cache invalidation when `Directory.Packages.props` changes — handled automatically by cache key.

### Decision 7: Migrator as separate deployment concern
- **Choice**: Migrator image built but NOT included in `docker-compose.prod.yml` as a long-running service; instead documented as a run-once Job/Init Container
- **Rationale**: Migration is a deployment-time operation, not a runtime service. The `docker-compose.prod.yml` will include a `migrate` service with `profiles: ["migrate"]` so it only runs when explicitly invoked.

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| Alpine base image may miss native dependencies for EF Core providers | Test PostgreSQL connectivity in CI; if issues, fall back to `jammy` |
| Gitea Container Registry may not be available or disabled | Document fallback to Docker Hub; make registry configurable via CI secrets |
| Docker build context large if `.dockerignore` misses patterns | Explicitly exclude `bin/`, `obj/`, `.git/, node_modules/`, test projects |
| `dotnet publish` for 3 services increases CI time | Parallel builds via matrix strategy; consider `--no-restore` with cached packages |
| CI runner requires Docker-in-Docker or socket binding | Document runner requirement (mounted `/var/run/docker.sock` or `dind` service) |
