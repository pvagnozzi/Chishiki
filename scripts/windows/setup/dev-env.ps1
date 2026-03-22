#Requires -Version 7.0
<#
.SYNOPSIS
    🛠️  Set up the developer environment on Windows.

.DESCRIPTION
    Installs and configures the developer tooling for a productive
    development experience:

      • Git                — version control
      • Visual Studio Code — code editor
      • PowerShell 7+      — cross-platform shell (updates if already installed)
      • Oh My Posh         — prompt engine with the M365Princess theme
      • MesloLGS NF        — Nerd Font required by Oh My Posh icons

    Oh My Posh is configured to auto-start in the PowerShell profile
    ($PROFILE — current user, all hosts).

.PARAMETER Install
    Install any missing tool and write Oh My Posh to the PowerShell profile.

.PARAMETER Help
    Show this help message and exit.

.EXAMPLE
    .\dev-env.ps1
    .\dev-env.ps1 -Install

.NOTES
    Requires: PowerShell 7+, Windows 10/11, winget
    Idempotent: safe to re-run — skips steps that are already complete.
#>
[CmdletBinding(SupportsShouldProcess)]
param(
    [switch]$Install,
    [switch]$Help
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ── ANSI Colors ───────────────────────────────────────────────────────────────
$E = [char]27
$R = "$E[0m";  $B = "$E[1m"
$RED    = "$E[31m"; $GREEN  = "$E[32m"; $YELLOW = "$E[33m"
$BLUE   = "$E[34m"; $CYAN   = "$E[36m"; $WHITE  = "$E[37m"

function Write-Header { param([string]$Msg)
    Write-Host "`n$B$BLUE╔══════════════════════════════════════════════╗$R"
    Write-Host "$B$BLUE║  $WHITE$Msg$BLUE$((' ' * [Math]::Max(0, 44 - $Msg.Length)))║$R"
    Write-Host "$B$BLUE╚══════════════════════════════════════════════╝$R"
}
function Write-Step  { param([string]$Msg) Write-Host "`n$B$CYAN  ──$R $Msg" }
function Write-Ok    { param([string]$Msg) Write-Host "    $GREEN✔$R  $Msg" }
function Write-Warn  { param([string]$Msg) Write-Host "    $YELLOW⚠$R  $Msg" }
function Write-Fail  { param([string]$Msg) Write-Host "    $RED✖$R  $Msg" }
function Write-Info  { param([string]$Msg) Write-Host "    $CYAN·$R  $Msg" }

if ($Help) { Get-Help $PSCommandPath -Detailed; exit 0 }

# ── Helpers ───────────────────────────────────────────────────────────────────
function Get-ToolVersion { param([string]$Cmd, [string]$Flag = '--version')
    if (Get-Command $Cmd -ErrorAction SilentlyContinue) {
        try { return ((& $Cmd $Flag 2>&1 | Select-Object -First 1).ToString().Trim()) }
        catch { return '(version unknown)' }
    }
    return $null
}

function Install-Pkg { param([string]$Id, [string]$Name)
    if (-not (Get-Command winget -ErrorAction SilentlyContinue)) {
        Write-Fail 'winget not found — install App Installer from the Microsoft Store'
        exit 1
    }
    if ($PSCmdlet.ShouldProcess($Name, "winget install $Id")) {
        Write-Info "Installing $Name..."
        winget install --id $Id --silent --accept-source-agreements --accept-package-agreements
        Write-Ok "$Name installed"
    }
}

# Appends the Oh My Posh init block to a PowerShell profile (idempotent)
function Set-OmpProfile { param([string]$ProfilePath)
    if (Test-Path $ProfilePath) {
        $existing = Get-Content $ProfilePath -Raw -ErrorAction SilentlyContinue
        if ($existing -match 'oh-my-posh') {
            Write-Ok "Already configured: $(Split-Path $ProfilePath -Leaf)"
            return
        }
    }
    $dir = Split-Path $ProfilePath
    if ($dir -and -not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    @"

# Oh My Posh — M365Princess theme
oh-my-posh init pwsh --config "`$env:POSH_THEMES_PATH/M365Princess.omp.json" | Invoke-Expression
"@ | Add-Content -Path $ProfilePath
    Write-Ok "Configured: $ProfilePath"
}

# ── Main ──────────────────────────────────────────────────────────────────────
Write-Header '🛠️  Developer Environment Setup'
Write-Info "Mode: $(if ($Install) { "${YELLOW}${B}check + install${R}" } else { "check only ${CYAN}(use -Install to fix)${R}" })"

$issues = [System.Collections.Generic.List[string]]::new()
$ompInstalled = $false

# ── 1/5  Git ──────────────────────────────────────────────────────────────────
Write-Step '1/5  Git'
$v = Get-ToolVersion 'git' '--version'
if ($v) { Write-Ok "Git — $v" }
elseif ($Install) { Install-Pkg 'Git.Git' 'Git' }
else { Write-Warn 'Git not found'; $issues.Add('Git not installed') }

# ── 2/5  Visual Studio Code ───────────────────────────────────────────────────
Write-Step '2/5  Visual Studio Code'
$v = Get-ToolVersion 'code' '--version'
if ($v) { Write-Ok "VS Code — $($v -split "`n" | Select-Object -First 1)" }
elseif ($Install) { Install-Pkg 'Microsoft.VisualStudioCode' 'Visual Studio Code' }
else { Write-Warn 'VS Code not found'; $issues.Add('VS Code not installed') }

# ── 3/5  PowerShell ───────────────────────────────────────────────────────────
Write-Step '3/5  PowerShell 7+'
$v = Get-ToolVersion 'pwsh' '--version'
if ($v) { Write-Ok "PowerShell — $v" }
elseif ($Install) { Install-Pkg 'Microsoft.PowerShell' 'PowerShell' }
else { Write-Warn 'PowerShell (pwsh) not found in PATH'; $issues.Add('PowerShell not found') }

# ── 4/5  Oh My Posh ───────────────────────────────────────────────────────────
Write-Step '4/5  Oh My Posh'
$v = Get-ToolVersion 'oh-my-posh' '--version'
if ($v) {
    Write-Ok "Oh My Posh — v$v"
    $ompInstalled = $true
} elseif ($Install) {
    Install-Pkg 'JanDeDobbeleer.OhMyPosh' 'Oh My Posh'
    # Refresh PATH so oh-my-posh is available in this session
    $env:PATH = [System.Environment]::GetEnvironmentVariable('PATH', 'Machine') + ';' +
                [System.Environment]::GetEnvironmentVariable('PATH', 'User')
    $ompInstalled = (Get-Command oh-my-posh -ErrorAction SilentlyContinue) -ne $null
} else {
    Write-Warn 'Oh My Posh not found'
    $issues.Add('Oh My Posh not installed')
}

# ── 4a  Nerd Font ─────────────────────────────────────────────────────────────
if ($ompInstalled -and $Install) {
    Write-Info 'Installing MesloLGS NF (Nerd Font)...'
    try { oh-my-posh font install meslo; Write-Ok 'MesloLGS NF installed' }
    catch { Write-Warn "Font install failed — run manually: oh-my-posh font install meslo" }
}
if ($ompInstalled) {
    Write-Info "Select 'MesloLGS NF' (or another Nerd Font) in your terminal emulator settings"
}

# ── 4b  Configure shell profiles ─────────────────────────────────────────────
if ($ompInstalled -and $Install) {
    Write-Info 'Writing Oh My Posh init to PowerShell profile...'
    Set-OmpProfile $PROFILE
}

# ── 5/5  Summary ──────────────────────────────────────────────────────────────
Write-Step 'Summary'
if ($issues.Count -eq 0) {
    Write-Ok 'All developer tools are installed'
    if ($ompInstalled -and $Install) {
        Write-Info "M365Princess theme active — restart your terminal to see the new prompt"
    }
} else {
    Write-Warn "$($issues.Count) tool(s) missing:"
    foreach ($i in $issues) { Write-Info "  $i" }
    Write-Info "Re-run with ${YELLOW}-Install${R} to install missing tools"
    Write-Host ""
    exit 1
}
Write-Host ""
