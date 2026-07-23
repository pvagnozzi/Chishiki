#Requires -Version 7.0
<#
.SYNOPSIS
    Aggiorna i pacchetti NuGet del progetto Chishiki.

.DESCRIPTION
    Esegue dotnet-outdated per rilevare i pacchetti obsoleti e li aggiorna
    rispettando i vincoli di versione (esclude preview .NET 11+ per i pacchetti
    di framework, aggiorna liberamente i pacchetti di terze parti stabili).

.PARAMETER DryRun
    Mostra solo i pacchetti obsoleti senza applicare modifiche.

.PARAMETER IncludePreRelease
    Considera anche le release di anteprima (alpha/beta/rc) per i pacchetti
    che già usano una versione pre-release.

.PARAMETER ExcludeFrameworkPreviews
    Esclude l'upgrade ai preview di .NET 11+ per i pacchetti Microsoft.*,
    System.* e Aspire.*. Default: $true.

.EXAMPLE
    .\Update-NugetPackages.ps1 -DryRun
    .\Update-NugetPackages.ps1 -IncludePreRelease
#>
[CmdletBinding()]
param(
    [switch] $DryRun,
    [switch] $IncludePreRelease,
    [bool]   $ExcludeFrameworkPreviews = $true
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$RepoRoot = Split-Path $PSScriptRoot -Parent
$Solution = Join-Path $RepoRoot 'Chishiki.slnx'

Write-Host "`n=== Chishiki — Aggiornamento pacchetti NuGet ===" -ForegroundColor Cyan
Write-Host "  Soluzione : $Solution"
Write-Host "  DryRun    : $DryRun"
Write-Host "  PreRelease: $IncludePreRelease`n"

# ── Prerequisiti ─────────────────────────────────────────────────────────────
if (-not (Get-Command dotnet-outdated -ErrorAction SilentlyContinue)) {
    Write-Host "  Installazione dotnet-outdated-tool..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-outdated-tool
}

# ── Pacchetti di framework Microsoft da non promuovere ai preview .NET 11+ ───
$FrameworkPrefixes = @(
    'Microsoft.AspNetCore.',
    'Microsoft.EntityFrameworkCore',
    'Microsoft.Extensions.',
    'System.',
    'Aspire.',
    'Npgsql.EntityFrameworkCore.'
)

# ── Analisi ───────────────────────────────────────────────────────────────────
Write-Host "  Analisi pacchetti obsoleti..." -ForegroundColor Yellow
$OutdatedArgs = @('outdated', $Solution, '--output', 'json', '--output-filename', 'outdated.json')
if ($IncludePreRelease) { $OutdatedArgs += '--pre-release' }

Push-Location $RepoRoot
try {
    & dotnet @OutdatedArgs | Out-Null

    if (-not (Test-Path 'outdated.json')) {
        Write-Host "  Nessun file di output generato; tutti i pacchetti sono aggiornati." -ForegroundColor Green
        return
    }

    $Report = Get-Content 'outdated.json' -Raw | ConvertFrom-Json
    Remove-Item 'outdated.json' -Force

    $Updates = @()
    foreach ($project in $Report.Projects) {
        foreach ($tf in $project.TargetFrameworks) {
            foreach ($dep in $tf.Dependencies) {
                if ($dep.LatestVersion -and $dep.ResolvedVersion -ne $dep.LatestVersion) {
                    $Updates += [PSCustomObject]@{
                        Project     = $project.Name
                        Package     = $dep.Name
                        Current     = $dep.ResolvedVersion
                        Latest      = $dep.LatestVersion
                        IsFramework = ($FrameworkPrefixes | Where-Object { $dep.Name.StartsWith($_) }).Count -gt 0
                    }
                }
            }
        }
    }

    if ($Updates.Count -eq 0) {
        Write-Host "  Tutti i pacchetti sono aggiornati." -ForegroundColor Green
        return
    }

    Write-Host "`n  Pacchetti obsoleti trovati: $($Updates.Count)" -ForegroundColor Yellow
    $Updates | Format-Table Package, Current, Latest, IsFramework -AutoSize

    if ($DryRun) {
        Write-Host "`n  [DryRun] Nessuna modifica applicata." -ForegroundColor Magenta
        return
    }

    # ── Applicazione aggiornamenti ────────────────────────────────────────────
    $Skipped = 0
    $Applied = 0

    foreach ($upd in $Updates) {
        # Salta promozione a preview di framework .NET 11+
        if ($ExcludeFrameworkPreviews -and $upd.IsFramework) {
            $latestMajor = [int]($upd.Latest -replace '\..*', '')
            if ($latestMajor -ge 11) {
                Write-Host "  SKIP  $($upd.Package) $($upd.Current) → $($upd.Latest) (preview .NET 11+)" -ForegroundColor DarkGray
                $Skipped++
                continue
            }
        }

        Write-Host "  UP    $($upd.Package) $($upd.Current) → $($upd.Latest)" -ForegroundColor Green

        $ProjFiles = Get-ChildItem $RepoRoot -Filter '*.csproj' -Recurse
        foreach ($f in $ProjFiles) {
            $content = Get-Content $f.FullName -Raw
            if ($content -match [regex]::Escape($upd.Package)) {
                dotnet add $f.FullName package $upd.Package --version $upd.Latest | Out-Null
            }
        }
        $Applied++
    }

    Write-Host "`n  Aggiornamenti applicati : $Applied" -ForegroundColor Green
    Write-Host "  Ignorati (framework 11+): $Skipped" -ForegroundColor DarkGray

    # ── Restore ───────────────────────────────────────────────────────────────
    Write-Host "`n  Ripristino dipendenze..." -ForegroundColor Yellow
    dotnet restore $Solution
    Write-Host "  Restore completato." -ForegroundColor Green
}
finally {
    Pop-Location
}

