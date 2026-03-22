#!/usr/bin/env zsh
# ==============================================================================
# install-prereqs — Check and install prerequisites on macOS
# ==============================================================================
#
# SYNOPSIS
#   ./install-prereqs.zsh [--install] [--help]
#
# DESCRIPTION
#   Bootstraps the two foundational dependencies that all other
#   setup scripts rely on:
#
#     • Homebrew (brew)  — macOS package manager (install backend)
#     • .NET SDK 10+     — application runtime
#
#   Docker, Git, Node.js, and uv are managed by dedicated scripts:
#     dev-env.zsh           — Git, VS Code, PowerShell, Oh My Posh
#     container-runtime.zsh — Docker / Podman
#     mcp-setup.zsh         — Node.js, uv, MCP server packages
#
# OPTIONS
#   --install    Auto-install Homebrew and .NET SDK if missing
#   --help, -h   Show this help and exit
#
# EXAMPLES
#   ./install-prereqs.zsh
#   ./install-prereqs.zsh --install
#
# NOTES
#   Idempotent: running multiple times produces the same result.
#   Requires: zsh 5.8+, macOS 12+
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

usage()  { grep '^#' "$0" | sed 's/^# \{0,2\}//' | sed -n '/^SYNOPSIS/,/^=\{10\}/p' | head -30 }
trap 'fail "Unexpected error on line $LINENO. Exit code: $?"; exit 1' ERR TERM INT

OPT_INSTALL=0
for arg in "$@"; do
  case "$arg" in
    --install) OPT_INSTALL=1 ;;
    --help|-h) usage; exit 0 ;;
    *) fail "Unknown option: $arg"; exit 1 ;;
  esac
done

# ── Version Comparison ────────────────────────────────────────────────────────
version_gte() { printf '%s\n%s' "$1" "$2" | sort -V | head -1 | grep -qx "$2" }

# ── Check Tool ────────────────────────────────────────────────────────────────
typeset -a MISSING_REQUIRED MISSING_OPTIONAL
MISSING_REQUIRED=(); MISSING_OPTIONAL=()

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
  raw_ver=$("$cmd" --version 2>&1 | grep -oE '[0-9]+\.[0-9]+[\.0-9]*' | head -1)
  if [[ -n "$min_ver" ]] && ! version_gte "$raw_ver" "$min_ver"; then
    warn "$name found v$raw_ver — required v${min_ver}+"; MISSING_REQUIRED+=("$name")
  else
    ok "$name ${CYAN}${raw_ver}${R}"
  fi
}

# ── Install via Homebrew ──────────────────────────────────────────────────────
brew_install() {
  local formula="$1" name="$2"
  if brew list "$formula" &>/dev/null; then
    info "$name already installed via brew — upgrading..."
    brew upgrade "$formula" || true
  else
    info "Installing $name via brew..."
    brew install "$formula"
  fi
  ok "$name installed"
}

ensure_brew() {
  if ! command -v brew &>/dev/null; then
    info "Installing Homebrew..."
    /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
  fi
}

# ── Main ──────────────────────────────────────────────────────────────────────
header '🔧 Prerequisites Check'
print "  Platform: ${B}${WHITE}macOS $(sw_vers -productVersion) — zsh ${ZSH_VERSION}${R}"
info "Mode: $(if [[ "$OPT_INSTALL" == "1" ]]; then print "check + install"; else print "check only (pass --install to fix)"; fi)"

# ── 1/2  Homebrew ─────────────────────────────────────────────────────────────
step '1/2  Homebrew'
if command -v brew &>/dev/null; then
  ok "Homebrew — $(brew --version 2>&1 | head -1)"
elif [[ "$OPT_INSTALL" == "1" ]]; then
  ensure_brew
  ok 'Homebrew installed'
else
  warn 'Homebrew not found'; MISSING_REQUIRED+=('Homebrew')
fi

# ── 2/2  .NET SDK 10+ ─────────────────────────────────────────────────────────
step '2/2  .NET SDK 10+'
check_tool '.NET SDK' dotnet '10.0' 1

# ── Summary ───────────────────────────────────────────────────────────────────
step 'Summary'
if [[ ${#MISSING_REQUIRED[@]} -eq 0 && ${#MISSING_OPTIONAL[@]} -eq 0 ]]; then
  print "\n  ${GREEN}${B}✔ All prerequisites satisfied.${R}\n"
  info 'Next — run the other setup scripts in order:'
  info '  dev-env.zsh --install            Git, VS Code, PowerShell, Oh My Posh'
  info '  container-runtime.zsh --install  Docker / Podman'
  info '  mcp-setup.zsh --install          Node.js, uv, MCP server packages'
  print ""
  exit 0
fi

(( ${#MISSING_REQUIRED[@]} > 0 )) && fail "Missing required: ${MISSING_REQUIRED[*]}"

if [[ "$OPT_INSTALL" == "1" ]]; then
  step '📦 Installing missing tools...'
  ensure_brew
  for tool in "${MISSING_REQUIRED[@]}"; do
    case "$tool" in
      '.NET SDK') brew_install 'dotnet' '.NET SDK' ;;
    esac
  done
  print "\n  ${YELLOW}⚠ Restart your terminal for PATH changes to take effect.${R}\n"
else
  print "\n  ${YELLOW}  Run with --install to auto-install via Homebrew.${R}\n"
  (( ${#MISSING_REQUIRED[@]} > 0 )) && exit 1
fi
