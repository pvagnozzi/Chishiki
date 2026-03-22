#Requires -Version 7.0
<#
.SYNOPSIS
    🧹 Remove project Docker images and prune dangling layers.

.DESCRIPTION
    Removes all local Docker images tagged chishiki/* and then prunes
    any dangling (untagged) image layers. Does not touch running containers
    or volumes. Safe to re-run (idempotent).

.PARAMETER Force
    Skip the confirmation prompt.

.PARAMETER DanglingOnly
    Only prune dangling images — do not remove chishiki/* tagged images.

.PARAMETER Help
    Show this help message and exit.

.EXAMPLE
    .\clean.ps1
    .\clean.ps1 -Force
    .\clean.ps1 -DanglingOnly

.NOTES
    Requires: PowerShell 7+, Docker Desktop
    Idempotent: safe to run when no images are present.
#>
[CmdletBinding(SupportsShouldProcess)]
param(
    [switch]$Force,
    [switch]$DanglingOnly,
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

Write-Header '🧹 Clean Docker Images'

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Fail 'docker not found — please install Docker Desktop'; exit 1
}
try { docker info 2>&1 | Out-Null }
catch { Write-Fail 'Docker Desktop is not running — please start it first'; exit 1 }

# ── Remove chishiki/* images ──────────────────────────────────────────────────
if (-not $DanglingOnly) {
    Write-Step 'Finding chishiki/* images...'
    $images = docker images --format '{{.Repository}}:{{.Tag}}\t{{.ID}}' 2>$null |
        ConvertFrom-Csv -Delimiter "`t" -Header Tag,ID |
        Where-Object { $_.Tag -like 'chishiki/*' }

    if ($images) {
        if (-not $Force) {
            Write-Host ""
            $images | ForEach-Object { Write-Info $_.Tag }
            Write-Host ""
            $ans = Read-Host "  Remove these $(@($images).Count) image(s)? (yes/no)"
            if ($ans -ne 'yes') { Write-Warn 'Aborted'; exit 0 }
        }
        foreach ($img in $images) {
            Write-Info "Removing: $($img.Tag)"
            if ($PSCmdlet.ShouldProcess($img.Tag, 'docker rmi')) {
                docker rmi -f $img.ID 2>$null | Out-Null
            }
        }
        Write-Ok "$(@($images).Count) image(s) removed"
    } else {
        Write-Ok 'No chishiki/* images found'
    }
}

# ── Prune dangling layers ─────────────────────────────────────────────────────
Write-Step 'Pruning dangling image layers...'
$dangling = docker images -f 'dangling=true' -q 2>$null
if ($dangling) {
    if ($PSCmdlet.ShouldProcess('dangling images', 'docker image prune')) {
        docker image prune -f 2>$null | Out-Null
    }
    Write-Ok 'Dangling layers pruned'
} else {
    Write-Ok 'No dangling layers found'
}

Write-Host "`n  $GREEN$B✔ Docker clean complete.$R`n"
