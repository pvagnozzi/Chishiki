#!/usr/bin/env bash
# ==============================================================================
# install-prereqs — Check and install prerequisites on Linux
# ==============================================================================
#
# SYNOPSIS
#   ./install-prereqs.sh [--install] [--help]
#
# DESCRIPTION
#   Bootstraps the two foundational dependencies that all other
#   setup scripts rely on:
#
#     • Homebrew (brew)  — cross-distro package manager
#     • .NET SDK 10+     — application runtime
#
#   Docker, Git, Node.js, and uv are managed by dedicated scripts:
#     dev-env.sh           — Git, VS Code, PowerShell, Oh My Posh
#     container-runtime.sh — Docker / Podman
#     mcp-setup.sh         — Node.js, uv, MCP server packages
#
# OPTIONS
#   --install    Auto-install Homebrew and .NET SDK if missing
#   --help, -h   Show this help and exit
#
# EXAMPLES
#   ./install-prereqs.sh
#   ./install-prereqs.sh --install
#
# NOTES
#   Idempotent: running multiple times produces the same result.
#   Requires: bash 4+
# ==============================================================================

set -euo pipefail
IFS=$'\n\t'
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

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

usage() { grep '^#' "$0" | sed 's/^# \{0,2\}//' | sed -n '/^SYNOPSIS/,/^=\{10\}/p' | head -30; }
trap 'fail "Unexpected error on line $LINENO. Exit code: $?"; exit 1' ERR

OPT_INSTALL=0
for arg in "$@"; do
  case "$arg" in
    --install) OPT_INSTALL=1 ;;
    --help|-h) usage; exit 0 ;;
    *) fail "Unknown option: $arg"; exit 1 ;;
  esac
done

# ── Detect Package Manager ────────────────────────────────────────────────────
detect_pkgmgr() {
  if   command -v apt-get &>/dev/null; then echo 'apt'
  elif command -v dnf     &>/dev/null; then echo 'dnf'
  elif command -v pacman  &>/dev/null; then echo 'pacman'
  else echo 'unknown'; fi
}

# ── Version Comparison ────────────────────────────────────────────────────────
version_gte() { printf '%s\n%s' "$1" "$2" | sort -V | head -1 | grep -qx "$2"; }

# ── Check Tool ────────────────────────────────────────────────────────────────
MISSING_REQUIRED=()
MISSING_OPTIONAL=()

check_tool() {
  local name="$1" cmd="$2" min_ver="${3:-}" required="${4:-1}"
  if ! command -v "$cmd" &>/dev/null; then
    if [[ "$required" == "1" ]]; then
      fail "$name — NOT FOUND (required)"; MISSING_REQUIRED+=("$name")
    else
      warn "$name — not found (optional)"; MISSING_OPTIONAL+=("$name")
    fi
    return
  fi
  local raw_ver
  raw_ver=$("$cmd" --version 2>&1 | grep -oP '\d+\.\d+[\.\d]*' | head -1)
  if [[ -n "$min_ver" ]] && ! version_gte "$raw_ver" "$min_ver"; then
    warn "$name found v$raw_ver — required v${min_ver}+"
    MISSING_REQUIRED+=("$name")
  else
    ok "$name ${CYAN}${raw_ver}${R}"
  fi
}

# ── Install helpers ───────────────────────────────────────────────────────────
install_brew() {
  info 'Installing Homebrew...'
  /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
  local brew_bin
  brew_bin="$(command -v /home/linuxbrew/.linuxbrew/bin/brew /usr/local/bin/brew 2>/dev/null | head -1 || true)"
  if [[ -n "$brew_bin" ]]; then
    eval "$("$brew_bin" shellenv)"
    grep -qF 'brew shellenv' "$HOME/.bashrc" 2>/dev/null || \
      printf '\neval "$(%s shellenv)"\n' "$brew_bin" >> "$HOME/.bashrc"
  fi
  ok 'Homebrew installed'
}

install_dotnet() {
  info 'Installing .NET SDK 10 via Microsoft install script...'
  curl -fsSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 10.0 --install-dir "$HOME/.dotnet"
  export PATH="$HOME/.dotnet:$PATH"
  ok '.NET SDK installed to ~/.dotnet'
}

# ── Main ──────────────────────────────────────────────────────────────────────
header '🔧 Prerequisites Check'
DISTRO=$(grep '^ID=' /etc/os-release 2>/dev/null | cut -d= -f2 | tr -d '"' || echo 'linux')
printf "  Platform: ${B}${WHITE}Linux (%s) — bash %s${R}\n" "$DISTRO" "$BASH_VERSION"
info "Mode: $(if [[ "$OPT_INSTALL" == "1" ]]; then echo "check + install"; else echo "check only (pass --install to fix)"; fi)"

# ── 1/2  Homebrew ─────────────────────────────────────────────────────────────
step '1/2  Homebrew'
if command -v brew &>/dev/null; then
  ok "Homebrew — $(brew --version 2>&1 | head -1)"
elif [[ "$OPT_INSTALL" == "1" ]]; then
  install_brew
else
  warn 'Homebrew not found'; MISSING_OPTIONAL+=('Homebrew')
fi

# ── 2/2  .NET SDK 10+ ─────────────────────────────────────────────────────────
step '2/2  .NET SDK 10+'
check_tool '.NET SDK' dotnet '10.0' 1

# ── Summary ───────────────────────────────────────────────────────────────────
step 'Summary'
if [[ ${#MISSING_REQUIRED[@]} -eq 0 && ${#MISSING_OPTIONAL[@]} -eq 0 ]]; then
  printf "\n  ${GREEN}${B}✔ All prerequisites satisfied.${R}\n\n"
  info 'Next — run the other setup scripts in order:'
  info '  dev-env.sh --install           Git, VS Code, PowerShell, Oh My Posh'
  info '  container-runtime.sh --install Docker / Podman'
  info '  mcp-setup.sh --install         Node.js, uv, MCP server packages'
  printf "\n"
  exit 0
fi

[[ ${#MISSING_REQUIRED[@]} -gt 0 ]] && fail "Missing required: ${MISSING_REQUIRED[*]}"
[[ ${#MISSING_OPTIONAL[@]} -gt 0 ]] && warn "Missing optional: ${MISSING_OPTIONAL[*]}"

if [[ "$OPT_INSTALL" == "1" ]]; then
  step '📦 Installing missing tools...'
  for tool in "${MISSING_REQUIRED[@]}"; do
    case "$tool" in
      '.NET SDK') install_dotnet ;;
    esac
  done
  printf "\n  ${YELLOW}⚠ Restart your terminal for PATH changes to take effect.${R}\n\n"
else
  printf "\n  ${YELLOW}  Run with --install to auto-install missing tools.${R}\n\n"
  [[ ${#MISSING_REQUIRED[@]} -gt 0 ]] && exit 1
fi
