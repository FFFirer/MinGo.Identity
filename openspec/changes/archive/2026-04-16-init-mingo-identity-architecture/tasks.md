## 1. Project Infrastructure Setup

- [x] 1.1 Create solution root directory and folder structure (src/, docker/, docs/)
- [x] 1.2 Create `Directory.Packages.props` with CPM configuration for all NuGet dependencies
- [x] 1.3 Create `Directory.Build.props` with shared MSBuild properties (TargetFramework: net10.0, Nullable: enable)
- [x] 1.4 Create `nuget.config` with package source configuration
- [x] 1.5 Create solution file `MinGo.Identity.sln`
- [x] 1.6 Initialize Git repository and add `.gitignore` for .NET projects

## 2. Docker Development Environment

- [x] 2.1 Create `docker-compose.yml` with PostgreSQL 16 service configuration
- [x] 2.2 Configure PostgreSQL container with persistent volume for data storage
- [x] 2.3 Add health check for PostgreSQL container startup
- [x] 2.4 Document environment variables (POSTGRES_USER, POSTGRES_PASSWORD, POSTGRES_DB)

## 3. IdServer Core Service (Port 5001)

- [x] 3.1 Create `MinGo.IdServer` project from SimpleIdServer `idserver` template
- [x] 3.2 Configure project file with CPM package references (no version numbers)
    - SimpleIdServer.IdServer
    - SimpleIdServer.IdServer.Pwd
    - SimpleIdServer.IdServer.Email
    - SimpleIdServer.IdServer.PostgreSQLMigrations
    - Microsoft.EntityFrameworkCore.Design (private assets)
- [x] 3.3 Create `appsettings.json` with PostgreSQL connection string configuration
- [x] 3.4 Configure `appsettings.Development.json` with developer-specific settings
- [x] 3.5 Implement `Program.cs` with:
    - `AddSidIdentityServer()` configuration
    - `AddDeveloperSigningCredential()` for dev environment
    - `AddPostgreSQL()` with connection string
    - `AddPwdAuthentication(true)` for password auth
    - `AddEmailNotification(true)` for console email output
    - `app.UseSid()` middleware
    - Seed data initialization
- [x] 3.6 Configure HTTPS development certificate for local development
- [x] 3.7 Set application URL to `https://localhost:5001`
- [x] 3.8 Add default administrator user (administrator/password) via seed data

## 4. Admin Portal Service (Port 5002)

- [x] 4.1 Create `MinGo.IdServer.Admin` project from SimpleIdServer `idserveradmin` template
- [x] 4.2 Configure project file with CPM package references:
    - SimpleIdServer.IdServer.Admin
    - Microsoft.AspNetCore.Authentication.OpenIdConnect
- [x] 4.3 Configure `appsettings.json` with IdServer base URL pointing to port 5001
- [x] 4.4 Implement `Program.cs` with Admin service configuration
- [x] 4.5 Configure OIDC authentication to connect to IdServer (port 5001)
- [x] 4.6 Set application URL to `https://localhost:5002`
- [x] 4.7 Ensure Admin service uses HTTPS and trusts IdServer certificate

## 5. Verification and Testing

- [ ] 5.1 Start PostgreSQL container: `docker-compose up -d postgres`
- [ ] 5.2 Run `dotnet restore` to verify CPM configuration works
- [ ] 5.3 Build solution: `dotnet build` with no errors
- [ ] 5.4 Start IdServer and verify it listens on `https://localhost:5001`
- [ ] 5.5 Verify OIDC Discovery endpoint returns valid JSON: `GET /.well-known/openid-configuration`
- [ ] 5.6 Start Admin service and verify it connects to IdServer
- [ ] 5.7 Test login to Admin portal with administrator/password credentials
- [ ] 5.8 Create a test OIDC client via Admin UI
- [ ] 5.9 Test Authorization Code flow with the test client
- [ ] 5.10 Test Token Introspection endpoint with valid and expired tokens

## 6. Documentation

- [x] 6.1 Create `README.md` with project overview and quick start guide
- [x] 6.2 Document local development setup steps
- [x] 6.3 Document CPM configuration and how to add/update NuGet packages
- [x] 6.4 Create architecture diagram showing service interactions
- [x] 6.5 Document port assignments and service responsibilities
- [x] 6.6 Add troubleshooting guide for common setup issues (certificate errors, database connection)

## 7. Optional: UI Customization Foundation

- [x] 7.1 Create `Areas/Password/Pages/` directory structure in IdServer project
- [x] 7.2 Create placeholder `Authenticate.cshtml` with Tailwind CSS CDN include
- [x] 7.3 Document the Razor Pages Area override mechanism
- [x] 7.4 Create sample Tailwind configuration in `_ViewImports.cshtml`
