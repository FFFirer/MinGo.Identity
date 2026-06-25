## 1. Middleware Registration

- [ ] 1.1 Add `app.UseStaticFiles()` in `src/MinGo.IdServer.Admin/Program.cs` after `UseExceptionHandler` and before `UseIdserverAdmin`

## 2. Verification

- [ ] 2.1 Run `dotnet publish` and confirm `wwwroot/_framework/blazor.web.js` exists in output
- [ ] 2.2 Run locally with `ASPNETCORE_ENVIRONMENT=Production` to confirm static files serve correctly
