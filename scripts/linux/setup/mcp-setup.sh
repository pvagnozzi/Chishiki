#!/usr/bin/env bash
# ==============================================================================
# mcp-setup — Install and verify all MCP server dependencies on Linux
# ==============================================================================
#
# SYNOPSIS
#   ./mcp-setup.sh [--install] [--help]
#
# DESCRIPTION
#   Installs and verifies every dependency required by the MCP servers
#   declared in .mcp.json:
#
#     • Node.js 20+ / npm / npx     — npm-based MCP servers
#     • uv / uvx                    — Python runner   (mcp-server-fetch)
#     • gh CLI                      — GitHub CLI
#     • GitHub Copilot extension    — gh extension install github/gh-copilot
#     • dnx (.NET NuGet runner)     — Azure.Mcp  and  NuGet.Mcp.Server
#     • @modelcontextprotocol/server-sequential-thinking
#     • @modelcontextprotocol/server-postgres
#     • @modelcontextprotocol/server-memory
#
#   HTTP servers (github, microsoft-learn) require no installation.
#
# OPTIONS
#   --install    Install missing tools and MCP packages
#   --help, -h   Show this help and exit
#
# EXAMPLES
#   ./mcp-setup.sh
#   ./mcp-setup.sh --install
#
# NOTES
#   Requires: bash 4+
#   Idempotent: safe to re-run — skips steps already complete.
# ==============================================================================

set -euo pipefail
IFS=$'\n\t'

# ── ANSI Colors ───────────────────────────────────────────────────────────────
R='\033[0m'; B='\033[1m'
RED='\033[31m'; GREEN='\033[32m'; YELLOW='\033[33m'
BLUE='\033[34m'; CYAN='\033[36m'; WHITE='\033[37m'

header() { printf "\n${B}${BLUE}╔══════════════════════════════════════════════╗${R}\n"
           printf   "${B}${BLUE}║  ${WHITE}%-44s${BLUE}║${R}\n" "$1"
           printf   "${B}${BLUE}╚══════════════════════════════════════════════╝${R}\n"; }
step()   { printf "\n${B}${CYAN}  ──${R} %s\n" "$1"; }
ok()     { printf "    ${GREEN}✔${R}  %s\n" "$1"; }
warn()   { printf "    ${YELLOW}⚠${R}  %s\n" "$1"; }
fail()   { printf "    ${RED}✖${R}  %s\n" "$1"; }
info()   { printf "    ${CYAN}·${R}  %s\n" "$1"; }

trap 'fail "Unexpected error on line $LINENO — exit code: $?"; exit 1' ERR

OPT_INSTALL=0
for arg in "$@"; do
  case "$arg" in
    --install) OPT_INSTALL=1 ;;
    --help|-h) grep '^#' "$0" | sed 's/^# \{0,2\}//'; exit 0 ;;
    *) fail "Unknown option: $arg"; exit 1 ;;
  esac
done

# ── Helpers ───────────────────────────────────────────────────────────────────
detect_pkgmgr() {
  if   command -v apt-get &>/dev/null; then echo apt
  elif command -v dnf     &>/dev/null; then echo dnf
  elif command -v pacman  &>/dev/null; then echo pacman
  else echo unknown
  fi
}

PKG_MGR=$(detect_pkgmgr)
ISSUES=()

pkg_install() {
  local pkg="$1"
  case "$PKG_MGR" in
    apt)    sudo apt-get install -y "$pkg" ;;
    dnf)    sudo dnf install -y "$pkg" ;;
    pacman) sudo pacman -S --noconfirm "$pkg" ;;
    *)      fail "Unsupported package manager — install $pkg manually"; return 1 ;;
  esac
}

npm_pkg_install() {
  local pkg="$1" bin="$2"
  if command -v "$bin" &>/dev/null; then
    ok "$bin — already installed"; return
  fi
  if [[ "$OPT_INSTALL" == "1" ]]; then
    info "npm install -g $pkg..."
    npm install -g "$pkg"
    ok "$bin installed"
  else
    warn "$bin not found"
    ISSUES+=("$bin not installed  (re-run with --install)")
  fi
}

