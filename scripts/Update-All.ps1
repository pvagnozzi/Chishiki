#Requires -Version 7.0
<#
.SYNOPSIS
    Script master: aggiorna tutti i componenti del progetto Chishiki.

.DESCRIPTION
    Esegue in sequenza:
      1. Update-DotnetTools    — global .NET tools
      2. Update-NugetPackages  — pacchetti NuGet della soluzione
      3. Update-McpServers     — versioni dei server MCP in .mcp.json
      4. Update-CopilotAssets  — skill, instructions e agent Copilot

    Ogni step può essere saltato con i rispettivi flag -Skip*.

.PARAMETER DryRun
    Propaga -DryRun a tutti gli step. Mostra cosa verrebbe fatto senza modifiche.

.PARAMETER SkipTools
    Salta l'aggiornamento dei global .NET tools.

.PARAMETER SkipPackages
    Salta l'aggiornamento dei pacchetti NuGet.

.PARAMETER SkipMcp
    Salta l'aggiornamento dei server MCP.

.PARAMETER SkipCopilot
    Salta la sincronizzazione dei Copilot asset.

.PARAMETER IncludePreRelease
    Propaga -IncludePreRelease a Update-NugetPackages.

.EXAMPLE
    .\Update-All.ps1
    .\Update-All.ps1 -DryRun
    .\Update-All.ps1 -SkipCopilot -SkipMcp
    .\Update-All.ps1 -IncludePreRelease
#>
[CmdletBinding()]
param(
    [switch] $DryRun,
    [switch] $SkipTools,
    [switch] $SkipPackages,
    [switch] $SkipMcp,
    [switch] $SkipCopilot,
    [switch] $IncludePreRelease
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$ScriptsDir = $PSScriptRoot
$StartTime = Get-Date

Write-Host ""
Write-Host "╔══════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║       Chishiki — Aggiornamento Completo              ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host "  Avvio : $($StartTime.ToString('yyyy-MM-dd HH:mm:ss'))"
Write-Host "  DryRun: $DryRun`n"

$Results = [ordered]@{}

function Invoke-Step {
    param(
        [string]   $Name,
        [string]   $Script,
        [hashtable] $Params,
        [bool]     $Skip
    )

    if ($Skip) {
        Write-Host "  ── $Name → SALTATO" -ForegroundColor DarkGray
        $Results[$Name] = 'SKIPPED'
        return
    }

    Write-Host "  ── $Name..." -ForegroundColor Yellow
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        & (Join-Path $ScriptsDir $Script) @Params
        $Results[$Name] = "OK ($($sw.Elapsed.TotalSeconds.ToString('F1'))s)"
        Write-Host "  ── $Name → OK ($($sw.Elapsed.TotalSeconds.ToString('F1'))s)`n" -ForegroundColor Green
    }
    catch {
        $Results[$Name] = "ERRORE: $_"
        Write-Host "  ── $Name → ERRORE: $_`n" -ForegroundColor Red
    }
}

# ── Step 1: .NET Global Tools ─────────────────────────────────────────────────
Invoke-Step -Name 'DotnetTools' -Script 'Update-DotnetTools.ps1' `
    -Params @{ DryRun = $DryRun.IsPresent } `
    -Skip $SkipTools.IsPresent

# ── Step 2: NuGet Packages ────────────────────────────────────────────────────
Invoke-Step -Name 'NugetPackages' -Script 'Update-NugetPackages.ps1' `
    -Params @{
    DryRun            = $DryRun.IsPresent
    IncludePreRelease = $IncludePreRelease.IsPresent
} `
    -Skip $SkipPackages.IsPresent

# ── Step 3: MCP Servers ───────────────────────────────────────────────────────
Invoke-Step -Name 'McpServers' -Script 'Update-McpServers.ps1' `
    -Params @{ DryRun = $DryRun.IsPresent } `
    -Skip $SkipMcp.IsPresent

# ── Step 4: Copilot Assets ────────────────────────────────────────────────────
Invoke-Step -Name 'CopilotAssets' -Script 'Update-CopilotAssets.ps1' `
    -Params @{ DryRun = $DryRun.IsPresent } `
    -Skip $SkipCopilot.IsPresent

# ── Riepilogo ─────────────────────────────────────────────────────────────────
$Elapsed = (Get-Date) - $StartTime
Write-Host ""
Write-Host "╔══════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║                    Riepilogo                         ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════╝" -ForegroundColor Cyan

foreach ($key in $Results.Keys) {
    $val = $Results[$key]
    $color = if ($val -like 'ERRORE*') { 'Red' } elseif ($val -eq 'SKIPPED') { 'DarkGray' } else { 'Green' }
    Write-Host ("  {0,-20} {1}" -f $key, $val) -ForegroundColor $color
}

Write-Host ""
Write-Host "  Durata totale: $($Elapsed.TotalSeconds.ToString('F1'))s" -ForegroundColor Cyan

if ($DryRun) {
    Write-Host "  [DryRun] Nessuna modifica è stata applicata." -ForegroundColor Magenta
}

