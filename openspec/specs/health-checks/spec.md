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

### Requirement: IdServer readiness checks all database backends
The IdServer service SHALL verify database connectivity as part of the `/readyz` probe. The check SHALL support all configured storage types.

#### Scenario: PostgreSQL database is reachable
- **WHEN** IdServer receives a GET request to `/readyz`
- **AND** `StorageConfiguration.Type` is `POSTGRE`
- **AND** the PostgreSQL database is reachable and accepting connections
- **THEN** the readiness check SHALL pass

#### Scenario: PostgreSQL database is unreachable
- **WHEN** IdServer receives a GET request to `/readyz`
- **AND** `StorageConfiguration.Type` is `POSTGRE`
- **AND** the PostgreSQL database is unreachable
- **THEN** the readiness check SHALL fail with status 503

#### Scenario: SQL Server database is reachable
- **WHEN** IdServer receives a GET request to `/readyz`
- **AND** `StorageConfiguration.Type` is `SQLSERVER`
- **AND** the SQL Server database is reachable and accepting connections
- **THEN** the readiness check SHALL pass

#### Scenario: SQL Server database is unreachable
- **WHEN** IdServer receives a GET request to `/readyz`
- **AND** `StorageConfiguration.Type` is `SQLSERVER`
- **AND** the SQL Server database is unreachable
- **THEN** the readiness check SHALL fail with status 503

#### Scenario: SQLite database is reachable
- **WHEN** IdServer receives a GET request to `/readyz`
- **AND** `StorageConfiguration.Type` is `SQLITE`
- **AND** the SQLite database file is accessible
- **THEN** the readiness check SHALL pass

#### Scenario: InMemory storage skips database check
- **WHEN** IdServer receives a GET request to `/readyz`
- **AND** `StorageConfiguration.Type` is `INMEMORY`
- **THEN** the readiness check SHALL NOT include a database health check

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

### Requirement: Shared AddIdentityHealthChecks accepts IConfiguration
The `MinGo.Identity.Configuration` project SHALL provide an `AddIdentityHealthChecks(IConfiguration)` extension method that reads configuration and auto-registers health checks based on configured dependencies.

#### Scenario: AddIdentityHealthChecks registers IdServer checks
- **WHEN** `AddIdentityHealthChecks(configuration)` is called on IdServer's `IServiceCollection`
- **AND** `StorageConfiguration.Type` is `POSTGRE`
- **THEN** a `DatabaseHealthCheck` SHALL be registered with tag `"ready"`

#### Scenario: AddIdentityHealthChecks registers Admin checks
- **WHEN** `AddIdentityHealthChecks(configuration)` is called on Admin's `IServiceCollection`
- **THEN** an `IdServerEndpointCheck` SHALL be registered with tag `"ready"`

#### Scenario: AddIdentityHealthChecks registers KeyValueStore check
- **WHEN** `AddIdentityHealthChecks(configuration)` is called
- **AND** `KeyValueConfiguration.Type` is `REDIS`
- **THEN** a `KeyValueStoreHealthCheck` SHALL be registered with tag `"ready"`

#### Scenario: AddIdentityHealthChecks registers MessageBroker check
- **WHEN** `AddIdentityHealthChecks(configuration)` is called
- **AND** `MessageBrokerOptions.Transport` is `SQLSERVER`
- **THEN** a `MessageBrokerHealthCheck` SHALL be registered with tag `"ready"`

### Requirement: MapIdentityHealthChecks maps endpoints
The `MinGo.Identity.Configuration` project SHALL provide a `MapIdentityHealthChecks(this WebApplication)` extension method that maps `/healthz`, `/readyz`, and `/startupz` endpoints.

#### Scenario: MapIdentityHealthChecks maps single endpoint
- **WHEN** `MapIdentityHealthChecks()` is called on a `WebApplication`
- **THEN** `/healthz` endpoint SHALL be mapped with predicate `_ => false`
- **THEN** `/readyz` endpoint SHALL be mapped with predicate filtering tag `"ready"`
- **THEN** `/startupz` endpoint SHALL be mapped with predicate `_ => false`

### Requirement: IdServer readiness checks Redis key-value store
The IdServer service SHALL verify Redis key-value store connectivity as part of the `/readyz` probe when Redis is configured.

