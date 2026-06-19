## 1. Configuration

- [ ] 1.1 Create `.vscode/` directory in project root
- [ ] 1.2 Create `.vscode/launch.json` with IdServer launch configuration (`coreclr`, `projectPath` to `src/MinGo.IdServer/MinGo.IdServer.csproj`, `launchProfile: "MinGo.IdServer"`)
- [ ] 1.3 Add Admin launch configuration (`coreclr`, `projectPath` to `src/MinGo.IdServer.Admin/MinGo.IdServer.Admin.csproj`, `launchProfile: "MinGo.IdServer.Admin"`)
- [ ] 1.4 Add `"IdServer + Admin"` compound configuration combining both profiles

## 2. Verification

- [ ] 2.1 Validate `launch.json` syntax is valid JSON
- [ ] 2.2 Verify both `projectPath` references point to existing `.csproj` files
- [ ] 2.3 Verify compound configuration references correct launch configuration names
