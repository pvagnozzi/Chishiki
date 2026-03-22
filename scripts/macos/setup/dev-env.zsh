#!/usr/bin/env zsh
# ==============================================================================
# dev-env — Set up the developer environment on macOS
# ==============================================================================
#
# SYNOPSIS
#   ./dev-env.zsh [--install] [--help]
#
# DESCRIPTION
#   Installs and configures the developer tooling for a productive
#   development experience:
#
#     • Git                — version control
#     • Visual Studio Code — code editor   (Homebrew cask)
#     • PowerShell 7+      — cross-platform shell (Homebrew cask)
#     • Oh My Posh         — prompt engine with the M365Princess theme
#     • MesloLGS NF        — Nerd Font for Oh My Posh icons
#
#   Oh My Posh is configured to auto-start in every detected shell profile:
#     • ~/.zshrc
#     • ~/.bashrc  (when bash is present)
#     • PowerShell profile (when pwsh is present)
#
# OPTIONS
#   --install    Install missing tools and configure shell profiles
#   --help, -h   Show this help and exit
#
# EXAMPLES
#   ./dev-env.zsh
#   ./dev-env.zsh --install
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
ensure_brew() {
  if command -v brew &>/dev/null; then return; fi
  info 'Installing Homebrew...'
  /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
  eval "$(/opt/homebrew/bin/brew shellenv 2>/dev/null || /usr/local/bin/brew shellenv 2>/dev/null)"
}

brew_install()      { local f="$1"; brew install "$f"        }
brew_install_cask() { local f="$1"; brew install --cask "$f" }

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

OMP_INSTALLED=0
typeset -a ISSUES=()

# ── Header ────────────────────────────────────────────────────────────────────
header '🛠️  Developer Environment Setup'
info "Mode: $([ "$OPT_INSTALL" = "1" ] && print "check + install" || print "check only (use --install to fix)")"

# ── 1/5  Git ──────────────────────────────────────────────────────────────────
step '1/5  Git'
if command -v git &>/dev/null; then
  ok "Git — $(git --version)"
elif [[ "$OPT_INSTALL" == "1" ]]; then
  ensure_brew; info 'Installing Git...'; brew_install git
  ok "Git — $(git --version)"
else
  warn 'Git not found'; ISSUES+=('Git not installed')
fi

# ── 2/5  Visual Studio Code ───────────────────────────────────────────────────
step '2/5  Visual Studio Code'
if command -v code &>/dev/null; then
  ok "VS Code — $(code --version | head -1)"
elif [[ "$OPT_INSTALL" == "1" ]]; then
  ensure_brew; info 'Installing Visual Studio Code (cask)...'; brew_install_cask visual-studio-code
  ok 'VS Code installed'
else
  warn 'VS Code not found'; ISSUES+=('VS Code not installed')
fi

# ── 3/5  PowerShell ───────────────────────────────────────────────────────────
step '3/5  PowerShell 7+'
if command -v pwsh &>/dev/null; then
  ok "PowerShell — $(pwsh --version)"
elif [[ "$OPT_INSTALL" == "1" ]]; then
  ensure_brew; info 'Installing PowerShell (cask)...'; brew_install_cask powershell
  ok 'PowerShell installed'
else
  warn 'PowerShell (pwsh) not found'; ISSUES+=('PowerShell not installed')
fi

# ── 4/5  Oh My Posh ───────────────────────────────────────────────────────────
step '4/5  Oh My Posh'
if command -v oh-my-posh &>/dev/null; then
  ok "Oh My Posh — v$(oh-my-posh --version 2>/dev/null | head -1)"
  OMP_INSTALLED=1
elif [[ "$OPT_INSTALL" == "1" ]]; then
  ensure_brew
  info 'Installing Oh My Posh via Homebrew...'
  brew install jandedobbeleer/oh-my-posh/oh-my-posh
  ok "Oh My Posh — v$(oh-my-posh --version 2>/dev/null | head -1)"
  OMP_INSTALLED=1
else
  warn 'Oh My Posh not found'; ISSUES+=('Oh My Posh not installed')
fi

if (( OMP_INSTALLED == 1 && OPT_INSTALL == 1 )); then
  # Nerd Font
  info 'Installing MesloLGS NF (Nerd Font)...'
  oh-my-posh font install meslo 2>/dev/null && ok 'MesloLGS NF installed' || \
    warn 'Font install failed — run manually: oh-my-posh font install meslo'

  # Determine Homebrew-managed themes path
  BREW_OMP_PREFIX=$(brew --prefix oh-my-posh 2>/dev/null || print '/opt/homebrew/opt/oh-my-posh')
  OMP_THEMES="${BREW_OMP_PREFIX}/themes"

  # Shell profiles
  info 'Configuring shell profiles...'
  OMP_ZSH="eval \"\$(oh-my-posh init zsh  --config '${OMP_THEMES}/M365Princess.omp.json')\""
  OMP_BASH="eval \"\$(oh-my-posh init bash --config '${OMP_THEMES}/M365Princess.omp.json')\""
  OMP_PWSH="oh-my-posh init pwsh --config '${OMP_THEMES}/M365Princess.omp.json' | Invoke-Expression"

  configure_omp_profile "$HOME/.zshrc" "$OMP_ZSH" '~/.zshrc'

  if command -v bash &>/dev/null && [[ -f "$HOME/.bashrc" ]]; then
    configure_omp_profile "$HOME/.bashrc" "$OMP_BASH" '~/.bashrc'
  fi

  if command -v pwsh &>/dev/null; then
    PWSH_PROFILE="$HOME/.config/powershell/Microsoft.PowerShell_profile.ps1"
    configure_omp_profile "$PWSH_PROFILE" "$OMP_PWSH" 'PowerShell profile'
  fi
fi

if (( OMP_INSTALLED == 1 )); then
  info "Select 'MesloLGS NF' (or another Nerd Font) in your terminal emulator settings"
fi

# ── 5/5  Summary ──────────────────────────────────────────────────────────────
step 'Summary'
if (( ${#ISSUES[@]} == 0 )); then
  ok 'All developer tools are installed'
  if (( OMP_INSTALLED == 1 && OPT_INSTALL == 1 )); then
    info "M365Princess theme active — run 'source ~/.zshrc' or restart terminal"
  fi
else
  warn "${#ISSUES[@]} tool(s) missing:"
  for i in "${ISSUES[@]}"; do info "  $i"; done
  info "Re-run with ${YELLOW}--install${R} to install missing tools"
  print ""; exit 1
fi
print ""
