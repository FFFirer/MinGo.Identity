## ADDED Requirements

### Requirement: Production Docker Compose file exists
The system SHALL provide a `docker/docker-compose.prod.yml` file for orchestrating the full production stack.

#### Scenario: docker-compose.prod.yml exists
- **WHEN** listing files in `docker/`
- **THEN** `docker-compose.prod.yml` SHALL exist

### Requirement: All services defined
The production Compose file SHALL define services for PostgreSQL, IdServer, Admin, and Migrator.

#### Scenario: PostgreSQL service defined
- **WHEN** inspecting `docker-compose.prod.yml`
- **THEN** a `postgres` service SHALL be defined using `postgres:16-alpine` with a named volume for persistence

#### Scenario: IdServer service defined
- **WHEN** inspecting `docker-compose.prod.yml`
- **THEN** an `idserver` service SHALL be defined using the `mingo-identity-idserver` image, mapped to port 5001

#### Scenario: Admin service defined
- **WHEN** inspecting `docker-compose.prod.yml`
- **THEN** an `admin` service SHALL be defined using the `mingo-identity-admin` image, mapped to port 5002, with dependency on `idserver`

#### Scenario: Migrate service defined (one-shot)
- **WHEN** inspecting `docker-compose.prod.yml`
- **THEN** a `migrate` service SHALL be defined using the `mingo-identity-migrator` image with command `migrate`, and a Compose profile `migrate` so it only runs when explicitly invoked

### Requirement: Service dependencies and startup order
The Compose file SHALL specify service dependencies to ensure correct startup order.

#### Scenario: IdServer depends on PostgreSQL
- **WHEN** inspecting `idserver` service
- **THEN** `depends_on` SHALL include `postgres` with `condition: service_healthy`

#### Scenario: Admin depends on IdServer
- **WHEN** inspecting `admin` service
- **THEN** `depends_on` SHALL include `idserver` with `condition: service_started`

### Requirement: Environment configuration
The Compose file SHALL support environment variable overrides for all configurable settings.

#### Scenario: Connection string configurable via env
- **WHEN** the `idserver` service starts
- **THEN** `ConnectionStrings__DefaultConnection` or environment variable mapping SHALL be configurable
