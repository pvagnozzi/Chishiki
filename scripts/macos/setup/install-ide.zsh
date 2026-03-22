#!/usr/bin/env zsh
# ==============================================================================
# install-ide — Install JetBrains Rider on macOS
# ==============================================================================
#
# SYNOPSIS
#   ./install-ide.zsh [--install] [--help]
#
# DESCRIPTION
#   Checks for (and optionally installs) JetBrains Rider for
#   development on macOS via Homebrew cask.
#
#   Run install-prereqs.zsh first to ensure Homebrew is present.
#
# OPTIONS
#   --install    Install Rider if not found
#   --help, -h   Show this help and exit
#
# EXAMPLES
#   ./install-ide.zsh
#   ./install-ide.zsh --install
#
# NOTES
#   Idempotent: running multiple times produces the same result.
#   Requires: zsh 5.8+, macOS 12+, Homebrew
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

# ── Check Homebrew ────────────────────────────────────────────────────────────
if ! command -v brew &>/dev/null; then
  fail 'Homebrew not found — run install-prereqs.zsh --install first'
  exit 1
fi

# ── Detect Rider ──────────────────────────────────────────────────────────────
find_rider() {
  if command -v rider &>/dev/null; then command -v rider; return 0; fi
  for app_dir in /Applications ~/Applications; do
    if [[ -d "$app_dir/Rider.app" ]]; then echo "$app_dir/Rider.app"; return 0; fi
  done
  local tb="$HOME/Library/Application Support/JetBrains/Toolbox/apps/Rider"
  if [[ -d "$tb" ]]; then echo "$tb"; return 0; fi
  return 1
}

# ── Install helper ────────────────────────────────────────────────────────────
install_rider() {
  info 'Installing JetBrains Rider via Homebrew cask...'
  brew install --cask rider
  ok 'JetBrains Rider installed'
}

# ── Main ──────────────────────────────────────────────────────────────────────
header '💻 IDE Setup'
printf "  Platform: ${B}${WHITE}macOS %s — zsh %s${R}\n" "$(sw_vers -productVersion)" "$ZSH_VERSION"
info "Mode: $(if (( OPT_INSTALL )); then echo "check + install"; else echo "check only (pass --install to fix)"; fi)"

# ── 1/1  JetBrains Rider ─────────────────────────────────────────────────────
step '1/1  JetBrains Rider'
if rider_path=$(find_rider 2>/dev/null); then
  ok "JetBrains Rider — ${CYAN}${rider_path}${R}"
elif (( OPT_INSTALL )); then
  install_rider
else
  warn 'JetBrains Rider not found'
  info 'Re-run with --install to install via Homebrew'
  print ""
  exit 1
fi

# ── Summary ───────────────────────────────────────────────────────────────────
step 'Summary'
print ""
ok 'IDE ready — JetBrains Rider'
info 'Next — start the full stack:'
info '  zsh scripts/macos/dev/start.zsh'
print ""
