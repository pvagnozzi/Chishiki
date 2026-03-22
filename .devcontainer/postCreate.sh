#!/usr/bin/env bash
# ==============================================================================
# postCreate.sh — DevContainer post-create setup for Chishiki
# ==============================================================================
# Runs automatically after the devcontainer is created.
# Idempotent: safe to re-run at any time.
# ==============================================================================

set -euo pipefail
IFS=$'\n\t'

CYAN='\033[36m'; GREEN='\033[32m'; YELLOW='\033[33m'; R='\033[0m'; B='\033[1m'
step() { printf "\n${B}${CYAN}  ──${R} %s\n" "$1"; }
ok()   { printf "    ${GREEN}✔${R}  %s\n" "$1"; }
info() { printf "    ${CYAN}·${R}  %s\n" "$1"; }
warn() { printf "    ${YELLOW}⚠${R}  %s\n" "$1"; }

export PATH="$HOME/.local/bin:$HOME/.dotnet/tools:$PATH"

# ── 1  Aspire workload ────────────────────────────────────────────────────────
step '1/6  Aspire workload'
if dotnet workload list 2>/dev/null | grep -q 'aspire'; then
  ok 'Aspire workload already installed'
else
  info 'Installing Aspire workload...'
  dotnet workload install aspire --skip-sign-check 2>/dev/null \
    || dotnet workload install aspire 2>/dev/null \
    || warn 'Aspire workload install failed — run manually: dotnet workload install aspire'
  ok 'Aspire workload installed'
fi

# ── 2  NuGet restore ──────────────────────────────────────────────────────────
step '2/6  NuGet restore'
info 'Restoring solution packages...'
dotnet restore /workspace/Chishiki.slnx --verbosity quiet
ok 'NuGet packages restored'

# ── 3  uv / uvx (Python runner for mcp-server-fetch) ─────────────────────────
step '3/6  uv (Python runner)'
if command -v uvx &>/dev/null; then
  ok "uvx already installed — $(uvx --version 2>&1 | head -1)"
else
  info 'Installing uv...'
  curl -LsSf https://astral.sh/uv/install.sh | bash
  export PATH="$HOME/.local/bin:$PATH"
  ok "uvx installed — $(uvx --version 2>&1 | head -1)"
fi

# ── 4  Oh My Posh + M365Princess theme ───────────────────────────────────────
step '4/6  Oh My Posh'
if command -v oh-my-posh &>/dev/null; then
  ok "Oh My Posh already installed — v$(oh-my-posh --version 2>&1 | head -1)"
else
  info 'Installing Oh My Posh...'
  curl -s https://ohmyposh.dev/install.sh | bash -s -- -d "$HOME/.local/bin"
  ok "Oh My Posh installed"
fi

OMP_INIT='eval "$(oh-my-posh init bash --config ${POSH_THEMES_PATH:-$HOME/.cache/oh-my-posh/themes}/M365Princess.omp.json)"'
if ! grep -qF 'oh-my-posh' "$HOME/.bashrc" 2>/dev/null; then
  printf '\n# Oh My Posh — M365Princess theme\n%s\n' "$OMP_INIT" >> "$HOME/.bashrc"
  ok 'Oh My Posh configured in ~/.bashrc'
else
  ok 'Oh My Posh already configured in ~/.bashrc'
fi

# ── 5  dnx (.NET NuGet package runner) ────────────────────────────────────────
step '5/6  dnx (.NET NuGet runner)'
if dotnet tool list -g 2>/dev/null | grep -qi 'dnx'; then
  ver=$(dotnet tool list -g 2>/dev/null | grep -i 'dnx' | awk '{print $2}')
  ok "dnx already installed — v${ver}"
else
  info 'Installing dnx...'
  dotnet tool install --global dnx 2>/dev/null \
    && ok 'dnx installed' \
    || warn 'dnx install failed — used by azure and nuget MCP servers'
fi

# ── 6  npm global MCP packages ───────────────────────────────────────────────
step '6/6  npm MCP packages'
declare -A MCP_PACKAGES=(
  ["@modelcontextprotocol/server-sequential-thinking"]="mcp-server-sequential-thinking"
  ["@modelcontextprotocol/server-postgres"]="mcp-server-postgres"
  ["@modelcontextprotocol/server-memory"]="mcp-server-memory"
)
for pkg in "${!MCP_PACKAGES[@]}"; do
  bin="${MCP_PACKAGES[$pkg]}"
  if command -v "$bin" &>/dev/null; then
    ok "$bin — already installed"
  else
    info "Installing $pkg..."
    npm install -g "$pkg" --silent
    ok "$bin installed"
  fi
done

# ── Git safe directory ────────────────────────────────────────────────────────
git config --global --add safe.directory /workspace 2>/dev/null || true

# ── Summary ───────────────────────────────────────────────────────────────────
printf "\n  ${GREEN}${B}✔ DevContainer ready.${R}\n\n"
info 'Infrastructure services are pre-started by docker-compose:'
info '  postgresql:5432  redis:6379  keycloak:8180  prometheus:9090'
info '  grafana:3000     qdrant:6333  ollama:11434'
printf "\n"
info 'Quick start:'
info '  Debug panel → "🌐 API Web"       — start API with debugger attached'
info '  Debug panel → "🏰 Orleans Host"  — start Orleans silo with debugger'
info '  Debug panel → "🎯 API + Host"    — start both services together'
info '  Debug panel → "🚀 Aspire AppHost" — full Aspire orchestration'
info '    ↑ stop compose infra first:  Task: "infra: stop"'
info '  Run: gh auth login              — authenticate GitHub CLI'
printf "\n"
