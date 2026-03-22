#Requires -Version 7.0
<#
.SYNOPSIS
    ✅ Install package managers (Chocolatey, Scoop) and .NET SDK on Windows.

.DESCRIPTION
    Verifies and installs the essential package managers and .NET runtime that
    the rest of the setup scripts depend on:

      • winget        — Windows Package Manager (check only)
      • Chocolatey    — community package manager  (choco)
      • Scoop         — developer-focused package manager
      • .NET SDK 10+  — application runtime

    Docker, Git, Node.js, and uv are managed by dedicated scripts:
      dev-env.ps1            — Git, VS Code, PowerShell, Oh My Posh
      container-prereqs.ps1  — Hyper-V, WSL2, Ubuntu
      container-runtime.ps1  — Docker / Podman
      mcp-setup.ps1          — Node.js, uv, MCP server packages

.PARAMETER Install
    Auto-install any missing required tools via winget.

.PARAMETER Help
    Show this help message and exit.

.EXAMPLE
    .\install-prereqs.ps1
    .\install-prereqs.ps1 -Install

.NOTES
    Requires: PowerShell 7+, Windows 10/11
    Idempotent: running multiple times produces the same result.
#>
[CmdletBinding(SupportsShouldProcess)]
param(
    [switch]$Install,
    [switch]$Help
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ── ANSI Colors ──────────────────────────────────────────────────────────────
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

function Install-Chocolatey {
    if ($PSCmdlet.ShouldProcess('Chocolatey', 'install via official script')) {
        Write-Info 'Installing Chocolatey...'
        Set-ExecutionPolicy Bypass -Scope Process -Force
        [System.Net.ServicePointManager]::SecurityProtocol =
            [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
        Invoke-Expression (
            (New-Object System.Net.WebClient).DownloadString(
                'https://community.chocolatey.org/install.ps1'))
        $env:PATH = $env:PATH + ";$env:ALLUSERSPROFILE\chocolatey\bin"
        Write-Ok 'Chocolatey installed'
    }
}

function Install-Scoop {
    if ($PSCmdlet.ShouldProcess('Scoop', 'install via get.scoop.sh')) {
        Write-Info 'Installing Scoop...'
        Invoke-RestMethod get.scoop.sh | Invoke-Expression
        $env:PATH = $env:PATH + ";$env:USERPROFILE\scoop\shims"
        Write-Ok 'Scoop installed'
    }
}

# ── Main ──────────────────────────────────────────────────────────────────────
Write-Header '🔧 Prerequisites Check'
Write-Host "  Platform: $B$WHITE Windows (PowerShell $($PSVersionTable.PSVersion))$R"
Write-Info "Mode: $(if ($Install) { "${YELLOW}${B}check + install${R}" } else { "check only ${CYAN}(use -Install to fix)${R}" })"

$issues = [System.Collections.Generic.List[string]]::new()

# ── 1/4  winget ───────────────────────────────────────────────────────────────
Write-Step '1/4  winget'
if (Get-Command winget -ErrorAction SilentlyContinue) {
    $v = (winget --version 2>&1).ToString().Trim()
    Write-Ok "winget — $v"
} else {
    Write-Warn 'winget not found — install App Installer from the Microsoft Store'
    $issues.Add('winget not found')
}

# ── 2/4  Chocolatey ───────────────────────────────────────────────────────────
Write-Step '2/4  Chocolatey'
$v = Get-ToolVersion 'choco'
if ($v) { Write-Ok "Chocolatey — $v" }
elseif ($Install) { Install-Chocolatey }
else { Write-Warn 'Chocolatey not found'; $issues.Add('Chocolatey not installed  (re-run with -Install)') }

# ── 3/4  Scoop ────────────────────────────────────────────────────────────────
Write-Step '3/4  Scoop'
$v = Get-ToolVersion 'scoop'
if ($v) { Write-Ok "Scoop — $v" }
elseif ($Install) { Install-Scoop }
else { Write-Warn 'Scoop not found'; $issues.Add('Scoop not installed  (re-run with -Install)') }

# ── 4/4  .NET SDK 10+ ─────────────────────────────────────────────────────────
Write-Step '4/4  .NET SDK 10+'
$v = Get-ToolVersion 'dotnet'
if ($v) {
    $verStr = [regex]::Match($v, '\d+\.\d+[\d.]*').Value
    if ([Version]$verStr -ge [Version]'10.0') { Write-Ok ".NET SDK — $v" }
    else { Write-Warn ".NET SDK found $v — required 10.0+"; $issues.Add('.NET SDK outdated') }
} elseif ($Install) {
    if (Get-Command winget -ErrorAction SilentlyContinue) {
        Write-Info 'Installing .NET SDK 10 via winget...'
        winget install --id Microsoft.DotNet.SDK.10 --silent --accept-source-agreements --accept-package-agreements
        $env:PATH = [System.Environment]::GetEnvironmentVariable('PATH', 'Machine') + ';' +
                    [System.Environment]::GetEnvironmentVariable('PATH', 'User')
        Write-Ok '.NET SDK 10 installed'
    } else {
        Write-Fail '.NET SDK not found and winget unavailable — install from https://dot.net'
        exit 1
    }
} else {
    Write-Fail '.NET SDK not found (required)'; $issues.Add('.NET SDK 10+ not installed')
}

# ── Summary ───────────────────────────────────────────────────────────────────
Write-Step 'Summary'
if ($issues.Count -eq 0) {
    Write-Host ""
    Write-Ok 'All prerequisites satisfied'
    Write-Info 'Next — run the other setup scripts in order:'
    Write-Info "  ${CYAN}dev-env.ps1 -Install${R}              Git, VS Code, PowerShell, Oh My Posh"
    Write-Info "  ${CYAN}container-prereqs.ps1${R}             Hyper-V, WSL2, Ubuntu"
    Write-Info "  ${CYAN}container-runtime.ps1 -Install${R}   Docker / Podman"
    Write-Info "  ${CYAN}mcp-setup.ps1 -Install${R}            Node.js, uv, MCP server packages"
} else {
    Write-Warn "$($issues.Count) issue(s) found:"
    foreach ($i in $issues) { Write-Info "  $i" }
    Write-Info "Re-run with ${YELLOW}-Install${R} to resolve"
    Write-Host ""
    exit 1
}
Write-Host ""
