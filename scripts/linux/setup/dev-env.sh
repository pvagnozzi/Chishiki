#!/usr/bin/env bash
# ==============================================================================
# dev-env — Set up the developer environment on Linux
# ==============================================================================
#
# SYNOPSIS
#   ./dev-env.sh [--install] [--help]
#
# DESCRIPTION
#   Installs and configures the developer tooling for a productive
#   development experience:
#
#     • Git                — version control
#     • Visual Studio Code — code editor   (snap preferred, else apt/dnf)
#     • PowerShell 7+      — cross-platform shell (snap preferred, else MS repo)
#     • Oh My Posh         — prompt engine with the M365Princess theme
#     • MesloLGS NF        — Nerd Font for Oh My Posh icons
#
#   Oh My Posh is configured to auto-start in every detected shell profile:
#     • ~/.bashrc
#     • ~/.zshrc   (when zsh is present)
#     • PowerShell profile (when pwsh is present)
#
# OPTIONS
#   --install    Install missing tools and configure shell profiles
#   --help, -h   Show this help and exit
#
# EXAMPLES
#   ./dev-env.sh
#   ./dev-env.sh --install
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

pkg_install() {
  local pkg="$1"
  case "$PKG_MGR" in
    apt)    sudo apt-get install -y "$pkg" ;;
    dnf)    sudo dnf install -y "$pkg" ;;
    pacman) sudo pacman -S --noconfirm "$pkg" ;;
    *)      fail "Unsupported package manager — install $pkg manually"; return 1 ;;
  esac
}

# Appends Oh My Posh init to a shell profile (idempotent)
configure_omp_profile() {
  local profile="$1"
  local init_line="$2"
  local label="$3"

  if grep -qF 'oh-my-posh' "$profile" 2>/dev/null; then
    ok "Already configured: $label"; return
  fi

  mkdir -p "$(dirname "$profile")"
  printf '\n# Oh My Posh — M365Princess theme\n%s\n' "$init_line" >> "$profile"
  ok "Configured: $label"
}

PKG_MGR=$(detect_pkgmgr)
OMP_THEMES="${POSH_THEMES_PATH:-$HOME/.cache/oh-my-posh/themes}"
OMP_INSTALLED=0
declare -a ISSUES=()

# ── Header ────────────────────────────────────────────────────────────────────
header '🛠️  Developer Environment Setup'
info "Mode: $([ "$OPT_INSTALL" = "1" ] && printf "check + install" || printf "check only (use --install to fix)")"
info "Package manager: $PKG_MGR"

# ── 1/5  Git ──────────────────────────────────────────────────────────────────
step '1/5  Git'
if command -v git &>/dev/null; then
  ok "Git — $(git --version)"
elif [[ "$OPT_INSTALL" == "1" ]]; then
  info 'Installing Git...'; pkg_install git
  ok "Git — $(git --version)"
else
  warn 'Git not found'; ISSUES+=('Git not installed')
fi

# ── 2/5  Visual Studio Code ───────────────────────────────────────────────────
step '2/5  Visual Studio Code'
if command -v code &>/dev/null; then
  ok "VS Code — $(code --version | head -1)"
elif [[ "$OPT_INSTALL" == "1" ]]; then
  if command -v snap &>/dev/null; then
    info 'Installing VS Code via snap...'
    sudo snap install code --classic
    ok 'VS Code installed'
  elif [[ "$PKG_MGR" == "apt" ]]; then
    info 'Adding Microsoft repository and installing VS Code...'
    wget -qO- https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > /tmp/ms.gpg
    sudo install -D -o root -g root -m 644 /tmp/ms.gpg /etc/apt/keyrings/packages.microsoft.gpg
    printf 'deb [arch=%s signed-by=/etc/apt/keyrings/packages.microsoft.gpg] https://packages.microsoft.com/repos/code stable main\n' \
      "$(dpkg --print-architecture)" | sudo tee /etc/apt/sources.list.d/vscode.list > /dev/null
    sudo apt-get update -y && sudo apt-get install -y code
    rm -f /tmp/ms.gpg; ok 'VS Code installed'
  elif [[ "$PKG_MGR" == "dnf" ]]; then
    sudo rpm --import https://packages.microsoft.com/keys/microsoft.asc
    sudo dnf config-manager --add-repo https://packages.microsoft.com/yumrepos/vscode
    sudo dnf install -y code; ok 'VS Code installed'
  else
    warn 'Cannot auto-install VS Code on this distro'
    info 'Download from: https://code.visualstudio.com/download'
    ISSUES+=('VS Code not installed (download from https://code.visualstudio.com)')
  fi
else
  warn 'VS Code not found'; ISSUES+=('VS Code not installed')
fi

# ── 3/5  PowerShell ───────────────────────────────────────────────────────────
step '3/5  PowerShell 7+'
if command -v pwsh &>/dev/null; then
  ok "PowerShell — $(pwsh --version)"
