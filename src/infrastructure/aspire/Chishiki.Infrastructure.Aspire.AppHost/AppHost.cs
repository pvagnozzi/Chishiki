// -----------------------------------------------------------------------------
// File:        AppHost.cs
// Author:      Piergiorgio Vagnozzi
// Description: Aspire AppHost entry point — declares and wires all Chishiki infrastructure containers and services.
// Created:     2026-04-26
// Modified:    2026-06-11
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
/// <summary>Aspire AppHost that orchestrates all Chishiki infrastructure services.</summary>
/// <remarks>
/// This application uses Aspire to declare and wire:
/// - PostgreSQL with pgvector extension for vector search.
/// - Redis for caching and Orleans persistence.
/// - Keycloak for OIDC authentication.
/// - Prometheus for metrics collection.
/// - Grafana for visualization.
/// - Qdrant for vector database (RAG pipelines).
/// - Ollama for local LLM inference (embeddings).
/// - Optional security scanners (SAST, DAST, SCA, secrets, SBOM) when CHISHIKI_SECURITY_PROFILE=true.
/// - Chishiki MCP Host server.
/// </remarks>
var builder = DistributedApplication.CreateBuilder(args);

// ── PostgreSQL + pgvector ────────────────────────────────────────────────────
var postgres = builder.AddContainer("postgresql", "pgvector/pgvector", "pg16")
    .WithEndpoint(port: 5432, targetPort: 5432, name: "tcp", scheme: "tcp")
    .WithEnvironment("POSTGRES_USER", "chishiki")
    .WithEnvironment("POSTGRES_PASSWORD", "chishiki")
    .WithEnvironment("POSTGRES_DB", "chishiki")
    .WithVolume("postgresql-data", "/var/lib/postgresql/data");

// ── Redis (cache + Orleans persistence) ─────────────────────────────────────
_ = builder.AddContainer("redis", "redis", "7.4")
    .WithEndpoint(port: 6379, targetPort: 6379, name: "tcp", scheme: "tcp")
    .WithVolume("redis-data", "/data");

// ── Keycloak OIDC ────────────────────────────────────────────────────────────
_ = builder.AddContainer("keycloak", "quay.io/keycloak/keycloak", "26.2")
    .WithHttpEndpoint(port: 8180, targetPort: 8080, name: "http")
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_USERNAME", "admin")
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_PASSWORD", "admin")
    .WithEnvironment("KC_DB", "postgres")
    .WithEnvironment("KC_DB_URL", "jdbc:postgresql://postgresql:5432/chishiki")
    .WithEnvironment("KC_DB_USERNAME", "chishiki")
    .WithEnvironment("KC_DB_PASSWORD", "chishiki")
    .WithArgs("start-dev")
    .WaitFor(postgres);

// ── Prometheus ───────────────────────────────────────────────────────────────
var prometheus = builder.AddContainer("prometheus", "prom/prometheus", "v3.4.1")
    .WithHttpEndpoint(port: 9090, targetPort: 9090, name: "http")
    .WithVolume("prometheus-data", "/prometheus");

// ── Grafana ──────────────────────────────────────────────────────────────────
builder.AddContainer("grafana", "grafana/grafana", "12.0.1")
    .WithHttpEndpoint(port: 3000, targetPort: 3000, name: "http")
    .WithEnvironment("GF_SECURITY_ADMIN_USER", "admin")
    .WithEnvironment("GF_SECURITY_ADMIN_PASSWORD", "admin")
    .WithEnvironment("GF_USERS_ALLOW_SIGN_UP", "false")
    .WithVolume("grafana-data", "/var/lib/grafana")
    .WaitFor(prometheus);

// ── Qdrant (vector search) ───────────────────────────────────────────────────
_ = builder.AddContainer("qdrant", "qdrant/qdrant", "v1.14.1")
    .WithHttpEndpoint(port: 6333, targetPort: 6333, name: "http")
    .WithEndpoint(port: 6334, targetPort: 6334, name: "grpc", scheme: "grpc")
    .WithVolume("qdrant-data", "/qdrant/storage")
    .WithVolume("qdrant-snapshots", "/qdrant/snapshots");

// ── Ollama (LLM inference) ───────────────────────────────────────────────────
_ = builder.AddContainer("ollama", "ollama/ollama", "latest")
    .WithHttpEndpoint(port: 11434, targetPort: 11434, name: "http")
    .WithEnvironment("OLLAMA_DEFAULT_MODEL", "nomic-embed-text")
    .WithVolume("ollama-data", "/root/.ollama");

