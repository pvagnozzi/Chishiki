#Requires -Version 7.0
<#
.SYNOPSIS
    🚀 Start the full development stack via Aspire AppHost.

.DESCRIPTION
    Launches the .NET Aspire AppHost which orchestrates all infrastructure
    services and containers required for local development (PostgreSQL, Redis,
    Keycloak, Ollama, Qdrant, Grafana, Prometheus, the Orleans host, and the
    API). Checks if the stack is already running before launching (idempotent).
    Dashboard: http://localhost:15888

.PARAMETER Detach
    Launch the AppHost in a new terminal window and return immediately.

.PARAMETER Help
    Show this help message and exit.

.EXAMPLE
    .\start.ps1
    .\start.ps1 -Detach

.NOTES
    Requires: PowerShell 7+, .NET 10 SDK, Docker Desktop (running)
    Idempotent: safe to re-run if the stack is already up.
#>
[CmdletBinding()]
param(
    [switch]$Detach,
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

$repoRoot    = (Get-Item $PSScriptRoot).Parent.Parent.Parent.FullName
$appHostProj = Join-Path $repoRoot 'src\infrastructure\aspire\Chishiki.Infrastructure.Aspire.AppHost\Chishiki.Infrastructure.Aspire.AppHost.csproj'
$dashboard   = 'http://localhost:15888'
$dashPort    = 15888

Write-Header '🚀 Start Dev Stack'
Write-Info "Repo:      $repoRoot"
Write-Info "Dashboard: $CYAN$dashboard$R"

Write-Step 'Checking prerequisites...'
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Fail '.NET SDK not found. Run: scripts\windows\setup\install-prereqs.ps1 -Install'
    exit 1
}
Write-Ok ".NET SDK $(dotnet --version)"

try { docker info 2>&1 | Out-Null; Write-Ok 'Docker daemon reachable' }
catch { Write-Fail 'Docker Desktop is not running — please start it first'; exit 1 }

if (-not (Test-Path $appHostProj)) {
    Write-Fail "AppHost project not found:`n    $appHostProj"
    exit 1
}

Write-Step 'Checking if stack is already running...'
$alreadyUp = $false
try {
    $tcp = [System.Net.Sockets.TcpClient]::new()
    $tcp.Connect('127.0.0.1', $dashPort)
    $tcp.Close()
    $alreadyUp = $true
} catch { }

if ($alreadyUp) {
    Write-Warn "Stack already running — dashboard at $CYAN$dashboard$R"
    Write-Host ""
    exit 0
}

Write-Step 'Launching Aspire AppHost...'
Write-Host ""

if ($Detach) {
    $proc = Start-Process -FilePath 'dotnet' `
        -ArgumentList "run --project `"$appHostProj`"" `
        -WorkingDirectory $repoRoot `
        -PassThru
    Write-Ok  "AppHost started (PID $($proc.Id))"
    Write-Info "Dashboard available at $CYAN$dashboard$R in ~30 s"
    Write-Info "Stop with: scripts\windows\dev\stop.ps1"
} else {
    Write-Warn 'Running in foreground — press Ctrl+C to stop the stack'
    Write-Host ""
    Push-Location $repoRoot
    try { dotnet run --project $appHostProj }
    finally { Pop-Location }
}
