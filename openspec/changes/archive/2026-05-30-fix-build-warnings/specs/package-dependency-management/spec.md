## ADDED Requirements

### Requirement: NuGet package versions are centrally managed
The system SHALL use Central Package Management (CPM) via `Directory.Packages.props` to define all NuGet package versions in a single location. Each project SHALL reference packages without version numbers.

#### Scenario: Version defined in CPM overrides transitive dependency
- **WHEN** a project references a package that is listed in `Directory.Packages.props`
- **THEN** the version from `Directory.Packages.props` SHALL be used, regardless of transitive dependency versions

### Requirement: All NuGet packages must be free of known critical/high severity vulnerabilities
The system SHALL use only NuGet package versions that have no known high-severity or critical-severity vulnerabilities (as reported by `dotnet restore` NU1903/NU1904 warnings).

#### Scenario: Vulnerability warning is resolved
- **WHEN** `dotnet build` is run
- **THEN** there SHALL be zero NU1903 (high severity) warnings and zero NU1902 (moderate severity) warnings

### Requirement: EFCore and Pomelo versions must be compatible
The `Pomelo.EntityFrameworkCore.MySql` package version SHALL be compatible with the resolved `Microsoft.EntityFrameworkCore.Relational` version, as defined by Pomelo's NuGet dependency constraints.

#### Scenario: No NU1608 version constraint warnings
- **WHEN** `dotnet build` is run
- **THEN** there SHALL be zero NU1608 warnings related to Pomelo.EntityFrameworkCore.MySql
