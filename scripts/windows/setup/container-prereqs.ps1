#Requires -Version 7.0
<#
.SYNOPSIS
    🐋 Verify Windows container prerequisites: Hyper-V, WSL2, and Ubuntu.

.DESCRIPTION
    Checks the status of the three prerequisites required to run Linux
    containers and WSL-based development workloads on Windows:

      • Hyper-V              — Windows hypervisor platform
      • WSL2                 — Windows Subsystem for Linux v2
      • Ubuntu (WSL distro)  — Ubuntu distribution inside WSL2

    When a component is already installed this script keeps it current:
      • WSL2   — runs 'wsl --update' to ensure the latest kernel
      • Ubuntu — runs 'apt-get update' followed by 'apt-get dist-upgrade -y'

    When -Install is specified, missing components are installed automatically.
    A system reboot may be required after first-time Hyper-V or WSL2 installation.

.PARAMETER Install
    Install any missing prerequisite instead of just reporting it.
    Requires Administrator privileges.

.PARAMETER Help
    Show this help message and exit.

.EXAMPLE
    .\container-prerequisites.ps1
    .\container-prerequisites.ps1 -Install

.NOTES
    Requires:     PowerShell 7+, Windows 10/11 (x64), Administrator privileges
    Windows-only: Hyper-V and WSL2 are Windows-exclusive features; this script
                  has no Linux or macOS equivalent.
    Idempotent:   safe to re-run — updates are applied only when already installed.
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

# ── Admin check ───────────────────────────────────────────────────────────────
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
    [Security.Principal.WindowsBuiltInRole]::Administrator)

Write-Header '🐋 Windows Container Prerequisites'
Write-Info "Mode:  $(if ($Install) { "${YELLOW}${B}check + install${R}" } else { "check only ${CYAN}(use -Install to fix)${R}" })"
Write-Info "Admin: $(if ($isAdmin) { "${GREEN}yes${R}" } else { "${YELLOW}no${R}" })"

