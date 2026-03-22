var builder = DistributedApplication.CreateBuilder(args);

// ── PostgreSQL + pgvector ────────────────────────────────────────────────────
var postgres = builder.AddDockerfile("postgresql", "../../../../containers/postgresql")
    .WithEndpoint(port: 5432, targetPort: 5432, name: "tcp", scheme: "tcp")
    .WithEnvironment("POSTGRES_USER", "chishiki")
    .WithEnvironment("POSTGRES_PASSWORD", "chishiki")
    .WithEnvironment("POSTGRES_DB", "chishiki")
    .WithVolume("postgresql-data", "/var/lib/postgresql/data");

// ── Redis (cache + Orleans persistence) ─────────────────────────────────────
var redis = builder.AddDockerfile("redis", "../../../../containers/redis")
    .WithEndpoint(port: 6379, targetPort: 6379, name: "tcp", scheme: "tcp")
    .WithVolume("redis-data", "/data");

// ── Keycloak OIDC ────────────────────────────────────────────────────────────
var keycloak = builder.AddDockerfile("keycloak", "../../../../containers/keycloak")
    .WithHttpEndpoint(port: 8180, targetPort: 8080, name: "http")
    .WithEnvironment("KEYCLOAK_ADMIN", "admin")
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")
    .WithEnvironment("KC_DB", "postgres")
    .WithEnvironment("KC_DB_URL", "jdbc:postgresql://postgresql:5432/keycloak")
    .WithEnvironment("KC_DB_USERNAME", "chishiki")
    .WithEnvironment("KC_DB_PASSWORD", "chishiki")
    .WaitFor(postgres);

// ── Prometheus ───────────────────────────────────────────────────────────────
var prometheus = builder.AddDockerfile("prometheus", "../../../../containers/prometheus")
    .WithHttpEndpoint(port: 9090, targetPort: 9090, name: "http")
    .WithVolume("prometheus-data", "/prometheus");

// ── Grafana ──────────────────────────────────────────────────────────────────
builder.AddDockerfile("grafana", "../../../../containers/grafana")
    .WithHttpEndpoint(port: 3000, targetPort: 3000, name: "http")
    .WithEnvironment("GF_SECURITY_ADMIN_USER", "admin")
    .WithEnvironment("GF_SECURITY_ADMIN_PASSWORD", "admin")
    .WithEnvironment("GF_USERS_ALLOW_SIGN_UP", "false")
    .WithVolume("grafana-data", "/var/lib/grafana")
    .WaitFor(prometheus);

// ── Qdrant (vector search) ───────────────────────────────────────────────────
var qdrant = builder.AddDockerfile("qdrant", "../../../../containers/qdrant")
    .WithHttpEndpoint(port: 6333, targetPort: 6333, name: "http")
    .WithEndpoint(port: 6334, targetPort: 6334, name: "grpc", scheme: "grpc")
    .WithVolume("qdrant-data", "/qdrant/storage")
    .WithVolume("qdrant-snapshots", "/qdrant/snapshots");

// ── Ollama (LLM inference) ───────────────────────────────────────────────────
var ollama = builder.AddDockerfile("ollama", "../../../../containers/ollama")
    .WithHttpEndpoint(port: 11434, targetPort: 11434, name: "http")
    .WithEnvironment("OLLAMA_DEFAULT_MODEL", "nomic-embed-text")
    .WithVolume("ollama-data", "/root/.ollama");

// ── Orleans Silo Host ────────────────────────────────────────────────────────
builder.AddDockerfile("chishiki-host", "../../../../", "src/backend/Chishiki.Host/Dockerfile")
    .WithEndpoint(port: 11111, targetPort: 11111, name: "silo", scheme: "tcp")
    .WithEndpoint(port: 30000, targetPort: 30000, name: "gateway", scheme: "tcp")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("ConnectionStrings__redis", "redis:6379")
    .WaitFor(redis)
    .WaitFor(postgres);

// ── API Web ──────────────────────────────────────────────────────────────────
builder.AddDockerfile("chishiki-api-web", "../../../../", "src/backend/Chishiki.API.Web/Dockerfile")
    .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "http")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("ASPNETCORE_HTTP_PORTS", "8080")
    .WithEnvironment("ASPNETCORE_URLS", "http://+:8080")
    .WaitFor(postgres)
    .WaitFor(redis)
    .WaitFor(keycloak)
    .WaitFor(qdrant)
    .WaitFor(ollama);

builder.Build().Run();
