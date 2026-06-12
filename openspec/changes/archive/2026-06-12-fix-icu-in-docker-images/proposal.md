## Why

Alpine-based Docker images for `idserver` and `admin` crash on startup with `Couldn't find a valid ICU package installed on the system` because `apk add --no-cache icu-data-full` only installs ICU data files, not the shared libraries (`.so` files) that the .NET runtime requires to load `CultureInfo`.

This regression appeared after Alpine 3.21+ changed the `icu-data-full` dependency chain — it no longer pulls `icu-libs` as a transitive dependency.

## What Changes

- `docker/Dockerfile.idserver`: Add `icu-libs` alongside `icu-data-full` in the `RUN apk add` command
- `docker/Dockerfile.admin`: Same change as above

## Capabilities

### New Capabilities

None. This is a build configuration fix, not a new feature.

### Modified Capabilities

None. No spec-level requirement changes.

## Impact

- **Affected files**: `docker/Dockerfile.idserver`, `docker/Dockerfile.admin`
- **Dependencies**: `icu-libs` package added to Alpine runtime stage
- **Image size**: Negligible increase (~2MB for the `.so` libraries)
- **Risk**: Low — `icu-libs` is a stable Alpine package, co-maintained with `icu-data-full`