dotnet_tool_install() {
  local tool_id="$1" display_name="$2"
  if dotnet tool list -g 2>/dev/null | grep -qi "$tool_id"; then
    local ver
    ver=$(dotnet tool list -g 2>/dev/null | grep -i "$tool_id" | awk '{print $2}')
    ok "$display_name — v$ver (global)"
    return
  fi
  if [[ "$OPT_INSTALL" == "1" ]]; then
    info "dotnet tool install --global $tool_id..."
    dotnet tool install --global "$tool_id"
    export PATH="$PATH:$HOME/.dotnet/tools"
    ok "$display_name installed"
  else
    warn "$display_name not found"
    ISSUES+=("$display_name not installed  (re-run with --install)")
  fi
}

# ── Main ──────────────────────────────────────────────────────────────────────
header '🤖  MCP Servers Setup'
if [[ "$OPT_INSTALL" == "1" ]]; then
  info "Mode: check + install  (pkg manager: $PKG_MGR)"
else
  info "Mode: check only  (pass --install to fix)"
fi

# ── 1/5  Prerequisites ────────────────────────────────────────────────────────
step '1/5  Prerequisites'

# Node.js
if command -v node &>/dev/null; then
  ok "Node.js — $(node --version)"
elif [[ "$OPT_INSTALL" == "1" ]]; then
  info 'Installing Node.js LTS...'
  if [[ "$PKG_MGR" == "apt" ]]; then
    curl -fsSL https://deb.nodesource.com/setup_lts.x | sudo -E bash -
    sudo apt-get install -y nodejs
  elif [[ "$PKG_MGR" == "dnf" ]]; then
    sudo dnf install -y nodejs npm
  elif [[ "$PKG_MGR" == "pacman" ]]; then
    sudo pacman -S --noconfirm nodejs npm
  else
    fail 'Cannot auto-install Node.js — install manually from https://nodejs.org'
  fi
  ok "Node.js — $(node --version)"
else
  warn 'Node.js not found — required for npm MCP packages'
  ISSUES+=('Node.js not installed')
fi

# npx
if command -v npx &>/dev/null; then
  ok "npx — $(npx --version 2>&1 | head -1)"
else
  warn 'npx not found (comes with Node.js)'; ISSUES+=('npx not found')
fi

# uv / uvx
if command -v uvx &>/dev/null; then
  ok "uvx (uv) — $(uvx --version 2>&1 | head -1)"
elif [[ "$OPT_INSTALL" == "1" ]]; then
  info 'Installing uv...'
  curl -LsSf https://astral.sh/uv/install.sh | sh
  export PATH="$HOME/.cargo/bin:$HOME/.local/bin:$PATH"
  ok "uvx (uv) — $(uvx --version 2>&1 | head -1)"
else
  warn 'uvx not found — required for mcp-server-fetch'
  ISSUES+=('uv / uvx not installed')
fi

# .NET SDK
if command -v dotnet &>/dev/null; then
  ok ".NET SDK — $(dotnet --version 2>&1 | head -1)"
else
  warn '.NET SDK not found — required for dnx'
  ISSUES+=('.NET SDK not installed')
fi

# gh CLI
if command -v gh &>/dev/null; then
  ok "gh CLI — $(gh --version 2>&1 | head -1)"
elif [[ "$OPT_INSTALL" == "1" ]]; then
  info 'Installing GitHub CLI...'
  if [[ "$PKG_MGR" == "apt" ]]; then
    curl -fsSL https://cli.github.com/packages/githubcli-archive-keyring.gpg \
      | sudo dd of=/usr/share/keyrings/githubcli-archive-keyring.gpg
    sudo chmod go+r /usr/share/keyrings/githubcli-archive-keyring.gpg
    echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/githubcli-archive-keyring.gpg] \
