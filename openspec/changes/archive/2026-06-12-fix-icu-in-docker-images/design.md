## Context

Current Dockerfiles for `idserver` and `admin` both install ICU via:

```dockerfile
RUN apk add --no-cache icu-data-full
```

On Alpine 3.21+, this package only installs ICU data files (character mappings, collation tables) but NOT the shared libraries (`libicuuc.so`, `libicui18n.so`). The .NET runtime requires these `.so` files to initialize `CultureInfo`.

The `icu-libs` package (Alpine 3.19-) / `icu` meta-package provides these shared libraries. Since Alpine 3.21+, `icu-data-full` no longer pulls `icu-libs` as a dependency.

## Goals / Non-Goals

**Goals:**
- Add `icu-libs` to the `apk add` command in both `Dockerfile.idserver` and `Dockerfile.admin`
- Ensure idserver and admin containers start without ICU-related crash
- Minimal image size increase

**Non-Goals:**
- Not touching `Dockerfile.migrator` (console app, no ICU dependency at startup)
- Not switching base images (keep `aspnet:10.0-alpine`)

## Decisions

| Decision | Choice | Rationale |
|---|---|---|
| Package selection | `icu-libs` + `icu-data-full` instead of meta `icu` | Explicit is better. `icu-libs` provides exactly the `.so` files .NET needs; `icu-data-full` provides full Unicode data. The meta `icu` pulls both anyway, but separating them makes the dependency intent clearer. |
| Not switching to `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1` | Keep `=0` | The SimpleIdServer framework uses `CultureInfo` for OIDC protocol handling. Invariant mode may cause subtle bugs in locale-sensitive string operations. |
| Not switching to Ubuntu base image | Stay on Alpine | Alpine provides significantly smaller images (~100MB vs ~200MB). The ICU issue is a simple package fix, not a fundamental Alpine limitation. |

## Risks / Trade-offs

| Risk | Mitigation |
|---|---|
| `icu-libs` package adds ~2MB to image size | Acceptable — the `.so` files are necessary for .NET globalization |
| Future Alpine versions change ICU packaging again | Pin ICU package versions in Dockerfile if this becomes recurrent. Currently using latest is fine. |
| `migrator` could eventually hit the same issue | Not needed now (console app doesn't trigger `CultureInfo` init), but if it uses `Host.CreateDefaultBuilder()` in the future, same fix applies |