// ── Security scanners (opt-in via CHISHIKI_SECURITY_PROFILE=true) ───────────
if (builder.Configuration["CHISHIKI_SECURITY_PROFILE"] == "true")
{
    // SAST: Semgrep — static analysis across all source files.
    _ = builder.AddContainer("semgrep", "semgrep/semgrep", "latest")
           .WithBindMount("../../../../", "/src", isReadOnly: true)
           .WithVolume("scan-semgrep-output", "/output");

    // SAST: SonarQube — multi-language code quality and security.
    _ = builder.AddContainer("sonarqube", "sonarqube", "community")
           .WithHttpEndpoint(port: 9000, targetPort: 9000, name: "http")
           .WithEnvironment("SONAR_JDBC_URL", "jdbc:postgresql://postgresql:5432/chishiki")
           .WithEnvironment("SONAR_JDBC_USERNAME", "chishiki")
           .WithEnvironment("SONAR_JDBC_PASSWORD", "chishiki")
           .WithVolume("sonarqube-data", "/opt/sonarqube/data")
           .WithVolume("sonarqube-logs", "/opt/sonarqube/logs")
           .WithVolume("sonarqube-extensions", "/opt/sonarqube/extensions")
           .WaitFor(postgres);

    // SCA: OWASP Dependency-Check — NuGet package CVE scanning.
    _ = builder.AddContainer("dependency-check", "owasp/dependency-check", "latest")
           .WithBindMount("../../../../", "/src", isReadOnly: true)
           .WithVolume("scan-dependency-check-output", "/output");

    // SCA + containers: Trivy — filesystem and image vulnerability scanning.
    _ = builder.AddContainer("trivy", "aquasec/trivy", "latest")
           .WithBindMount("/var/run/docker.sock", "/var/run/docker.sock", isReadOnly: true)
           .WithBindMount("../../../../", "/src", isReadOnly: true)
           .WithVolume("scan-trivy-output", "/output");

    // Secret scanning: gitleaks — detect secrets in git history and working tree.
    _ = builder.AddContainer("gitleaks", "zricethezav/gitleaks", "latest")
           .WithBindMount("../../../../", "/path", isReadOnly: true)
           .WithVolume("scan-gitleaks-output", "/output");

    // SBOM generation: Syft — produce CycloneDX SBOM from source and images.
    _ = builder.AddContainer("syft", "anchore/syft", "latest")
           .WithBindMount("/var/run/docker.sock", "/var/run/docker.sock", isReadOnly: true)
           .WithBindMount("../../../../", "/src", isReadOnly: true)
           .WithVolume("scan-syft-output", "/output");

    // SCA: Grype — vulnerability scan against Syft SBOM and filesystem.
    _ = builder.AddContainer("grype", "anchore/grype", "latest")
           .WithBindMount("/var/run/docker.sock", "/var/run/docker.sock", isReadOnly: true)
           .WithBindMount("../../../../", "/src", isReadOnly: true)
           .WithVolume("scan-grype-output", "/output");

    // Binary analysis: BinSkim — PE/ELF binary security checks on build output.
    _ = builder.AddContainer("binskim", "mcr.microsoft.com/dotnet/sdk", "10.0")
           .WithBindMount("../../../../", "/src", isReadOnly: true)
           .WithVolume("scan-binskim-output", "/output");

    // DAST: Nuclei — template-driven HTTP vulnerability probes.
    _ = builder.AddContainer("nuclei", "projectdiscovery/nuclei", "latest")
           .WithEnvironment("NUCLEI_TARGET", "http://chishiki-mcp:8080")
           .WithVolume("scan-nuclei-output", "/output");

    // DAST: SQLMap — automated SQL injection detection.
    _ = builder.AddContainer("sqlmap", "googlesky/sqlmap", "latest")
           .WithEnvironment("SQLMAP_TARGET", "http://chishiki-mcp:8080")
           .WithVolume("scan-sqlmap-output", "/output")
           .WaitFor(postgres);

    // DAST: OWASP ZAP — active web application security scanner.
    _ = builder.AddContainer("zap", "ghcr.io/zaproxy/zaproxy", "stable")
           .WithHttpEndpoint(port: 8090, targetPort: 8090, name: "http");

    // Fuzzing: ffuf — HTTP endpoint fuzzing with common wordlist.
    _ = builder.AddContainer("ffuf", "ghcr.io/ffuf/ffuf", "latest")
           .WithEnvironment("FFUF_TARGET", "http://chishiki-mcp:8080/FUZZ")
           .WithVolume("scan-ffuf-output", "/output");
}

// ── Chishiki MCP Host ────────────────────────────────────────────────────────
_ = builder.AddProject<Projects.Chishiki_MCP_Host>("chishiki-mcp");

builder.Build().Run();