elif [[ "$OPT_INSTALL" == "1" ]]; then
  if command -v snap &>/dev/null; then
    info 'Installing PowerShell via snap...'
    sudo snap install powershell --classic
    ok 'PowerShell installed'
  elif [[ "$PKG_MGR" == "apt" ]]; then
    info 'Adding Microsoft repository and installing PowerShell...'
    os_ver=$(. /etc/os-release && printf '%s' "${VERSION_ID:-22.04}")
    wget -q "https://packages.microsoft.com/config/ubuntu/${os_ver}/packages-microsoft-prod.deb" -O /tmp/pms.deb
    sudo dpkg -i /tmp/pms.deb; rm -f /tmp/pms.deb
    sudo apt-get update -y && sudo apt-get install -y powershell
    ok 'PowerShell installed'
  elif [[ "$PKG_MGR" == "dnf" ]]; then
    curl -sSL https://packages.microsoft.com/config/rhel/9/prod.repo | sudo tee /etc/yum.repos.d/microsoft.repo > /dev/null
    sudo dnf install -y powershell; ok 'PowerShell installed'
  else
    warn 'Cannot auto-install PowerShell on this distro'
    info 'See: https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-linux'
    ISSUES+=('PowerShell not installed (see MS docs)')
  fi
else
  warn 'PowerShell (pwsh) not found'; ISSUES+=('PowerShell not installed')
fi

# ── 4/5  Oh My Posh ───────────────────────────────────────────────────────────
step '4/5  Oh My Posh'
if command -v oh-my-posh &>/dev/null; then
  ok "Oh My Posh — v$(oh-my-posh --version 2>/dev/null | head -1)"
  OMP_INSTALLED=1
elif [[ "$OPT_INSTALL" == "1" ]]; then
  info 'Installing Oh My Posh (official installer)...'
  curl -s https://ohmyposh.dev/install.sh | bash -s
  export PATH="$PATH:$HOME/.local/bin"
  OMP_THEMES="${POSH_THEMES_PATH:-$HOME/.cache/oh-my-posh/themes}"
  ok "Oh My Posh — v$(oh-my-posh --version 2>/dev/null | head -1)"
  OMP_INSTALLED=1
else
  warn 'Oh My Posh not found'; ISSUES+=('Oh My Posh not installed')
fi

if [[ "$OMP_INSTALLED" == "1" && "$OPT_INSTALL" == "1" ]]; then
  # Nerd Font
  info 'Installing MesloLGS NF (Nerd Font)...'
  oh-my-posh font install meslo 2>/dev/null && ok 'MesloLGS NF installed' || \
    warn 'Font install failed — run manually: oh-my-posh font install meslo'

  # Shell profiles
  info 'Configuring shell profiles...'
  OMP_BASH="eval \"\$(oh-my-posh init bash --config \"\${POSH_THEMES_PATH:-\$HOME/.cache/oh-my-posh/themes}/M365Princess.omp.json\")\""
  OMP_ZSH="eval \"\$(oh-my-posh init zsh  --config \"\${POSH_THEMES_PATH:-\$HOME/.cache/oh-my-posh/themes}/M365Princess.omp.json\")\""
  OMP_PWSH="oh-my-posh init pwsh --config \"\$HOME/.cache/oh-my-posh/themes/M365Princess.omp.json\" | Invoke-Expression"

  configure_omp_profile "$HOME/.bashrc" "$OMP_BASH" '~/.bashrc'

  if command -v zsh &>/dev/null || [[ -f "$HOME/.zshrc" ]]; then
    configure_omp_profile "$HOME/.zshrc" "$OMP_ZSH" '~/.zshrc'
  fi

  if command -v pwsh &>/dev/null; then
    PWSH_PROFILE="$HOME/.config/powershell/Microsoft.PowerShell_profile.ps1"
    configure_omp_profile "$PWSH_PROFILE" "$OMP_PWSH" 'PowerShell profile'
  fi
fi

if [[ "$OMP_INSTALLED" == "1" ]]; then
  info "Select 'MesloLGS NF' (or another Nerd Font) in your terminal emulator settings"
fi

# ── 5/5  Summary ──────────────────────────────────────────────────────────────
step 'Summary'
if [[ ${#ISSUES[@]} -eq 0 ]]; then
  ok 'All developer tools are installed'
  if [[ "$OMP_INSTALLED" == "1" && "$OPT_INSTALL" == "1" ]]; then
    info "M365Princess theme active — run 'source ~/.bashrc' or restart terminal"
  fi
else
  warn "${#ISSUES[@]} tool(s) missing:"
  for i in "${ISSUES[@]}"; do info "  $i"; done
  info "Re-run with ${YELLOW}--install${R} to install missing tools"
  echo ""; exit 1
fi
echo ""
