#Requires -Version 7.0
<#
.SYNOPSIS
    Builds self-contained single-file binaries for the Chishiki Agent across all platforms.

.DESCRIPTION
    Publishes Chishiki.Agent.Host as a self-contained binary (no runtime required) for:
      Windows:  win-x64, win-arm64
      Linux:    linux-x64, linux-arm64
      macOS:    osx-x64, osx-arm64

    Output lands in dist/agent/{platform}/.

.PARAMETER Configuration
    Build configuration. Default is Release.

.PARAMETER OutputDir
    Root output directory. Default is <repo-root>/dist/agent.

.PARAMETER Platforms
    Comma-separated list of RIDs to build. Defaults to all 6.

.EXAMPLE
    .\Build-Agent.ps1
    .\Build-Agent.ps1 -Platforms win-x64,linux-x64
#>
[CmdletBinding()]
param(
    [string]   $Configuration = 'Release',
    [string]   $OutputDir = '',
    [string[]] $Platforms = @('win-x64', 'win-arm64', 'linux-x64', 'linux-arm64', 'osx-x64', 'osx-arm64')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$RepoRoot = Split-Path $PSScriptRoot -Parent
$HostCsproj = Join-Path $RepoRoot 'src\agents\Chishiki.Agent.Host\Chishiki.Agent.Host.csproj'
$OutRoot = if ($OutputDir) { $OutputDir } else { Join-Path $RepoRoot 'dist\agent' }

Write-Host "`n╔══════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   Chishiki Agent — Cross-platform Build      ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host "  Config   : $Configuration"
Write-Host "  Output   : $OutRoot"
Write-Host "  Platforms: $($Platforms -join ', ')`n"

$Results = @{}

foreach ($rid in $Platforms) {
    $outDir = Join-Path $OutRoot $rid
    Write-Host "  Building $rid …" -ForegroundColor Yellow

    $sw = [System.Diagnostics.Stopwatch]::StartNew()

    dotnet publish $HostCsproj `
        --configuration $Configuration `
        --runtime $rid `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:PublishTrimmed=false `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        --output $outDir `
        --nologo `
        -v q

    if ($LASTEXITCODE -eq 0) {
        $Results[$rid] = "OK ($($sw.Elapsed.TotalSeconds.ToString('F1'))s)"
        Write-Host "  ✓ $rid → $outDir" -ForegroundColor Green
    }
    else {
        $Results[$rid] = "FAILED"
        Write-Host "  ✗ $rid FAILED" -ForegroundColor Red
    }
}

# Build VS Code extension
Write-Host "`n  Building VS Code extension…" -ForegroundColor Yellow
$vscodDir = Join-Path $RepoRoot 'src\agents\vscode-extension'
$vscodeOut = Join-Path $OutRoot 'vscode-extension'

if (Get-Command npm -ErrorAction SilentlyContinue) {
    Push-Location $vscodDir
    try {
        npm install --silent
        npm run build
        New-Item -ItemType Directory -Force -Path $vscodeOut | Out-Null
        if (Get-Command vsce -ErrorAction SilentlyContinue) {
            vsce package --out $vscodeOut
            $Results['vscode-extension'] = 'OK'
        }
        else {
            Copy-Item -Path (Join-Path $vscodDir 'dist') -Destination $vscodeOut -Recurse -Force
            $Results['vscode-extension'] = 'OK (dist only — install vsce for .vsix)'
        }
        Write-Host "  ✓ VS Code extension → $vscodeOut" -ForegroundColor Green
    }
    catch {
        $Results['vscode-extension'] = "FAILED: $_"
        Write-Host "  ✗ VS Code extension FAILED: $_" -ForegroundColor Red
    }
    finally {
        Pop-Location
    }
}
else {
    $Results['vscode-extension'] = 'SKIPPED (npm not found)'
    Write-Host "  - VS Code extension SKIPPED (npm not found)" -ForegroundColor DarkGray
}

Write-Host "`n╔══════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║                 Summary                      ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════╝" -ForegroundColor Cyan
foreach ($key in $Results.Keys) {
    $val = $Results[$key]
    $color = if ($val -like 'FAILED*') { 'Red' } elseif ($val -like 'SKIPPED*') { 'DarkGray' } else { 'Green' }
    Write-Host ("  {0,-25} {1}" -f $key, $val) -ForegroundColor $color
}

