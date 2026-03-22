# 🤝 Contributing to Chishiki

Thank you for your interest in contributing to **Chishiki**! This document outlines the process for contributing code, documentation, and bug reports.

---

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Branch Strategy](#branch-strategy)
- [Commit Convention](#commit-convention)
- [Pull Request Process](#pull-request-process)
- [Coding Standards](#coding-standards)
- [Testing](#testing)

---

## 📜 Code of Conduct

By participating in this project, you agree to uphold our [Code of Conduct](CODE_OF_CONDUCT.md). Please read it before contributing.

---

## 🚀 Getting Started

1. **Fork** the repository on GitHub
2. **Clone** your fork locally:
   ```bash
   git clone https://github.com/your-username/chishiki.git
   cd chishiki
   ```
3. **Add upstream** remote:
   ```bash
   git remote add upstream https://github.com/your-org/chishiki.git
   ```
4. **Start the stack** via Aspire:
   ```powershell
   dotnet run --project .\src\infrastructure\aspire\Chishiki.Infrastructure.Aspire.AppHost\Chishiki.Infrastructure.Aspire.AppHost.csproj
   ```

---

## 🛠️ Development Setup

### Requirements

| Tool | Version |
|---|---|
| .NET SDK | 10.0+ |
| Docker Desktop | 4.x+ (Linux containers) |
| Git | 2.x+ |

### Build

```powershell
dotnet build .\Chishiki.slnx
```

### Code Style

This project uses `.editorconfig` and Roslyn analyzers. Your IDE (Visual Studio, Rider) will enforce the rules automatically.

---

## 🌿 Branch Strategy

We follow **Git Flow**:

| Branch | Purpose |
|---|---|
| `main` | Production-ready code |
| `develop` | Integration branch |
| `feat/<name>` | New features |
| `fix/<name>` | Bug fixes |
| `chore/<name>` | Maintenance tasks |
| `docs/<name>` | Documentation changes |
| `release/<version>` | Release preparation |

---

## 📝 Commit Convention

We use **Conventional Commits**:

```
<type>(<scope>): <short summary>

[optional body]

[optional footer]
```

### Types

| Type | Description |
|---|---|
| `feat` | New feature |
| `fix` | Bug fix |
| `docs` | Documentation only |
| `style` | Formatting (no logic change) |
| `refactor` | Code restructuring |
| `perf` | Performance improvement |
| `test` | Adding or fixing tests |
| `chore` | Build, CI, dependencies |
| `revert` | Reverts a previous commit |

### Examples

```bash
feat(grains): add KnowledgeIndexGrain for vector embedding
fix(host): switch base image from runtime to aspnet for ServiceDefaults
docs(readme): add architecture diagram
chore(docker): fix CRLF in entrypoint.sh
```

---

## 🔄 Pull Request Process

1. Ensure your branch is up-to-date with `develop`
2. Run the full build: `dotnet build .\Chishiki.slnx`
3. Run tests: `dotnet test .\Chishiki.slnx`
4. Fill in the PR template completely
5. Request review from a maintainer
6. Address review comments
7. PRs are merged via **Squash and Merge**

### PR Checklist

- [ ] Code compiles without warnings
- [ ] Tests pass (or new tests added for new functionality)
- [ ] `.editorconfig` rules respected
- [ ] Documentation updated if needed
- [ ] `CHANGELOG.md` updated for user-facing changes
- [ ] No secrets or credentials committed

---

## 🎯 Coding Standards

- **Namespace declarations**: file-scoped (`namespace Chishiki.X;`)
- **Nullability**: all projects use `<Nullable>enable</Nullable>`
- **Implicit usings**: enabled — no need for `using System;` etc.
- **Async**: suffix async methods with `Async`, always `await` or return the `Task`
- **Orleans Grains**: implement `IGrainWithStringKey` or appropriate key interface
- **Logging**: use `ILogger<T>` — no `Console.Write`
- **Configuration**: read via `IConfiguration` / options pattern — no hardcoded strings
- **Secrets**: use environment variables — never commit secrets

---

## 🧪 Testing

```powershell
# Unit + integration tests
dotnet test .\Chishiki.slnx

# With coverage report
dotnet test .\Chishiki.slnx --collect:"XPlat Code Coverage" --results-directory ./coverage
```

Test projects live in `tests/`. When adding new functionality, please add corresponding tests.

---

## ❓ Questions?

Open a [GitHub Discussion](https://github.com/your-org/chishiki/discussions) or reach out via [Issues](https://github.com/your-org/chishiki/issues).
