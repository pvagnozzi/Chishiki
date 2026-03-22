#!/usr/bin/env zsh
# ==============================================================================
# container-runtime — Check or install a container runtime (Docker or Podman)
# ==============================================================================
#
# SYNOPSIS
#   ./container-runtime.zsh [--install] [--help]
#
# DESCRIPTION
#   Detects whether Docker or Podman is installed and reports the version found.
#   If neither runtime is present and --install is specified, Podman CLI is
#   installed via Homebrew and Podman Desktop is installed as a Homebrew cask.
#
# OPTIONS
#   --install    Install Podman CLI and Podman Desktop (via Homebrew) when no
#                container runtime is detected
#   --help, -h   Show this help and exit
#
# EXAMPLES
#   ./container-runtime.zsh
#   ./container-runtime.zsh --install
#
# NOTES
#   Requires: zsh 5.8+, macOS 12+
#   Idempotent: skips installation when a runtime is already present.
# ==============================================================================

emulate -L zsh
setopt errexit nounset pipefail

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

header '🐳 Container Runtime'
info "Mode: $([ "$OPT_INSTALL" = "1" ] && print "check + install" || print "check only (use --install to fix)")"

DOCKER_FOUND=0
PODMAN_FOUND=0

# ── 1/2  Detect container runtime ────────────────────────────────────────────
step '1/2  Detecting container runtime...'

if command -v docker &>/dev/null; then
  ver=$(docker --version 2>/dev/null | sed 's/Docker version //;s/,.*//')
  ok "Docker found — v${ver}"
  DOCKER_FOUND=1
else
  warn 'Docker not found'
fi

if command -v podman &>/dev/null; then
  ver=$(podman --version 2>/dev/null | sed 's/podman version //')
  ok "Podman found — v${ver}"
  PODMAN_FOUND=1
else
  warn 'Podman not found'
fi

# ── 2/2  Install Podman if no runtime found ───────────────────────────────────
step '2/2  Installation...'

if [[ "$DOCKER_FOUND" == "0" && "$PODMAN_FOUND" == "0" ]]; then
  if [[ "$OPT_INSTALL" == "1" ]]; then
    info 'No container runtime detected — installing Podman via Homebrew...'

    # Ensure Homebrew is available
    if ! command -v brew &>/dev/null; then
      info 'Installing Homebrew...'
      /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
      # Add brew to PATH for the current session
      eval "$(/opt/homebrew/bin/brew shellenv 2>/dev/null || /usr/local/bin/brew shellenv 2>/dev/null)"
    fi

    # Install Podman CLI
    info 'Installing Podman CLI...'
    brew install podman
    ok 'Podman CLI installed'

    # Install Podman Desktop (cask)
    info 'Installing Podman Desktop...'
    brew install --cask podman-desktop
    ok 'Podman Desktop installed'
  else
    warn 'No container runtime found'
    info "Run with ${YELLOW}--install${R} to install Podman CLI + Podman Desktop"
  fi
else
  ok 'Container runtime already present — installation skipped'
fi

# ── Summary ───────────────────────────────────────────────────────────────────
step 'Summary'
RUNTIME_NOW=0

[[ "$DOCKER_FOUND" == "1" ]] && { ok 'Runtime: Docker ✓'; RUNTIME_NOW=1 }
[[ "$PODMAN_FOUND" == "1" ]] && { ok 'Runtime: Podman ✓'; RUNTIME_NOW=1 }

if (( RUNTIME_NOW == 0 )) && command -v podman &>/dev/null; then
  ok 'Runtime: Podman ✓ (just installed)'
  RUNTIME_NOW=1
fi

if (( RUNTIME_NOW == 0 )); then
  warn 'No container runtime available'
  info "Run with --install to install Podman"
  print ""
  exit 1
fi

print ""
