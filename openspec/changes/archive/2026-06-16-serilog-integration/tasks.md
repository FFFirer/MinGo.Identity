## 1. Package Management (CPM)

- [x] 1.1 Add `Serilog.AspNetCore` and `Serilog.Formatting.Compact` package versions to `Directory.Packages.props`
- [x] 1.2 Add `PackageReference` for `Serilog.AspNetCore` to `MinGo.IdServer.Admin.csproj`
- [x] 1.3 Add `PackageReference` for `Serilog.AspNetCore` to `MinGo.IdServer.csproj`

## 2. Admin Project (MinGo.IdServer.Admin)

- [x] 2.1 Add `using Serilog;` and `builder.Host.UseSerilog(...)` with `ReadFrom.Configuration` to `Admin/Program.cs` (before `AddIdserverAdmin`)
- [x] 2.2 Add `Serilog` configuration section to `Admin/appsettings.json` with Console + RenderedCompactJsonFormatter + MinimumLevel overrides
- [x] 2.3 Create `Admin/appsettings.Development.json` with Console sink override (no JSON formatter)

## 3. IdServer Project (MinGo.IdServer)

- [x] 3.1 Add `using Serilog;` and `webApplicationBuilder.Host.UseSerilog(...)` with `ReadFrom.Configuration` to `IdServer/Program.cs` (before `SidServerSetup.ConfigureIdServer`)
- [x] 3.2 Add `Serilog` configuration section to `IdServer/appsettings.json` with Console + RenderedCompactJsonFormatter + MinimumLevel overrides
- [x] 3.3 Create `IdServer/appsettings.Development.json` with Console sink override (no JSON formatter)

## 4. Verification

- [x] 4.1 Build the solution: `dotnet build` passes with 0 errors
- [x] 4.2 Run LSP diagnostics on changed files — clean (no diagnostics)
- [x] 4.3 Verify Development mode outputs readable text (manual check)
- [x] 4.4 Verify default (production) mode outputs structured JSON (manual check)
