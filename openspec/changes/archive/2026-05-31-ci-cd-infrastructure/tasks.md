## 1. Create .dockerignore

- [x] 1.1 Create root `.dockerignore` excluding `bin/`, `obj/`, `.git/`, `node_modules/`, `.opencode/`, `docs/`, `openspec/changes/archive/`

## 2. Create Dockerfile for IdServer

- [x] 2.1 Create `docker/Dockerfile.idserver` with multi-stage build (sdk:10.0 build → aspnet:10.0-alpine runtime)
- [x] 2.2 Set ASPNETCORE_URLS=http://+:5001, non-root user, HEALTHCHECK
- [x] 2.3 Verify Docker build succeeds: `docker build -f docker/Dockerfile.idserver -t idserver:test .`

## 3. Create Dockerfile for Admin

- [x] 3.1 Create `docker/Dockerfile.admin` with multi-stage build
- [x] 3.2 Set ASPNETCORE_URLS=http://+:5002, non-root user, HEALTHCHECK
- [x] 3.3 Verify Docker build succeeds: `docker build -f docker/Dockerfile.admin -t admin:test .`

## 4. Create Dockerfile for Migrator

- [x] 4.1 Create `docker/Dockerfile.migrator` with multi-stage build
- [x] 4.2 Set entrypoint to `dotnet MinGo.Identity.Migrator.dll` with `migrate` as default command
- [x] 4.3 Verify Docker build succeeds: `docker build -f docker/Dockerfile.migrator -t migrator:test .`

## 5. Create Production Docker Compose

- [x] 5.1 Create `docker/docker-compose.prod.yml` with PostgreSQL 16-alpine service
- [x] 5.2 Add `idserver` service (port 5001, depends_on postgres with healthcheck)
- [x] 5.3 Add `admin` service (port 5002, depends_on idserver)
- [x] 5.4 Add `migrate` service (one-shot, profile: migrate, depends_on postgres)
- [x] 5.5 Add persistent volume for PostgreSQL data
- [x] 5.6 Verify Compose config: `docker compose -f docker/docker-compose.prod.yml config`

## 6. Create Gitea Workflows CI Pipeline

- [x] 6.1 Create `.gitea/workflows/build.yml` with trigger rules (push to main/master, tags v*, PR)
- [x] 6.2 Add `build` job: restore NuGet (with cache) → build Release → test
- [x] 6.3 Add `docker` job using matrix strategy: [idserver, admin, migrator]
- [x] 6.4 Add Docker login step using Gitea Container Registry secrets
- [x] 6.5 Add Docker build and push step with multi-tag strategy (commit SHA, branch, version)
- [x] 6.6 Add job dependency: docker needs build

## 7. Final Verification

- [x] 7.1 Verify all CI pipeline files exist and are syntactically valid YAML
- [x] 7.2 Verify all Docker build commands succeed for all 3 services
- [x] 7.3 Verify `.dockerignore` effectively reduces build context size
