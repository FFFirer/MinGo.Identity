## REMOVED Requirements

### Requirement: Run tests in CI/CD pipeline

**Reason**: No test projects exist in the repository. The `dotnet test` step added unnecessary overhead (~10-30s per run) with no functional value.

**Migration**: When test projects are added in the future, add a `Run tests` step back to the build job with the appropriate `dotnet test` command.
