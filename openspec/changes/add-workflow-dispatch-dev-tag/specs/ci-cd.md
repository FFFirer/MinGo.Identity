## ADDED Requirements

### Requirement: Manual workflow trigger support
The CI/CD pipeline SHALL support manual triggering via Gitea's `workflow_dispatch` event.
When triggered manually, the system SHALL produce Docker images tagged with `dev` in addition to the commit SHA tag.

#### Scenario: Manual trigger produces dev tag
- **WHEN** a user triggers the workflow manually from Gitea UI
- **THEN** Docker images are built and pushed with both `{sha}` and `dev` tags

#### Scenario: Manual trigger pushes to registry
- **WHEN** a user triggers the workflow manually from Gitea UI
- **THEN** Docker images SHALL be pushed to the container registry

#### Scenario: Existing triggers unaffected
- **WHEN** the workflow is triggered by push, tag, or pull_request
- **THEN** the `dev` tag SHALL NOT be produced
- **THEN** the existing tag behavior (sha, semver, latest) SHALL remain unchanged
