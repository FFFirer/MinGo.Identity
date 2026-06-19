## ADDED Requirements

### Requirement: One-click launch for IdServer and Admin
The system SHALL provide a VS Code compound launch configuration that starts both MinGo.IdServer and MinGo.IdServer.Admin simultaneously.

#### Scenario: Compound launch starts both projects
- **WHEN** developer presses F5 with "IdServer + Admin" compound profile selected
- **THEN** both MinGo.IdServer (port 5001) and MinGo.IdServer.Admin (port 5002) start
- **AND** developer can set breakpoints in both projects

### Requirement: Individual project launch
Each project SHALL have its own standalone VS Code launch configuration that can be started independently.

#### Scenario: Launch IdServer alone
- **WHEN** developer selects "IdServer" launch profile in VS Code
- **THEN** only MinGo.IdServer starts on port 5001
- **AND** the debugger attaches to the IdServer process

#### Scenario: Launch Admin alone
- **WHEN** developer selects "Admin" launch profile in VS Code
- **THEN** only MinGo.IdServer.Admin starts on port 5002
- **AND** the debugger attaches to the Admin process

### Requirement: Existing launchSettings.json preserved
The VS Code launch configuration MUST reference profiles from existing `Properties/launchSettings.json` files rather than duplicating port and environment variable settings.

#### Scenario: Port change in launchSettings.json propagates
- **WHEN** developer changes the port in `MinGo.IdServer/Properties/launchSettings.json`
- **THEN** the same port is used when launching from VS Code without modifying `.vscode/launch.json`

### Requirement: Admin browser auto-launch
The Admin launch configuration SHALL automatically open the browser to `https://localhost:5002` upon start.

#### Scenario: Admin starts with browser
- **WHEN** developer launches Admin (standalone or as part of compound)
- **THEN** VS Code automatically opens the default browser to `https://localhost:5002`
