## ADDED Requirements

### Requirement: OIDC Discovery Endpoint
The IdServer SHALL expose a well-known OpenID Connect discovery document at `/.well-known/openid-configuration` containing all necessary endpoint URLs and supported features.

#### Scenario: Retrieve discovery document
- **WHEN** a client application sends GET request to `https://localhost:5001/.well-known/openid-configuration`
- **THEN** the system SHALL return a JSON document containing issuer, authorization_endpoint, token_endpoint, userinfo_endpoint, and supported scopes

### Requirement: OAuth2 Authorization Endpoint
The IdServer SHALL implement the `/connect/authorize` endpoint supporting the Authorization Code flow with PKCE.

#### Scenario: Authorization code request with PKCE
- **WHEN** a user agent redirects to `/connect/authorize?response_type=code&client_id={client_id}&redirect_uri={uri}&scope={scope}&code_challenge={challenge}&code_challenge_method=S256`
- **THEN** the system SHALL authenticate the user and redirect back with an authorization code

#### Scenario: Invalid client rejection
- **WHEN** a request contains an invalid or unauthorized client_id
- **THEN** the system SHALL return an error response with error=unauthorized_client

### Requirement: OAuth2 Token Endpoint
The IdServer SHALL implement the `/connect/token` endpoint for exchanging authorization codes and client credentials for access tokens.

#### Scenario: Exchange authorization code for tokens
- **WHEN** a client sends POST to `/connect/token` with grant_type=authorization_code, code, redirect_uri, client_id, client_secret, and code_verifier
- **THEN** the system SHALL return an access_token, id_token (if openid scope), and refresh_token

#### Scenario: Client credentials flow
- **WHEN** a backend service sends POST to `/connect/token` with grant_type=client_credentials, client_id, and client_secret
- **THEN** the system SHALL return an access_token for the requested scopes

### Requirement: Token Validation Endpoint
The IdServer SHALL provide `/connect/introspect` endpoint for resource servers to validate access tokens.

#### Scenario: Valid token introspection
- **WHEN** a resource server sends POST to `/connect/introspect` with a valid access_token
- **THEN** the system SHALL return active=true with token claims (sub, scope, exp, etc.)

#### Scenario: Expired token introspection
- **WHEN** a resource server sends POST to `/connect/introspect` with an expired access_token
- **THEN** the system SHALL return active=false

### Requirement: PostgreSQL Persistence
The IdServer SHALL use PostgreSQL as the persistent storage for identity data including users, clients, scopes, grants, and cryptographic keys.

#### Scenario: Database migration on startup
- **WHEN** the IdServer starts and detects pending Entity Framework migrations
- **THEN** the system SHALL automatically apply migrations to bring the database schema to the current version

#### Scenario: Client storage in PostgreSQL
- **WHEN** an administrator creates a new OIDC client via the Admin API
- **THEN** the system SHALL persist the client configuration to PostgreSQL and make it available for authentication flows

### Requirement: User Authentication
The IdServer SHALL support password-based user authentication with secure credential storage.

#### Scenario: Successful user login
- **WHEN** a user submits valid credentials (username and password) to the authentication endpoint
- **THEN** the system SHALL authenticate the user and establish a session for subsequent OIDC flows

#### Scenario: Failed user login
- **WHEN** a user submits invalid credentials
- **THEN** the system SHALL reject the authentication with appropriate error messaging and security logging

### Requirement: Developer Signing Credential
The IdServer SHALL use developer signing credentials for token signing in non-production environments.

#### Scenario: Token signing on startup
- **WHEN** the IdServer starts with AddDeveloperSigningCredential() configuration
- **THEN** the system SHALL generate or load development signing keys for JWT token issuance

**WARNING**: This SHALL NOT be used in production environments.
