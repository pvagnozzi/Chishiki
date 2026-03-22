#!/usr/bin/env bash
# ==============================================================================
# container-runtime — Check or install a container runtime (Docker or Podman)
# ==============================================================================
#
# SYNOPSIS
#   ./container-runtime.sh [--install] [--help]
#
# DESCRIPTION
#   Detects whether Docker or Podman is installed and reports the version found.
#   If neither runtime is present and --install is specified, Podman CLI is
#   installed via the system package manager (apt / dnf / pacman) and Podman
#   Desktop is installed via Flatpak when available.
#
# OPTIONS
#   --install    Install Podman CLI (and Podman Desktop via Flatpak if available)
#                when no container runtime is detected
#   --help, -h   Show this help and exit
#
# EXAMPLES
#   ./container-runtime.sh
#   ./container-runtime.sh --install
#
# NOTES
#   Requires: bash 4+
#   Idempotent: skips installation when a runtime is already present.
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

header '🐳 Container Runtime'
info "Mode: $([ "$OPT_INSTALL" = "1" ] && printf "check + install" || printf "check only (use --install to fix)")"

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
    info 'No container runtime detected — installing Podman...'

    # Detect package manager
    if   command -v apt-get &>/dev/null; then PKG_MGR='apt'
    elif command -v dnf     &>/dev/null; then PKG_MGR='dnf'
    elif command -v pacman  &>/dev/null; then PKG_MGR='pacman'
    else
      fail 'No supported package manager found (apt / dnf / pacman)'
      exit 1
    fi
    info "Package manager: $PKG_MGR"

    # Install Podman CLI
    case "$PKG_MGR" in
      apt)
        sudo apt-get update -y
        sudo apt-get install -y podman
        ;;
      dnf)
        sudo dnf install -y podman
        ;;
      pacman)
        sudo pacman -S --noconfirm podman
        ;;
    esac
    ok 'Podman CLI installed'

    # Podman Desktop via Flatpak
    if command -v flatpak &>/dev/null; then
      info 'Installing Podman Desktop via Flatpak...'
      flatpak remote-add --if-not-exists flathub https://flathub.org/repo/flathub.flatpakrepo 2>/dev/null || true
      flatpak install -y flathub io.podman_desktop.PodmanDesktop 2>/dev/null && \
        ok 'Podman Desktop installed' || \
        warn 'Podman Desktop installation failed — install manually from https://podman-desktop.io'
    else
      warn 'Flatpak not available — Podman Desktop skipped'
      info 'Install Flatpak or download Podman Desktop from https://podman-desktop.io'
    fi
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

[[ "$DOCKER_FOUND" == "1" ]] && { ok 'Runtime: Docker ✓'; RUNTIME_NOW=1; }
[[ "$PODMAN_FOUND" == "1" ]] && { ok 'Runtime: Podman ✓'; RUNTIME_NOW=1; }

if [[ "$RUNTIME_NOW" == "0" ]] && command -v podman &>/dev/null; then
  ok 'Runtime: Podman ✓ (just installed)'
  RUNTIME_NOW=1
fi

if [[ "$RUNTIME_NOW" == "0" ]]; then
  warn 'No container runtime available'
  info "Run with --install to install Podman"
  echo ""
  exit 1
fi

echo ""
