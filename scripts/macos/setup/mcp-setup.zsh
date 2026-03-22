#!/usr/bin/env zsh
# ==============================================================================
# mcp-setup — Install and verify all MCP server dependencies on macOS
# ==============================================================================
#
# SYNOPSIS
#   ./mcp-setup.zsh [--install] [--help]
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
#   --install    Install missing tools and MCP packages via Homebrew
#   --help, -h   Show this help and exit
#
# EXAMPLES
#   ./mcp-setup.zsh
#   ./mcp-setup.zsh --install
#
# NOTES
#   Requires: zsh 5.8+, macOS 12+
#   Idempotent: safe to re-run — skips steps already complete.
# ==============================================================================

emulate -L zsh
setopt errexit nounset pipefail
SCRIPT_DIR="${0:A:h}"

# ── ANSI Colors ───────────────────────────────────────────────────────────────
R=$'\e[0m';  B=$'\e[1m'
RED=$'\e[31m'; GREEN=$'\e[32m'; YELLOW=$'\e[33m'
BLUE=$'\e[34m'; CYAN=$'\e[36m'; WHITE=$'\e[37m'

header() { print "\n${B}${BLUE}╔══════════════════════════════════════════════╗${R}"
           printf  "${B}${BLUE}║  ${WHITE}%-44s${BLUE}║${R}\n" "$1"
           print   "${B}${BLUE}╚══════════════════════════════════════════════╝${R}"; }
step()   { print "\n${B}${CYAN}  ──${R} $1"; }
ok()     { print "    ${GREEN}✔${R}  $1"; }
warn()   { print "    ${YELLOW}⚠${R}  $1"; }
fail()   { print "    ${RED}✖${R}  $1"; }
info()   { print "    ${CYAN}·${R}  $1"; }

trap 'fail "Unexpected error on line $LINENO — exit code: $?"; exit 1' ERR TERM INT

OPT_INSTALL=0
for arg in "$@"; do
  case "$arg" in
    --install) OPT_INSTALL=1 ;;
    --help|-h) grep '^#' "$0" | sed 's/^# \{0,2\}//'; exit 0 ;;
    *) fail "Unknown option: $arg"; exit 1 ;;
  esac
done

# ── Helpers ───────────────────────────────────────────────────────────────────
typeset -a ISSUES
ISSUES=()

ensure_brew() {
  if command -v brew &>/dev/null; then return; fi
  info 'Installing Homebrew...'
  /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
  eval "$(/opt/homebrew/bin/brew shellenv 2>/dev/null || /usr/local/bin/brew shellenv 2>/dev/null)"
}

brew_install() {
  local formula="$1"
  if brew list "$formula" &>/dev/null; then return; fi
  brew install "$formula"
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
  info 'Mode: check + install  (backend: Homebrew)'
  ensure_brew
else
  info 'Mode: check only  (pass --install to fix)'
fi

# ── 1/5  Prerequisites ────────────────────────────────────────────────────────
step '1/5  Prerequisites'

# Node.js
if command -v node &>/dev/null; then
  ok "Node.js — $(node --version)"
elif [[ "$OPT_INSTALL" == "1" ]]; then
  info 'Installing Node.js LTS...'
  brew_install node
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
  brew_install uv
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
  brew_install gh
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
  print "\n  ${GREEN}${B}✔ All MCP server dependencies are satisfied.${R}\n"
else
  print "\n  ${YELLOW}${B}⚠  ${#ISSUES[@]} issue(s) found:${R}"
  for issue in "${ISSUES[@]}"; do info "  $issue"; done
  info 'Re-run with --install to resolve'
  print ""
  exit 1
fi
