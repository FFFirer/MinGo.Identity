## ADDED Requirements

### Requirement: Health check endpoints
The system SHALL provide standardized health check endpoints at `/healthz`, `/readyz`, and `/startupz` for both IdServer and Admin services.

#### Scenario: Liveness probe returns 200
- **WHEN** a GET request is sent to `/healthz`
- **THEN** the response SHALL have HTTP status 200 OK

#### Scenario: Readiness probe returns 200 when dependencies healthy
- **WHEN** a GET request is sent to `/readyz`
- **AND** all configured dependencies are healthy
- **THEN** the response SHALL have HTTP status 200 OK

#### Scenario: Readiness probe returns 503 when dependency unhealthy
- **WHEN** a GET request is sent to `/readyz`
- **AND** a configured dependency is unhealthy
- **THEN** the response SHALL have HTTP status 503 Service Unavailable

#### Scenario: Startup probe returns 200 after startup
- **WHEN** a GET request is sent to `/startupz`
- **AND** the application has completed startup
- **THEN** the response SHALL have HTTP status 200 OK

#### Scenario: Health check endpoints do not require authentication
- **WHEN** a GET request is sent to `/healthz`, `/readyz`, or `/startupz`
- **AND** no authentication token is provided
- **THEN** the response SHALL NOT return 401 or 403

### Requirement: IdServer readiness checks PostgreSQL
The IdServer service SHALL verify PostgreSQL database connectivity as part of the `/readyz` probe.

#### Scenario: PostgreSQL is reachable
- **WHEN** IdServer receives a GET request to `/readyz`
- **AND** PostgreSQL database is reachable and accepting connections
- **THEN** the readiness check SHALL pass

#### Scenario: PostgreSQL is unreachable
- **WHEN** IdServer receives a GET request to `/readyz`
- **AND** PostgreSQL database is unreachable
- **THEN** the readiness check SHALL fail

### Requirement: Admin readiness checks IdServer connectivity
The Admin service SHALL verify IdServer OIDC discovery endpoint reachability as part of the `/readyz` probe.

#### Scenario: IdServer is reachable
- **WHEN** Admin receives a GET request to `/readyz`
- **AND** the configured `IdserverBaseUrl/.well-known/openid-configuration` is reachable
- **THEN** the readiness check SHALL pass

#### Scenario: IdServer is unreachable
- **WHEN** Admin receives a GET request to `/readyz`
- **AND** the configured `IdserverBaseUrl/.well-known/openid-configuration` is unreachable
- **THEN** the readiness check SHALL fail

### Requirement: Shared extension methods in Configuration project
The `MinGo.Identity.Configuration` project SHALL provide reusable extension methods for health check registration and endpoint mapping.

#### Scenario: AddIdentityHealthChecks registers services
- **WHEN** `AddIdentityHealthChecks()` is called on an `IServiceCollection`
- **THEN** health check services SHALL be registered in the DI container

#### Scenario: MapIdentityHealthChecks maps endpoints
- **WHEN** `MapIdentityHealthChecks()` is called on a `WebApplication`
- **THEN** `/healthz`, `/readyz`, and `/startupz` endpoints SHALL be mapped

### Requirement: Docker HEALTHCHECK uses new endpoints
The `docker-compose.prod.yml` HEALTHCHECK definitions SHALL use the new `/readyz` endpoint for both services.

#### Scenario: IdServer HEALTHCHECK targets /readyz
- **WHEN** docker-compose.prod.yml is evaluated
- **THEN** the IdServer healthcheck SHALL use `http://localhost:5001/readyz`

#### Scenario: Admin HEALTHCHECK targets /readyz
- **WHEN** docker-compose.prod.yml is evaluated
- **THEN** the Admin healthcheck SHALL use `http://localhost:5002/readyz`
