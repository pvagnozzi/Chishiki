#Requires -Version 7.0
<#
.SYNOPSIS
    🐳 Check or install a container runtime (Docker or Podman) on Windows.

.DESCRIPTION
    Detects whether Docker or Podman is installed and reports the version found.
    If neither runtime is present and -Install is specified, Podman CLI and
    Podman Desktop are installed automatically via winget.

    On Windows, container-prereqs.ps1 is always invoked to verify (and
    optionally install) Hyper-V and WSL2, which are required by Podman on
    Windows as its virtualisation backend.

.PARAMETER Install
    Install Podman CLI and Podman Desktop (via winget) when no container
    runtime is detected. Also activates install mode in container-prereqs.ps1.

.PARAMETER Help
    Show this help message and exit.

.EXAMPLE
    .\container-runtime.ps1
    .\container-runtime.ps1 -Install

.NOTES
    Requires: PowerShell 7+, Windows 10/11
    Idempotent: skips installation when a runtime is already present.
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

Write-Header '🐳 Container Runtime'
Write-Info "Mode: $(if ($Install) { "${YELLOW}${B}check + install${R}" } else { "check only ${CYAN}(use -Install to fix)${R}" })"

$dockerFound = $false
$podmanFound = $false

# ── 1/2  Detect container runtime ────────────────────────────────────────────
Write-Step '1/2  Detecting container runtime...'

if (Get-Command docker -ErrorAction SilentlyContinue) {
    try {
        $ver = (docker --version 2>&1) -replace 'Docker version\s*', '' -replace ',.*', ''
        Write-Ok "Docker found — v$ver"
        $dockerFound = $true
    } catch {
        Write-Warn "Docker CLI present but not responding: $($_.Exception.Message)"
    }
} else {
    Write-Warn 'Docker not found'
}

if (Get-Command podman -ErrorAction SilentlyContinue) {
    try {
        $ver = (podman --version 2>&1) -replace 'podman version\s*', ''
        Write-Ok "Podman found — v$ver"
        $podmanFound = $true
    } catch {
        Write-Warn "Podman CLI present but not responding: $($_.Exception.Message)"
    }
} else {
    Write-Warn 'Podman not found'
}

if (-not $dockerFound -and -not $podmanFound) {
    if ($Install) {
        if (-not (Get-Command winget -ErrorAction SilentlyContinue)) {
            Write-Fail 'winget not found — install App Installer from the Microsoft Store first'
            exit 1
        }
        Write-Info 'No container runtime detected — installing Podman...'

        if ($PSCmdlet.ShouldProcess('Podman CLI', 'winget install')) {
            Write-Info 'Installing Podman CLI...'
            winget install --id Redhat.Podman --silent --accept-source-agreements --accept-package-agreements
            Write-Ok 'Podman CLI installed'
        }

        if ($PSCmdlet.ShouldProcess('Podman Desktop', 'winget install')) {
            Write-Info 'Installing Podman Desktop...'
            winget install --id RedHat.PodmanDesktop --silent --accept-source-agreements --accept-package-agreements
            Write-Ok 'Podman Desktop installed'
        }
    } else {
        Write-Warn 'No container runtime found'
        Write-Info "Run with ${YELLOW}-Install${R} to install Podman CLI + Podman Desktop"
    }
} else {
    Write-Ok 'Container runtime already present — installation skipped'
}

# ── 2/2  Windows prerequisites (Hyper-V + WSL2) ──────────────────────────────
Write-Step '2/2  Windows container prerequisites (Hyper-V + WSL2)...'
$prereqScript = Join-Path $PSScriptRoot 'container-prereqs.ps1'
if (Test-Path $prereqScript) {
    $prereqArgs = if ($Install) { @('-Install') } else { @() }
    Write-Info "Invoking container-prereqs.ps1$(if ($prereqArgs) { ' -Install' })"
    Write-Host ""
    & $prereqScript @prereqArgs
} else {
    Write-Warn "container-prereqs.ps1 not found at: $prereqScript"
    Write-Info 'Run scripts\windows\setup\container-prereqs.ps1 to check Hyper-V and WSL2'
}

# ── Summary ───────────────────────────────────────────────────────────────────
Write-Step 'Summary'
$runtimeNow = $dockerFound -or $podmanFound -or
              ($Install -and (Get-Command podman -ErrorAction SilentlyContinue))

if ($runtimeNow) {
    if   ($dockerFound) { Write-Ok 'Runtime: Docker ✓' }
    else                { Write-Ok 'Runtime: Podman ✓' }
    Write-Ok 'Windows prerequisites: checked via container-prereqs.ps1'
} else {
    Write-Warn 'No container runtime available'
    Write-Info "Run with ${YELLOW}-Install${R} to install Podman CLI + Podman Desktop"
    Write-Host ""
    exit 1
}

Write-Host ""