if ($Install -and -not $isAdmin) {
    Write-Fail 'Administrator privileges are required to install components'
    Write-Info  "Re-launch PowerShell as Administrator and retry:"
    Write-Info  "  Start-Process pwsh -Verb RunAs -ArgumentList \`"-File '$PSCommandPath' -Install\`""
    exit 1
}

$rebootRequired = $false
$issues         = [System.Collections.Generic.List[string]]::new()

# ── 1. Hyper-V ────────────────────────────────────────────────────────────────
Write-Step '1/3  Hyper-V'
try {
    $feature = Get-WindowsOptionalFeature -Online -FeatureName 'Microsoft-Hyper-V-All' -ErrorAction Stop

    if ($feature.State -eq 'Enabled') {
        Write-Ok 'Hyper-V is enabled'
    } else {
        Write-Warn "Hyper-V is available but not enabled (state: $($feature.State))"
        if ($Install -and $PSCmdlet.ShouldProcess('Microsoft-Hyper-V-All', 'Enable-WindowsOptionalFeature')) {
            Write-Info 'Enabling Hyper-V...'
            $result = Enable-WindowsOptionalFeature -Online -FeatureName 'Microsoft-Hyper-V-All' -NoRestart -ErrorAction Stop
            Write-Ok 'Hyper-V enabled'
            if ($result.RestartNeeded) {
                $rebootRequired = $true
                Write-Warn 'A reboot is needed to complete Hyper-V activation'
            }
        } elseif (-not $Install) {
            $issues.Add('Hyper-V not enabled (run with -Install to enable)')
        }
    }
} catch {
    $msg = $_.Exception.Message
    if ($msg -match 'access.?denied|unauthorized|privilege|elevated') {
        Write-Warn 'Cannot check Hyper-V status — Administrator privileges required'
        $issues.Add('Hyper-V status unknown (requires Administrator)')
    } elseif ($msg -match 'not found|invalid feature') {
        Write-Warn 'Hyper-V feature not found — this Windows edition may not support Hyper-V (Home editions)'
    } else {
        Write-Warn "Hyper-V check failed: $msg"
    }
}

# ── 2. WSL2 ───────────────────────────────────────────────────────────────────
Write-Step '2/3  WSL2'
$wslAvailable = $null -ne (Get-Command wsl -ErrorAction SilentlyContinue)

if ($wslAvailable) {
    $versionLine = (wsl --version 2>&1 | Select-String 'WSL version' | Select-Object -First 1)?.Line?.Trim()
    if ($versionLine) {
        Write-Ok "WSL2 installed — $versionLine"
    } else {
        Write-Ok 'WSL2 installed'
    }

    Write-Info 'Checking for WSL2 kernel updates...'
    try {
        $updateLines = wsl --update 2>&1 | Where-Object { $_ -notmatch '^\s*$' }
        foreach ($line in $updateLines) { Write-Info "  $line" }
        Write-Ok 'WSL2 kernel is up to date'
    } catch {
        Write-Warn "WSL2 update check failed: $($_.Exception.Message)"
    }
} else {
    Write-Warn 'WSL2 is not installed'
    if ($Install -and $PSCmdlet.ShouldProcess('WSL2', 'wsl --install --no-distribution')) {
        Write-Info 'Installing WSL2 (no default distribution)...'
        wsl --install --no-distribution
        Write-Ok 'WSL2 installation initiated'
        $rebootRequired = $true
        Write-Warn 'A system reboot is required before WSL2 becomes active'
    } elseif (-not $Install) {
        $issues.Add('WSL2 not installed (run with -Install to install)')
    }
}

# ── 3. Ubuntu (WSL distro) ────────────────────────────────────────────────────
Write-Step '3/3  Ubuntu (WSL distro)'

if (-not $wslAvailable) {
    Write-Warn 'WSL2 not available — skipping Ubuntu check'
    $issues.Add('Ubuntu check skipped (WSL2 not installed)')
} else {
    # wsl --list --quiet may output UTF-16 with embedded null bytes on some builds;
    # convert through Out-String and strip null characters before processing.
    $rawList   = wsl --list --quiet 2>&1 | Out-String
    $normList  = $rawList -replace '\x00', '' -replace '\r', ''
    $ubuntuDistro = ($normList -split '\n' |
        Where-Object { $_ -match '(?i)^ubuntu' } |
        Select-Object -First 1)?.Trim()

    if ($ubuntuDistro) {
        Write-Ok "Ubuntu distro found: $ubuntuDistro"

        # apt-get update
        Write-Info "Running apt-get update in '$ubuntuDistro'..."
        try {
            wsl -d $ubuntuDistro -- bash -c 'apt-get update -y 2>&1' |
                Where-Object { $_ -notmatch '^\s*$' } |
                ForEach-Object { Write-Info "  $_" }
            Write-Ok 'Package index refreshed'
        } catch {
            Write-Warn "apt-get update failed: $($_.Exception.Message)"
        }

        # apt-get dist-upgrade
        Write-Info "Running apt-get dist-upgrade in '$ubuntuDistro'..."
        try {
            wsl -d $ubuntuDistro -- bash -c 'DEBIAN_FRONTEND=noninteractive apt-get dist-upgrade -y 2>&1' |
                Where-Object { $_ -notmatch '^\s*$' } |
                ForEach-Object { Write-Info "  $_" }
            Write-Ok 'Ubuntu fully upgraded'
        } catch {
            Write-Warn "apt-get dist-upgrade failed: $($_.Exception.Message)"
        }
    } else {
        Write-Warn 'Ubuntu WSL distro is not installed'
        if ($Install -and $PSCmdlet.ShouldProcess('Ubuntu', 'wsl --install -d Ubuntu')) {
            Write-Info 'Installing Ubuntu distro...'
            wsl --install -d Ubuntu
            Write-Ok 'Ubuntu installation started'
            Write-Info 'Ubuntu will complete its initial setup on first launch'
        } elseif (-not $Install) {
            $issues.Add('Ubuntu WSL distro not installed (run with -Install to install)')
        }
    }
}

# ── Summary ───────────────────────────────────────────────────────────────────
Write-Step 'Summary'
if ($issues.Count -eq 0) {
    Write-Ok 'All prerequisites satisfied'
} else {
    Write-Warn "$($issues.Count) issue(s) found:"
    foreach ($issue in $issues) { Write-Info "  $issue" }
    Write-Host ""
    Write-Info "Re-run with ${YELLOW}-Install${R} to fix these issues (requires Administrator)"
}

if ($rebootRequired) {
    Write-Host ""
    Write-Host "  $YELLOW$B⚠  A system reboot is required to complete pending installations.$R"
    Write-Info  "  After rebooting, re-run this script to verify the final state."
}

Write-Host ""
if ($issues.Count -gt 0) { exit 1 }
