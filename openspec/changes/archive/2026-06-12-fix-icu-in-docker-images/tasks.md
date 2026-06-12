## 1. Dockerfile Fixes

- [ ] 1.1 Update `docker/Dockerfile.idserver` — change `RUN apk add --no-cache icu-data-full` to `RUN apk add --no-cache icu-libs icu-data-full`
- [ ] 1.2 Update `docker/Dockerfile.admin` — change `RUN apk add --no-cache icu-data-full` to `RUN apk add --no-cache icu-libs icu-data-full`

## 2. Verification

- [ ] 2.1 Verify files are syntactically valid (Dockerfile format)
