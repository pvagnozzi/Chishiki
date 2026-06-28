#Requires -Version 7.0
<#
.SYNOPSIS
    Aggiorna o installa i global .NET tools usati nel progetto Chishiki.

.DESCRIPTION
    Gestisce l'elenco canonico di dotnet global tools necessari per sviluppo,
    test, migrazione EF Core e controllo versione. Installa i tool mancanti
    e aggiorna quelli già presenti all'ultima versione stabile.

.PARAMETER DryRun
    Mostra cosa verrebbe fatto senza apportare modifiche.

.EXAMPLE
    .\Update-DotnetTools.ps1
    .\Update-DotnetTools.ps1 -DryRun
#>
[CmdletBinding()]
param(
    [switch] $DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Write-Host "`n=== Chishiki — Aggiornamento .NET Global Tools ===" -ForegroundColor Cyan
Write-Host "  DryRun: $DryRun`n"

# ── Elenco canonico dei tool richiesti ───────────────────────────────────────
$RequiredTools = @(
    [PSCustomObject]@{ Id = 'dotnet-ef'; Command = 'dotnet-ef' },
    [PSCustomObject]@{ Id = 'coverlet.console'; Command = 'coverlet' },
    [PSCustomObject]@{ Id = 'dotnet-reportgenerator-globaltool'; Command = 'reportgenerator' },
    [PSCustomObject]@{ Id = 'gitversion.tool'; Command = 'dotnet-gitversion' },
    [PSCustomObject]@{ Id = 'dotnet-outdated-tool'; Command = 'dotnet-outdated' }
)

# ── Tool installati ───────────────────────────────────────────────────────────
$InstalledOutput = dotnet tool list --global 2>&1
$Installed = @{}
foreach ($line in $InstalledOutput | Select-Object -Skip 2) {
    if ($line -match '^(\S+)\s+(\S+)\s+(\S+)') {
        $Installed[$Matches[1].ToLower()] = $Matches[2]
    }
}

# ── Analisi e aggiornamento ───────────────────────────────────────────────────
$Results = @()
foreach ($tool in $RequiredTools) {
    $id = $tool.Id.ToLower()
    $exists = $Installed.ContainsKey($id)
    $action = if ($exists) { 'update' } else { 'install' }

    $Results += [PSCustomObject]@{
        Id      = $tool.Id
        Current = if ($exists) { $Installed[$id] } else { '—' }
        Action  = $action
    }
}

$Results | Format-Table Id, Current, Action -AutoSize

if ($DryRun) {
    Write-Host "  [DryRun] Nessuna modifica applicata." -ForegroundColor Magenta
    return
}

# ── Esecuzione ────────────────────────────────────────────────────────────────
$Errors = 0
foreach ($r in $Results) {
    Write-Host "  $($r.Action.ToUpper().PadRight(7)) $($r.Id)..." -NoNewline
    try {
        if ($r.Action -eq 'install') {
            dotnet tool install --global $r.Id | Out-Null
        }
        else {
            dotnet tool update --global $r.Id | Out-Null
        }
        Write-Host " OK" -ForegroundColor Green
    }
    catch {
        Write-Host " ERRORE: $_" -ForegroundColor Red
        $Errors++
    }
}

Write-Host ""
if ($Errors -gt 0) {
    Write-Warning "Completato con $Errors errori."
}
else {
    Write-Host "  Tutti i tool sono aggiornati." -ForegroundColor Green
}

# ── Versioni finali ───────────────────────────────────────────────────────────
Write-Host "`n  Tool installati:" -ForegroundColor Cyan
dotnet tool list --global

