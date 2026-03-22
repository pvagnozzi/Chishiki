#!/usr/bin/env zsh
# ==============================================================================
# setup-devenv — Full developer environment setup on macOS
# ==============================================================================
#
# SYNOPSIS
#   ./setup-devenv.zsh [--install] [--help]
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
#   ./setup-devenv.zsh --install    # full install
#   ./setup-devenv.zsh              # check-only
#
# NOTES
#   Idempotent: safe to re-run — each sub-script skips steps already done.
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
phase()  { print "\n${B}${BLUE}━━━━  ${WHITE}$1${BLUE}  ━━━━${R}"; }
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

# ── Helper: run a sub-script and track failures ───────────────────────────────
FAILED=()

run_script() {
  local label="$1" script="$2"
  phase "$label"
  local -a flags=()
  (( OPT_INSTALL )) && flags+=('--install')
  if zsh "$SCRIPT_DIR/$script" "${flags[@]+"${flags[@]}"}"; then
    ok "$script completed"
  else
    warn "$script exited with errors"
    FAILED+=("$label")
  fi
}

# ── Main ──────────────────────────────────────────────────────────────────────
header '🚀 Dev Environment Setup'
printf "  Platform: ${B}${WHITE}macOS %s — zsh %s${R}\n" "$(sw_vers -productVersion)" "$ZSH_VERSION"
info "Mode: $(if (( OPT_INSTALL )); then echo "install"; else echo "check only (pass --install to install)"; fi)"

run_script '1/5  Prerequisites'       'install-prereqs.zsh'
run_script '2/5  Container runtime'   'container-runtime.zsh'
run_script '3/5  Developer tools'     'dev-env.zsh'
run_script '4/5  MCP servers'         'mcp-setup.zsh'
run_script '5/5  IDE'                 'install-ide.zsh'

# ── Summary ───────────────────────────────────────────────────────────────────
step 'Summary'
print ""
if (( ${#FAILED[@]} == 0 )); then
  ok 'All setup steps completed successfully'
  info 'Start the full stack:'
  info '  zsh scripts/macos/dev/start.zsh'
else
  warn "${#FAILED[@]} step(s) had issues:"
  for f in "${FAILED[@]}"; do info "  $f"; done
  info 'Re-run with --install to retry failed steps'
  print ""
  exit 1
fi
print ""