https://cli.github.com/packages stable main" \
      | sudo tee /etc/apt/sources.list.d/github-cli.list > /dev/null
    sudo apt-get update && sudo apt-get install -y gh
  elif [[ "$PKG_MGR" == "dnf" ]]; then
    sudo dnf install -y 'dnf-command(config-manager)'
    sudo dnf config-manager --add-repo https://cli.github.com/packages/rpm/gh-cli.repo
    sudo dnf install -y gh
  elif [[ "$PKG_MGR" == "pacman" ]]; then
    sudo pacman -S --noconfirm github-cli
  else
    fail 'Cannot auto-install gh CLI — install from https://cli.github.com'
  fi
  ok "gh CLI — $(gh --version 2>&1 | head -1)"
else
  warn 'gh CLI not found — required for GitHub Copilot extension'
  ISSUES+=('gh CLI not installed')
fi

# ── 2/5  GitHub Copilot CLI ───────────────────────────────────────────────────
step '2/5  GitHub Copilot CLI'
if command -v gh &>/dev/null; then
  if gh extension list 2>/dev/null | grep -q 'gh-copilot'; then
    ok 'GitHub Copilot extension — already installed'
  elif [[ "$OPT_INSTALL" == "1" ]]; then
    info 'Installing GitHub Copilot extension...'
    gh extension install github/gh-copilot
    ok 'GitHub Copilot extension installed'
  else
    warn 'GitHub Copilot extension not installed'
    ISSUES+=('gh-copilot extension not installed  (re-run with --install)')
  fi
else
  warn 'gh CLI not available — skipping Copilot extension'
fi

# ── 3/5  dnx — .NET NuGet package runner ─────────────────────────────────────
step '3/5  dnx (.NET NuGet runner)'
info 'Used by: azure MCP server (Azure.Mcp) and nuget MCP server (NuGet.Mcp.Server)'
if command -v dotnet &>/dev/null; then
  dotnet_tool_install 'dnx' 'dnx'
else
  warn '.NET SDK not available — skipping dnx installation'
fi

# ── 4/5  npm MCP packages ─────────────────────────────────────────────────────
step '4/5  npm MCP packages'
if command -v npm &>/dev/null; then
  info 'sequential-thinking server'
  npm_pkg_install '@modelcontextprotocol/server-sequential-thinking' 'mcp-server-sequential-thinking'
  info 'postgres server'
  npm_pkg_install '@modelcontextprotocol/server-postgres' 'mcp-server-postgres'
  info 'memory server'
  npm_pkg_install '@modelcontextprotocol/server-memory' 'mcp-server-memory'
else
  warn 'npm not found — skipping MCP npm package installation'
  ISSUES+=('npm not found — MCP npm packages not installed')
fi

# ── 5/5  Summary ──────────────────────────────────────────────────────────────
step '5/5  Summary'
info 'HTTP servers (no installation required):'
ok   '  github          — https://api.githubcopilot.com/mcp/'
ok   '  microsoft-learn — https://learn.microsoft.com/api/mcp'
info 'stdio servers:'
ok   '  azure           — dnx Azure.Mcp → azmcp server start'
ok   '  nuget           — dnx NuGet.Mcp.Server'
ok   '  fetch           — uvx mcp-server-fetch'
ok   '  sequential-thinking — mcp-server-sequential-thinking'
ok   '  postgres        — mcp-server-postgres'
ok   '  memory          — mcp-server-memory'

if [[ ${#ISSUES[@]} -eq 0 ]]; then
  printf "\n  ${GREEN}${B}✔ All MCP server dependencies are satisfied.${R}\n\n"
else
  printf "\n  ${YELLOW}${B}⚠  %d issue(s) found:${R}\n" "${#ISSUES[@]}"
  for issue in "${ISSUES[@]}"; do info "  $issue"; done
  info 'Re-run with --install to resolve'
  printf "\n"
  exit 1
fi
