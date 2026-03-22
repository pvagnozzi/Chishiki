#Requires -Version 7.0
<#
.SYNOPSIS
    🛑 Stop the development stack.

.DESCRIPTION
    Finds and terminates the Aspire AppHost process, then stops any
    lingering project Docker containers. Safe to run when the stack
    is already stopped (idempotent).

.PARAMETER Force
    Skip confirmation prompt.

.PARAMETER Help
    Show this help message and exit.

.EXAMPLE
    .\stop.ps1
    .\stop.ps1 -Force

.NOTES
    Requires: PowerShell 7+
    Idempotent: safe to run if the stack is already stopped.
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

Write-Header '🛑 Stop Dev Stack'

# ── Stop AppHost process ──────────────────────────────────────────────────────
Write-Step 'Looking for Aspire AppHost process...'
$procs = Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" |
    Where-Object { $_.CommandLine -like '*Chishiki.Infrastructure.Aspire.AppHost*' }

if ($procs) {
    foreach ($proc in $procs) {
        Write-Info "Stopping PID $($proc.ProcessId)"
        if ($PSCmdlet.ShouldProcess("PID $($proc.ProcessId)", 'Stop-Process')) {
            Stop-Process -Id $proc.ProcessId -Force -ErrorAction SilentlyContinue
        }
    }
    Write-Ok 'AppHost process stopped'
} else {
    Write-Warn 'No AppHost process found — may already be stopped'
}

# ── Stop lingering Docker containers ─────────────────────────────────────────
Write-Step 'Checking for lingering project containers...'
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Warn 'docker not found — skipping container cleanup'
} else {
    $patterns = @('postgresql','redis','keycloak','prometheus','grafana','qdrant','ollama','chishiki')
    $found    = [System.Collections.Generic.List[string]]::new()

    $running = docker ps -a --format '{{.Names}}' 2>$null
    foreach ($name in $running) {
        foreach ($p in $patterns) {
            if ($name -like "*$p*") { $found.Add($name); break }
        }
    }

    if ($found.Count -gt 0) {
        foreach ($name in $found) {
            Write-Info "Removing container: $name"
            if ($PSCmdlet.ShouldProcess($name, 'docker rm -f')) {
                docker rm -f $name 2>$null | Out-Null
            }
        }
        Write-Ok "$($found.Count) container(s) removed"
    } else {
        Write-Ok 'No lingering project containers found'
    }
}

Write-Host "`n  $GREEN$B✔ Stack stopped.$R`n"
