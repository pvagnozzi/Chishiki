#!/usr/bin/env bash
# ==============================================================================
# install-ide — Install JetBrains Rider on Linux
# ==============================================================================
#
# SYNOPSIS
#   ./install-ide.sh [--install] [--help]
#
# DESCRIPTION
#   Checks for (and optionally installs) JetBrains Rider for
#   development on Linux.
#
#   Installation is attempted in this order:
#     1. snap    — sudo snap install rider --classic
#     2. flatpak — flatpak install flathub com.jetbrains.Rider
#     3. JetBrains Toolbox — downloaded to ~/.local/bin
#
#   Run install-prereqs.sh first to ensure Homebrew and .NET SDK are present.
#
# OPTIONS
#   --install    Install Rider if not found
#   --help, -h   Show this help and exit
#
# EXAMPLES
#   ./install-ide.sh
#   ./install-ide.sh --install
#
# NOTES
#   Idempotent: running multiple times produces the same result.
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

# ── Detect Rider ──────────────────────────────────────────────────────────────
find_rider() {
  if command -v rider &>/dev/null; then command -v rider; return 0; fi
  if [[ -x /snap/bin/rider ]]; then echo '/snap/bin/rider'; return 0; fi
  if flatpak info com.jetbrains.Rider &>/dev/null; then
    echo 'flatpak:com.jetbrains.Rider'; return 0
  fi
  local tb
  tb=$(find "$HOME/.local/share/JetBrains/Toolbox/apps/Rider" \
       -name 'rider' -type f 2>/dev/null | head -1 || true)
  if [[ -n "$tb" ]]; then echo "$tb"; return 0; fi
  return 1
}

# ── Install helpers ───────────────────────────────────────────────────────────
install_via_snap() {
  info 'Installing JetBrains Rider via snap...'
  sudo snap install rider --classic
  ok 'Rider installed via snap'
}

install_via_flatpak() {
  info 'Installing JetBrains Rider via flatpak...'
  flatpak install -y flathub com.jetbrains.Rider
  ok 'Rider installed via flatpak'
}

install_via_toolbox() {
  info 'Downloading JetBrains Toolbox (universal installer)...'
  local api_url='https://data.services.jetbrains.com/products/releases?code=TBA&latest=true&type=release'
  local dl_url
  dl_url=$(curl -fsSL "$api_url" | \
           grep -oP '"linux":\s*\{[^}]+?"link":\s*"\K[^"]+' | head -1 || true)
  if [[ -z "$dl_url" ]]; then
    fail 'Could not determine JetBrains Toolbox download URL'
    info 'Download Toolbox manually from: https://www.jetbrains.com/toolbox-app/'
    exit 1
  fi
  local tmp_dir
  tmp_dir=$(mktemp -d)
  curl -fsSL "$dl_url" -o "$tmp_dir/toolbox.tar.gz"
  tar -xzf "$tmp_dir/toolbox.tar.gz" -C "$tmp_dir"
  local toolbox_bin
  toolbox_bin=$(find "$tmp_dir" -name 'jetbrains-toolbox' -type f | head -1)
  mkdir -p "$HOME/.local/bin"
  cp "$toolbox_bin" "$HOME/.local/bin/jetbrains-toolbox"
  chmod +x "$HOME/.local/bin/jetbrains-toolbox"
  rm -rf "$tmp_dir"
  ok 'JetBrains Toolbox installed to ~/.local/bin'
  info 'Open JetBrains Toolbox and install Rider from there'
}

install_rider() {
  if command -v snap &>/dev/null; then
    install_via_snap
  elif command -v flatpak &>/dev/null; then
    install_via_flatpak
  else
    install_via_toolbox
  fi
}

# ── Main ──────────────────────────────────────────────────────────────────────
header '💻 IDE Setup'
DISTRO=$(grep '^ID=' /etc/os-release 2>/dev/null | cut -d= -f2 | tr -d '"' || echo 'linux')
printf "  Platform: ${B}${WHITE}Linux (%s) — bash %s${R}\n" "$DISTRO" "$BASH_VERSION"
info "Mode: $(if [[ "$OPT_INSTALL" == "1" ]]; then echo "check + install"; else echo "check only (pass --install to fix)"; fi)"

# ── 1/1  JetBrains Rider ─────────────────────────────────────────────────────
step '1/1  JetBrains Rider'
if rider_path=$(find_rider 2>/dev/null); then
  ok "JetBrains Rider — ${CYAN}${rider_path}${R}"
elif [[ "$OPT_INSTALL" == "1" ]]; then
  install_rider
else
  warn 'JetBrains Rider not found'
  info 'Re-run with --install to install'
  printf "\n"
  exit 1
fi

# ── Summary ───────────────────────────────────────────────────────────────────
step 'Summary'
printf "\n  ${GREEN}${B}✔ IDE ready — JetBrains Rider${R}\n\n"
info 'Next — start the full stack:'
info '  bash scripts/linux/dev/start.sh'
printf "\n"
