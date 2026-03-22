#Requires -Version 7.0
<#
.SYNOPSIS
    🗑️ Reset all persistent development data.

.DESCRIPTION
    Stops the running stack, removes all project Docker containers,
    and prunes unused Docker volumes (PostgreSQL data, Redis AOF/RDB,
    Keycloak realm data, etc.). Use this to get a completely clean
    local environment. Prompts for confirmation unless -Force is given.

.PARAMETER Force
    Skip the confirmation prompt and proceed immediately.

.PARAMETER Help
    Show this help message and exit.

.EXAMPLE
    .\reset-data.ps1
    .\reset-data.ps1 -Force

.NOTES
    Requires: PowerShell 7+, Docker Desktop
    ⚠ DESTRUCTIVE: all local development data will be lost.
    Idempotent: safe to run on an already-clean environment.
#>
[CmdletBinding(SupportsShouldProcess)]
param(
    [switch]$Force,
    [switch]$Help
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$E = [char]27; $R = "$E[0m"; $B = "$E[1m"
$RED = "$E[31m"; $GREEN = "$E[32m"; $YELLOW = "$E[33m"
$BLUE = "$E[34m"; $CYAN = "$E[36m"; $WHITE = "$E[37m"

function Write-Header { param([string]$Msg)
    Write-Host "`n$B$BLUE╔══════════════════════════════════════════════╗$R"
    Write-Host "$B$BLUE║  $WHITE$Msg$BLUE$((' ' * [Math]::Max(0, 44 - $Msg.Length)))║$R"
    Write-Host "$B$BLUE╚══════════════════════════════════════════════╝$R" }
function Write-Step { param([string]$Msg) Write-Host "`n$B$CYAN  ──$R $Msg" }
function Write-Ok   { param([string]$Msg) Write-Host "    $GREEN✔$R  $Msg" }
function Write-Warn { param([string]$Msg) Write-Host "    $YELLOW⚠$R  $Msg" }
function Write-Fail { param([string]$Msg) Write-Host "    $RED✖$R  $Msg" }
function Write-Info { param([string]$Msg) Write-Host "    $CYAN·$R  $Msg" }

if ($Help) { Get-Help $PSCommandPath -Detailed; exit 0 }

Write-Header '🗑️  Reset Development Data'
Write-Host "  $RED$B⚠  This will permanently delete all local development data.$R"
Write-Host "     Databases, caches, and auth state will be wiped."

if (-not $Force) {
    Write-Host ""
    $ans = Read-Host "  Type 'yes' to confirm"
    if ($ans -ne 'yes') {
        Write-Warn 'Aborted — no data was removed'
        exit 0
    }
}

# ── Step 1: Stop the stack ────────────────────────────────────────────────────
Write-Step '1/3  Stopping the dev stack...'
$stopScript = Join-Path $PSScriptRoot 'stop.ps1'
if (Test-Path $stopScript) {
    & $stopScript -Force
} else {
    $procs = Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" |
        Where-Object { $_.CommandLine -like '*Chishiki.Infrastructure.Aspire.AppHost*' }
    foreach ($proc in $procs) {
        Stop-Process -Id $proc.ProcessId -Force -ErrorAction SilentlyContinue
    }
    Write-Ok 'Stack stopped'
}

# ── Step 2: Remove project containers ────────────────────────────────────────
Write-Step '2/3  Removing project containers...'
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Warn 'docker not found — skipping'
} else {
    $patterns   = @('postgresql','redis','keycloak','prometheus','grafana','qdrant','ollama','chishiki')
    $containers = docker ps -a --format '{{.Names}}' 2>$null |
        Where-Object { $name = $_.ToLower(); $patterns | Where-Object { $name -like "*$_*" } }

    if ($containers) {
        foreach ($c in $containers) {
            Write-Info "Removing: $c"
            docker rm -f $c 2>$null | Out-Null
        }
        Write-Ok "$(@($containers).Count) container(s) removed"
    } else {
        Write-Ok 'No project containers found'
    }
}

# ── Step 3: Prune unused volumes ──────────────────────────────────────────────
Write-Step '3/3  Pruning unused Docker volumes...'
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Warn 'docker not found — skipping'
} else {
    $before = (docker volume ls -q 2>$null | Measure-Object).Count
    docker volume prune -f 2>$null | Out-Null
    $after  = (docker volume ls -q 2>$null | Measure-Object).Count
    $pruned = $before - $after
    Write-Ok "Volume prune complete — $pruned volume(s) removed"
}

Write-Host "`n  $GREEN$B✔ Data reset complete. Run start.ps1 to rebuild from scratch.$R`n"
