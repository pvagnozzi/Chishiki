<div align="center">

# 🧠 Chishiki — 知識

### *Distributed AI-Powered Knowledge Platform*

---

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![C#](https://img.shields.io/badge/C%23-14.0-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Orleans](https://img.shields.io/badge/Orleans-10.0-FF6B35?style=for-the-badge&logo=microsoft&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/orleans/)
[![Aspire](https://img.shields.io/badge/.NET_Aspire-9.x-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/aspire/)

[![Docker](https://img.shields.io/badge/Docker-Enabled-2496ED?style=for-the-badge&logo=docker&logoColor=white)](https://www.docker.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-pgvector-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)](https://github.com/pgvector/pgvector)
[![Redis](https://img.shields.io/badge/Redis-7.x-DC382D?style=for-the-badge&logo=redis&logoColor=white)](https://redis.io/)
[![Keycloak](https://img.shields.io/badge/Keycloak-26.x-4D4D4D?style=for-the-badge&logo=keycloak&logoColor=white)](https://www.keycloak.org/)

[![Qdrant](https://img.shields.io/badge/Qdrant-1.13-FF4A4A?style=for-the-badge&logo=qdrant&logoColor=white)](https://qdrant.tech/)
[![Ollama](https://img.shields.io/badge/Ollama-0.6.x-000000?style=for-the-badge&logo=ollama&logoColor=white)](https://ollama.com/)
[![Prometheus](https://img.shields.io/badge/Prometheus-2.54-E6522C?style=for-the-badge&logo=prometheus&logoColor=white)](https://prometheus.io/)
[![Grafana](https://img.shields.io/badge/Grafana-11.x-F46800?style=for-the-badge&logo=grafana&logoColor=white)](https://grafana.com/)

[![OpenTelemetry](https://img.shields.io/badge/OpenTelemetry-Enabled-425CC7?style=for-the-badge&logo=opentelemetry&logoColor=white)](https://opentelemetry.io/)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-Welcome-brightgreen?style=for-the-badge&logo=github&logoColor=white)](CONTRIBUTING.md)

</div>

---

## 📖 Overview

**Chishiki** (知識 — *knowledge* in Japanese) is a distributed, cloud-native AI platform for intelligent knowledge management and semantic search. Built on **.NET 10** and **Microsoft Orleans**, it combines vector search, local LLM inference, and distributed actor-based processing to deliver a scalable, observable, and secure knowledge engine.

> 🎯 **Goal:** Enable organizations to ingest, index, query, and reason over large knowledge bases using state-of-the-art embeddings, semantic search, and AI-driven retrieval — entirely on-premises or in Azure.

---

## ✨ Key Features

| Feature | Technology | Description |
|---|---|---|
| 🎭 **Distributed Actors** | Orleans 10 | Virtual actor model for grain-based knowledge processing |
| 🔍 **Semantic Search** | Qdrant + pgvector | Dual-vector store for hybrid dense/sparse retrieval |
| 🤖 **Local LLM Inference** | Ollama | On-premises embedding & generation (nomic-embed-text) |
| 🔐 **Identity & Auth** | Keycloak 26 | OIDC / OAuth 2.0 with custom realm and themes |
| ⚡ **Distributed Cache** | Redis 7 | Orleans clustering, grain state, reminders & app cache |
| 🗄️ **Knowledge Store** | PostgreSQL + pgvector | Relational + vector persistence with pg17 |
| 📊 **Full Observability** | OTel + Prometheus + Grafana | Traces, metrics, logs — out of the box |
| 🚀 **Local Orchestration** | .NET Aspire | One-command full-stack startup |

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    .NET Aspire AppHost                       │
│                (Local Orchestration Layer)                   │
└────────────────────────┬────────────────────────────────────┘
                         │
         ┌───────────────┼───────────────┐
         ▼               ▼               ▼
┌──────────────┐ ┌──────────────┐ ┌──────────────────┐
│ Chishiki MCP │ │ Shared       │ │  Infrastructure  │
│ Host         │ │ Libraries    │ │                  │
│              │ │              │ │  • PostgreSQL    │
│ /mcp ✓       │ │  • ai        │ │  • Redis         │
│ Tools ✓      │ │  • core      │ │  • Keycloak      │
│ Prompts ✓    │ │  • orleans   │ │  • Qdrant        │
│ OTel ✓       │ │  • vision    │ │  • Ollama        │
└──────┬───────┘ └──────┬───────┘ │  • Prometheus    │
       │                │          │  • Grafana       │
       └────────────────┴──────────┴──────────────────┘
```

### Service Ports

| Service | Port | Protocol |
|---|---|---|
| **Aspire Dashboard** | `15888` | HTTP |
| **MCP Host** | `5010` | HTTP |
| **PostgreSQL** | `5432` | TCP |
| **Redis** | `6379` | TCP |
| **Keycloak** | `8180` | HTTP |
| **Qdrant** | `6333` / `6334` | HTTP / gRPC |
| **Ollama** | `11434` | HTTP |
| **Prometheus** | `9090` | HTTP |
| **Grafana** | `3000` | HTTP |

---

## 🚀 Getting Started

### Prerequisites

| Tool | Version | Notes |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/10.0) | `10.0+` | Required |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | `4.x+` | Linux containers |
| [Git](https://git-scm.com/) | `2.x+` | Required |

### 1️⃣ Clone the Repository

```bash
git clone https://github.com/your-org/chishiki.git
cd chishiki
```

### 2️⃣ Start the Full Stack

```powershell
# Start everything via .NET Aspire (recommended)
dotnet run --project .\src\infrastructure\aspire\Chishiki.Infrastructure.Aspire.AppHost\Chishiki.Infrastructure.Aspire.AppHost.csproj
```

The Aspire Dashboard will be available at **http://localhost:15888** and will show real-time logs, traces, and health for the MCP host and supporting infrastructure.

### 3️⃣ Verify Services

After the AppHost starts, verify the main local entry points:

- **Aspire Dashboard** — `http://localhost:15888`
- **MCP endpoint** — `http://localhost:5010/mcp`
- **Grafana** — `http://localhost:3000`
- **Keycloak** — `http://localhost:8180`
- **Qdrant** — `http://localhost:6333`

### 4️⃣ Individual Entry Points

```powershell
# MCP host only
dotnet run --project .\src\mcp\Chishiki.MCP.Host\Chishiki.MCP.Host.csproj

# Build entire solution
dotnet build .\Chishiki.slnx

# Run all tests
dotnet test .\Chishiki.slnx
```

---

## 📁 Project Structure

```
chishiki/
├── 📂 src/
│   ├── 📂 infrastructure/
│   │   └── aspire/
│   │       ├── Chishiki.Infrastructure.Aspire.AppHost/         # Aspire orchestration root
│   │       └── Chishiki.Infrastructure.Aspire.ServiceDefaults/ # Shared OTel / health / resilience
│   ├── 📂 mcp/
│   │   └── Chishiki.MCP.Host/                                  # MCP host
│   └── 📂 shared/
│       ├── ai/                                                 # AI abstractions and Semantic Kernel integrations
│       ├── core/                                               # Core, data, mapping, and messaging libraries
│       ├── orleans/                                            # Shared Orleans-related libraries
│       └── vision/                                             # Vision abstractions and implementations
│
├── 📂 tests/                                                   # Test projects
├── 📂 containers/                                              # Docker images & configs
├── 📂 docs/                                                    # Architecture & ADRs
├── 📂 scripts/                                                 # Build and operational scripts
├── 📂 .github/                                                 # Copilot instructions and related assets
├── 📂 skills/                                                  # Repo-local Pi skills and project conventions
├── .mcp.json                                                   # MCP server configuration for local tooling
├── Chishiki.slnx                                               # Solution file (.NET 10 format)
└── README.md
```

---

## 🔧 Configuration

### Orleans Cluster

| Setting | Value |
|---|---|
| Cluster ID | `chishiki-cluster` |
| Service ID | `chishiki` |
| Silo Port | `11111` |
| Gateway Port | `30000` |
| Clustering | Redis (`ConnectionStrings__redis`) |
| Grain Storage | Redis (default + `PubSubStore`) |
| Reminders | Redis |

### Environment Variables

| Variable | Default | Description |
|---|---|---|
| `ConnectionStrings__redis` | `redis:6379` | Redis connection string |
| `ConnectionStrings__postgres` | — | PostgreSQL connection string |
| `POSTGRES_DB` | `chishiki` | Primary database name |
| `POSTGRES_USER` | `postgres` | Database user |
| `OLLAMA_DEFAULT_MODEL` | `nomic-embed-text` | Default embedding model |

---

## 📊 Observability

Chishiki ships with full observability out of the box:

- **Traces** — Distributed tracing via OpenTelemetry → Prometheus
- **Metrics** — Scraped at `/metrics` (Prometheus format) by Prometheus, visualized in Grafana
- **Logs** — Structured logging via `Microsoft.Extensions.Logging` with OTel correlation
- **Health** — `/health` (deep) and `/alive` (liveness) endpoints on all services
- **Dashboard** — Grafana at `http://localhost:3000` with pre-provisioned .NET Overview dashboard

---

## 🔐 Security

- Authentication via **Keycloak** (realm `chishiki`, client `chishiki-api`)
- HTTPS enforced in production; HTTP only in container development
- Secrets managed via environment variables — never committed to source
- See [SECURITY.md](SECURITY.md) for the vulnerability disclosure policy

---

## 🧪 Testing

```powershell
# Run all tests
dotnet test .\Chishiki.slnx

# Run with coverage (when test projects are added)
dotnet test .\Chishiki.slnx --collect:"XPlat Code Coverage"
```

> 📝 Test projects are under `tests/` — contributions welcome! See [CONTRIBUTING.md](CONTRIBUTING.md).

---

## 🤝 Contributing

Contributions, issues, and feature requests are welcome!

1. Fork the repository
2. Create your feature branch: `git checkout -b feat/amazing-feature`
3. Commit your changes: `git commit -m 'feat: add amazing feature'`
4. Push to the branch: `git push origin feat/amazing-feature`
5. Open a Pull Request

### Pi project guidance

If you are working with Pi in this repository, start from:

- `skills/chishiki-repo-conventions/SKILL.md`
- `skills/chishiki-repo-conventions/references/architecture-layout-and-build.md`
- `skills/chishiki-repo-conventions/references/csharp-conventions.md`

These files are the Pi-compatible conversion of the repository's Copilot guidance and capture the current Chishiki-specific layout, build flow, file header convention, and XML documentation expectations.

See [CONTRIBUTING.md](CONTRIBUTING.md) for detailed guidelines.

---

## 📜 Changelog

See [CHANGELOG.md](CHANGELOG.md) for a full history of changes.

---

## 📄 License

This project is licensed under the **MIT License** — see [LICENSE](LICENSE) for details.

---

<div align="center">

Made with ❤️ and ☕ · Powered by .NET 10 · Built for the cloud

</div>