#### Scenario: Redis KV store is reachable
- **WHEN** IdServer receives a GET request to `/readyz`
- **AND** `KeyValueConfiguration.Type` is `REDIS`
- **AND** the Redis server responds to PING
- **THEN** the readiness check SHALL pass

#### Scenario: Redis KV store is unreachable
- **WHEN** IdServer receives a GET request to `/readyz`
- **AND** `KeyValueConfiguration.Type` is `REDIS`
- **AND** the Redis server is unreachable
- **THEN** the readiness check SHALL fail with status 503

#### Scenario: InMemory KV store skips check
- **WHEN** IdServer receives a GET request to `/readyz`
- **AND** `KeyValueConfiguration.Type` is `INMEMORY`
- **THEN** the readiness check SHALL NOT include a key-value store health check

### Requirement: IdServer readiness checks message broker transport
The IdServer service SHALL verify MassTransit message broker transport connectivity as part of the `/readyz` probe when external transport is configured.

#### Scenario: SQL Server transport is reachable
- **WHEN** IdServer receives a GET request to `/readyz`
- **AND** `MessageBrokerOptions.Transport` is `SQLSERVER`
- **AND** the SQL Server transport database is reachable
- **THEN** the readiness check SHALL pass

#### Scenario: SQL Server transport is unreachable
- **WHEN** IdServer receives a GET request to `/readyz`
- **AND** `MessageBrokerOptions.Transport` is `SQLSERVER`
- **AND** the SQL Server transport database is unreachable
- **THEN** the readiness check SHALL fail with status 503

#### Scenario: InMemory transport skips check
- **WHEN** IdServer receives a GET request to `/readyz`
- **AND** `MessageBrokerOptions.Transport` is `INMEMORY`
- **THEN** the readiness check SHALL NOT include a message broker health check

### Requirement: Admin readiness checks SCIM endpoint (optional)
The Admin service MAY verify SCIM endpoint reachability as part of the `/readyz` probe when `ScimBaseUrl` is configured.

#### Scenario: SCIM endpoint check passes when configured and reachable
- **WHEN** Admin receives a GET request to `/readyz`
- **AND** `ScimBaseUrl` is non-empty
- **AND** the SCIM endpoint is reachable
- **THEN** the SCIM health check SHALL pass

#### Scenario: SCIM endpoint check degrades when unreachable
- **WHEN** Admin receives a GET request to `/readyz`
- **AND** `ScimBaseUrl` is non-empty
- **AND** the SCIM endpoint is unreachable
- **THEN** the SCIM health check SHALL report Degraded status
- **THEN** the overall /readyz status SHALL remain Healthy

### Requirement: Health check response is structured JSON
All health check endpoints SHALL return a structured JSON response containing component-level status, descriptions, and durations.

#### Scenario: /readyz returns JSON with entry details
- **WHEN** a GET request is sent to `/readyz`
- **THEN** the response body SHALL be valid JSON
- **THEN** the response body SHALL contain a `status` field
- **THEN** the response body SHALL contain an `entries` object with per-check details

### Requirement: Health checks resolve through DI
All health check implementations SHALL be resolved through the DI container and SHALL NOT manually create their dependencies.

#### Scenario: DatabaseHealthCheck uses ILogger
- **WHEN** `DatabaseHealthCheck.CheckHealthAsync()` executes
- **THEN** it SHALL use constructor-injected `ILogger<DatabaseHealthCheck>`

#### Scenario: IdServerEndpointCheck uses IHttpClientFactory
- **WHEN** `IdServerEndpointCheck.CheckHealthAsync()` executes
- **THEN** it SHALL use constructor-injected `IHttpClientFactory` to create HTTP clients
- **THEN** it SHALL NOT create `HttpClient` instances directly

### Requirement: Docker HEALTHCHECK uses new endpoints
The `docker-compose.prod.yml` HEALTHCHECK definitions SHALL use the new `/readyz` endpoint for both services.

#### Scenario: IdServer HEALTHCHECK targets /readyz
- **WHEN** docker-compose.prod.yml is evaluated
- **THEN** the IdServer healthcheck SHALL use `http://localhost:5001/readyz`

#### Scenario: Admin HEALTHCHECK targets /readyz
- **WHEN** docker-compose.prod.yml is evaluated
- **THEN** the Admin healthcheck SHALL use `http://localhost:5002/readyz`
