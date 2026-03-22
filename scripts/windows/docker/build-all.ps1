#Requires -Version 7.0
<#
.SYNOPSIS
    🐳 Build all Docker images.

.DESCRIPTION
    Discovers and builds every Dockerfile in the repository:
    infrastructure containers (containers/*) and service images
    (src/backend/*/Dockerfile). Each image is tagged as
    chishiki/<name>:latest using the parent directory as the name.
    Safe to re-run — Docker layer caching makes subsequent builds fast.

.PARAMETER NoCache
    Build without Docker layer cache (full rebuild).

.PARAMETER Filter
    Only build images whose name contains this string.

.PARAMETER Help
    Show this help message and exit.

.EXAMPLE
    .\build-all.ps1
    .\build-all.ps1 -NoCache
    .\build-all.ps1 -Filter ollama

.NOTES
    Requires: PowerShell 7+, Docker Desktop (running)
    Idempotent: re-running updates images in place.
#>
[CmdletBinding()]
param(
    [switch]$NoCache,
    [string]$Filter = '',
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

$repoRoot = (Get-Item $PSScriptRoot).Parent.Parent.Parent.FullName

Write-Header '🐳 Build All Docker Images'
Write-Info "Repo: $repoRoot"

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Fail 'docker not found — please install Docker Desktop'; exit 1
}
try { docker info 2>&1 | Out-Null; Write-Ok 'Docker daemon reachable' }
catch { Write-Fail 'Docker Desktop is not running — please start it first'; exit 1 }

# ── Discover Dockerfiles ──────────────────────────────────────────────────────
Write-Step 'Discovering Dockerfiles...'

$searchPaths = @(
    (Join-Path $repoRoot 'containers'),
    (Join-Path $repoRoot 'src\backend')
)

$dockerfiles = [System.Collections.Generic.List[hashtable]]::new()
foreach ($base in $searchPaths) {
    if (-not (Test-Path $base)) { continue }
    Get-ChildItem -Path $base -Recurse -Filter 'Dockerfile' | ForEach-Object {
        $name = $_.Directory.Name.ToLower() -replace '[^a-z0-9-]', '-'
        if ($Filter -and $name -notlike "*$($Filter.ToLower())*") { return }
        $dockerfiles.Add(@{ File = $_.FullName; Name = $name; Dir = $_.Directory.FullName })
    }
}

if ($dockerfiles.Count -eq 0) {
    Write-Warn 'No Dockerfiles found matching the current filter'
    exit 0
}

Write-Ok "Found $($dockerfiles.Count) Dockerfile(s)"
$dockerfiles | ForEach-Object { Write-Info "  chishiki/$($_.Name):latest" }

# ── Build each image ──────────────────────────────────────────────────────────
Write-Step 'Building images...'
$failed  = [System.Collections.Generic.List[string]]::new()
$built   = 0
$noCacheArg = if ($NoCache) { '--no-cache' } else { '' }

foreach ($df in $dockerfiles) {
    $tag = "chishiki/$($df.Name):latest"
    Write-Host "`n  $B$WHITE▶ Building $tag$R"
    Write-Info "  Dockerfile: $($df.File)"

    $buildArgs = @('build', '-f', $df.File, '-t', $tag)
    if ($NoCache) { $buildArgs += '--no-cache' }
    $buildArgs += $repoRoot    # context = repo root

    try {
        & docker @buildArgs
        Write-Ok "$tag — built successfully"
        $built++
    } catch {
        Write-Fail "$tag — FAILED: $_"
        $failed.Add($tag)
    }
}

# ── Summary ───────────────────────────────────────────────────────────────────
Write-Step 'Summary'
Write-Ok   "$built image(s) built successfully"
if ($failed.Count -gt 0) {
    Write-Fail "$($failed.Count) image(s) failed:"
    $failed | ForEach-Object { Write-Info "  $_" }
    exit 1
}
Write-Host "`n  $GREEN$B✔ All images built.$R`n"
