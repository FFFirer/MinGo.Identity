## ADDED Requirements

### Requirement: Blazor-based admin management UI
The system SHALL provide a management UI built with Blazor Interactive Server using the `SimpleIdServer.IdServer.Website` (7.0.0) package. The admin UI SHALL authenticate against the IdServer via OIDC.

#### Scenario: Admin portal loads
- **WHEN** a browser navigates to `https://localhost:5002`
- **THEN** the admin portal loads and redirects to the IdServer login page

#### Scenario: OIDC authentication flow succeeds
- **WHEN** a user authenticates with valid credentials on the IdServer login page
- **THEN** the user is redirected back to the admin portal and gains access

### Requirement: Admin connects to IdServer via OIDC
The system SHALL configure the admin portal to use the IdServer at `https://localhost:5001` as its OIDC authority. Client credentials SHALL be `admin_client` / `admin_secret`.

#### Scenario: UpdateOpenid configures OIDC
- **WHEN** `UpdateOpenid("admin_client", "admin_secret", scopes, ignoreCertError)` is called
- **THEN** the admin portal is configured to use the IdServer for authentication

### Requirement: Realm support
The admin portal SHALL support multi-realm management when `IsRealmEnabled` is true.

#### Scenario: Realm configuration applied
- **WHEN** `adminBuilder.EnableRealm()` is called in the setup
- **THEN** the admin portal displays realm selection and management UI

### Requirement: Port binding
The Admin portal SHALL listen on `https://localhost:5002`.

#### Scenario: Port is bound
- **WHEN** the application starts
- **THEN** it listens on `https://localhost:5002`
