## 1. Add workflow_dispatch Trigger

- [ ] 1.1 Add `workflow_dispatch` to the `on:` section in `.gitea/workflows/build.yml`
- [ ] 1.2 Update Docker push condition to include `workflow_dispatch` event

## 2. Add dev Tag Logic

- [ ] 2.1 Modify Docker tag determination step to detect `gitea.event_name == 'workflow_dispatch'`
- [ ] 2.2 Add `dev` tag to the comma-separated TAGS string when manually triggered

## 3. Verify

- [ ] 3.1 Validate the YAML syntax
- [ ] 3.2 Commit and push changes
