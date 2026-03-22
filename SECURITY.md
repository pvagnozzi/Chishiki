# 🔒 Security Policy

## 📋 Supported Versions

| Version | Supported |
|---|---|
| `main` | ✅ Yes |
| `develop` | ⚠️ Pre-release only |
| `< 1.0.0` | ❌ No |

---

## 🚨 Reporting a Vulnerability

**Please do NOT report security vulnerabilities via public GitHub Issues.**

### Preferred Method

Send an email to: **security@your-org.example** with:

- A clear description of the vulnerability
- Steps to reproduce (PoC if possible)
- The affected component(s)
- Potential impact
- Suggested fix (optional)

### What to Expect

| Timeframe | Action |
|---|---|
| **48 hours** | Acknowledgement of your report |
| **7 days** | Initial assessment and severity classification |
| **30 days** | Patch or mitigation published (for critical/high) |
| **90 days** | Public disclosure (coordinated) |

We follow **responsible disclosure** — we will credit reporters in the release notes unless anonymity is requested.

---

## 🛡️ Security Scope

### In-Scope

- Authentication/authorization bypass (Keycloak integration)
- Unauthorized access to Orleans grains or grain state
- SQL injection in PostgreSQL queries
- Secrets or credentials exposure via API or logs
- Dependency vulnerabilities (NuGet packages)
- Docker image vulnerabilities
- Container escape or privilege escalation

### Out-of-Scope

- Issues in third-party infrastructure (Keycloak, PostgreSQL, Redis upstream)
- Vulnerabilities requiring physical access
- Social engineering attacks
- Issues already publicly known (CVEs in tracking)

---

## 🔐 Security Best Practices for Contributors

- Never commit secrets, API keys, passwords, or connection strings
- Use environment variables for all sensitive configuration
- The `secrets-scanner` hook runs at session end and will warn on potential leaks
- All container images must be pinned to specific versions (not `latest`)
- Follow OWASP Top 10 guidelines for API development

---

## 📦 Dependency Management

Dependencies are managed via NuGet. To check for known vulnerabilities:

```powershell
dotnet list package --vulnerable --include-transitive
```

Critical and high-severity vulnerabilities in direct dependencies will block merges via CI.
