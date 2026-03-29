# Hub Seed

This directory contains seed data files used to populate the hub database at startup.

## Seed file format

Each seed file is a YAML document that lists resources to register:

```yaml
type: Agent          # resource type
resources:
  - path: .github/agents/my-agent.chatmode.md
    name: my-agent
```

## Files

| File | Purpose |
|---|---|
| `agents.seed.yaml` | Seeds all Copilot agent definitions from `.github/agents/` |
| `skills.seed.yaml` | Seeds all skill definitions from `.github/skills/` |
| `instructions.seed.yaml` | Seeds coding instruction files from `.github/instructions/` |
| `collections.seed.yaml` | Seeds collection manifests from `hub/collections/` |
