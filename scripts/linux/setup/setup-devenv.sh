#!/usr/bin/env bash
# ==============================================================================
# setup-devenv — Full developer environment setup on Linux
# ==============================================================================
#
# SYNOPSIS
#   ./setup-devenv.sh [--install] [--help]
#
# DESCRIPTION
#   Orchestrates the complete developer environment setup by running every
#   setup script in the correct order:
#
#     1/5  install-prereqs   — Homebrew, .NET SDK
#     2/5  container-runtime — Docker / Podman
#     3/5  dev-env           — Git, VS Code, PowerShell, Oh My Posh
#     4/5  mcp-setup         — Node.js, uv, MCP server packages
#     5/5  install-ide       — JetBrains Rider
#
#   Each script is called with --install when that flag is passed here,
#   so missing tools are installed automatically.
#
# OPTIONS
#   --install    Pass --install to every sub-script
#   --help, -h   Show this help and exit
#
# EXAMPLES
#   ./setup-devenv.sh --install    # full install
#   ./setup-devenv.sh              # check-only
#
# NOTES
#   Idempotent: safe to re-run — each sub-script skips steps already done.
#   Requires: bash 4+, curl
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
phase()  { printf "\n${B}${BLUE}━━━━  ${WHITE}%s${BLUE}  ━━━━${R}\n" "$1"; }
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

# ── Helper: run a sub-script and track failures ───────────────────────────────
FAILED=()

run_script() {
  local label="$1" script="$2"
  phase "$label"
  local flags=()
  [[ "$OPT_INSTALL" == "1" ]] && flags+=('--install')
  if bash "$SCRIPT_DIR/$script" "${flags[@]+"${flags[@]}"}"; then
    ok "$script completed"
  else
    warn "$script exited with errors"
    FAILED+=("$label")
  fi
}

# ── Main ──────────────────────────────────────────────────────────────────────
header '🚀 Dev Environment Setup'
DISTRO=$(grep '^ID=' /etc/os-release 2>/dev/null | cut -d= -f2 | tr -d '"' || echo 'linux')
printf "  Platform: ${B}${WHITE}Linux (%s) — bash %s${R}\n" "$DISTRO" "$BASH_VERSION"
info "Mode: $(if [[ "$OPT_INSTALL" == "1" ]]; then echo "install"; else echo "check only (pass --install to install)"; fi)"

run_script '1/5  Prerequisites'       'install-prereqs.sh'
run_script '2/5  Container runtime'   'container-runtime.sh'
run_script '3/5  Developer tools'     'dev-env.sh'
run_script '4/5  MCP servers'         'mcp-setup.sh'
run_script '5/5  IDE'                 'install-ide.sh'

# ── Summary ───────────────────────────────────────────────────────────────────
step 'Summary'
printf "\n"
if [[ ${#FAILED[@]} -eq 0 ]]; then
  ok 'All setup steps completed successfully'
  info 'Start the full stack:'
  info '  bash scripts/linux/dev/start.sh'
else
  warn "${#FAILED[@]} step(s) had issues:"
  for f in "${FAILED[@]}"; do info "  $f"; done
  info 'Re-run with --install to retry failed steps'
  printf "\n"
  exit 1
fi
printf "\n"
