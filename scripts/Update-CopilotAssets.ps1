#Requires -Version 7.0
<#
.SYNOPSIS
    Aggiorna skill, instructions e agent file di GitHub Copilot in .github/.

.DESCRIPTION
    Sincronizza i file di Copilot (.github/skills/, .github/instructions/,
    .github/agents/) dai repository sorgente configurati in
    scripts/copilot-assets-sources.json oppure dalla lista interna.

    Strategia per repo sorgente:
      - Git sparse-checkout: clona solo le sottodirectory necessarie
      - gh api: scarica singoli file via GitHub API (richiede gh CLI autenticato)

    I file presenti in locale ma non nel repo sorgente NON vengono rimossi
    (politica conservativa). Usa -Clean per rimuovere gli obsoleti.

.PARAMETER DryRun
    Mostra l'analisi senza scrivere file.

.PARAMETER Clean
    Rimuove i file locali non presenti nel sorgente remoto dopo la sync.

.PARAMETER SourcesFile
    Percorso alternativo al manifest delle sorgenti.
    Default: <repo-root>/scripts/copilot-assets-sources.json

.EXAMPLE
    .\Update-CopilotAssets.ps1 -DryRun
    .\Update-CopilotAssets.ps1
    .\Update-CopilotAssets.ps1 -Clean
#>
[CmdletBinding()]
param(
    [switch] $DryRun,
    [switch] $Clean,
    [string] $SourcesFile = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$RepoRoot = Split-Path $PSScriptRoot -Parent
$GhDir = Join-Path $RepoRoot '.github'
$DefaultSrc = Join-Path $PSScriptRoot 'copilot-assets-sources.json'
$SourcesPath = if ($SourcesFile) { $SourcesFile } else { $DefaultSrc }

Write-Host "`n=== Chishiki — Aggiornamento Copilot Assets ===" -ForegroundColor Cyan
Write-Host "  .github : $GhDir"
Write-Host "  Sorgenti: $SourcesPath`n"

# ── Prerequisiti ─────────────────────────────────────────────────────────────
$hasGh = $null -ne (Get-Command gh  -ErrorAction SilentlyContinue)
$hasGit = $null -ne (Get-Command git -ErrorAction SilentlyContinue)

if (-not $hasGh -and -not $hasGit) {
    Write-Error "Richiesto almeno gh CLI o git. Installa 'gh' (https://cli.github.com) e riprova."
}

# ── Carica sorgenti ───────────────────────────────────────────────────────────
if (Test-Path $SourcesPath) {
    $Sources = Get-Content $SourcesPath -Raw | ConvertFrom-Json
}
else {
    Write-Host "  Manifest sorgenti non trovato — uso sorgenti built-in." -ForegroundColor Yellow

    # Sorgenti predefinite note per gli asset Copilot di questo repo
    $Sources = [PSCustomObject]@{
        sources = @(
            [PSCustomObject]@{
                name     = 'microsoft-copilot-instructions'
                repo     = 'microsoft/copilot-instructions'
                branch   = 'main'
                mappings = @(
                    [PSCustomObject]@{ remote = 'skills'; local = '.github/skills' }
                    [PSCustomObject]@{ remote = 'instructions'; local = '.github/instructions' }
                    [PSCustomObject]@{ remote = 'agents'; local = '.github/agents' }
                )
            }
        )
    }

    # Salva il manifest per le esecuzioni future
    if (-not $DryRun) {
        $Sources | ConvertTo-Json -Depth 10 | Set-Content $SourcesPath -Encoding utf8
        Write-Host "  Manifest salvato in $SourcesPath (modifica per personalizzare)." -ForegroundColor DarkGray
    }
}

# ── Funzione: sync via gh api ─────────────────────────────────────────────────
function Sync-ViaGhApi {
    param(
        [string] $Repo,
        [string] $Branch,
        [string] $RemotePath,
        [string] $LocalDir
    )

    Write-Host "    gh api  $Repo/$RemotePath → $LocalDir" -ForegroundColor DarkGray

    # Lista il contenuto della directory remota
    $apiPath = "repos/$Repo/contents/$RemotePath`?ref=$Branch"
    try {
        $items = gh api $apiPath 2>&1 | ConvertFrom-Json
    }
    catch {
        Write-Warning "    Impossibile accedere a $apiPath : $_"
        return
    }

    if (-not (Test-Path $LocalDir)) {
        if (-not $DryRun) { New-Item $LocalDir -ItemType Directory -Force | Out-Null }
    }

    foreach ($item in $items) {
        $destPath = Join-Path $LocalDir $item.name
        if ($item.type -eq 'dir') {
            Sync-ViaGhApi -Repo $Repo -Branch $Branch `
                -RemotePath "$RemotePath/$($item.name)" `
                -LocalDir $destPath
        }
        elseif ($item.type -eq 'file') {
            if (-not $DryRun) {
                $content = [System.Text.Encoding]::UTF8.GetString(
                    [System.Convert]::FromBase64String($item.content -replace '\s', '')
                )
                Set-Content $destPath -Value $content -Encoding utf8 -NoNewline
            }
            Write-Host "      $($item.name)" -ForegroundColor DarkGray
        }
    }
}

# ── Funzione: sync via git sparse-checkout ────────────────────────────────────
function Sync-ViaGit {
    param(
        [string] $Repo,
        [string] $Branch,
        [string] $RemotePath,
        [string] $LocalDir
    )

    $tmpDir = Join-Path ([System.IO.Path]::GetTempPath()) "chishiki-copilot-sync-$(Get-Random)"
    Write-Host "    git sparse  $Repo/$RemotePath → $LocalDir" -ForegroundColor DarkGray

    try {
        git clone --filter=blob:none --sparse --depth 1 --branch $Branch `
            "https://github.com/$Repo.git" $tmpDir 2>&1 | Out-Null
        Push-Location $tmpDir
        git sparse-checkout set $RemotePath 2>&1 | Out-Null
        Pop-Location

        $srcDir = Join-Path $tmpDir $RemotePath
        if (Test-Path $srcDir) {
            if (-not $DryRun) {
                if (-not (Test-Path $LocalDir)) {
                    New-Item $LocalDir -ItemType Directory -Force | Out-Null
                }
                Copy-Item -Path "$srcDir\*" -Destination $LocalDir -Recurse -Force
            }
            $count = (Get-ChildItem $srcDir -Recurse -File).Count
            Write-Host "      $count file copiati da $RemotePath" -ForegroundColor DarkGray
        }
        else {
            Write-Warning "    Path $RemotePath non trovato in $Repo@$Branch"
        }
    }
    finally {
        if (Test-Path $tmpDir) { Remove-Item $tmpDir -Recurse -Force -ErrorAction SilentlyContinue }
    }
}

# ── Iterazione sorgenti ───────────────────────────────────────────────────────
$totalUpdated = 0
foreach ($src in $Sources.sources) {
    Write-Host "  Sorgente: $($src.repo) [$($src.branch)]" -ForegroundColor Yellow

    foreach ($map in $src.mappings) {
        $localAbs = Join-Path $RepoRoot $map.local

        if ($hasGh) {
            Sync-ViaGhApi -Repo $src.repo -Branch $src.branch `
                -RemotePath $map.remote -LocalDir $localAbs
        }
        elseif ($hasGit) {
            Sync-ViaGit -Repo $src.repo -Branch $src.branch `
                -RemotePath $map.remote -LocalDir $localAbs
        }
        $totalUpdated++
    }
}

if ($DryRun) {
    Write-Host "`n  [DryRun] Nessuna modifica applicata." -ForegroundColor Magenta
}
else {
    Write-Host "`n  Sincronizzazione completata: $totalUpdated mapping elaborati." -ForegroundColor Green
}

# ── Riepilogo locale ──────────────────────────────────────────────────────────
Write-Host "`n  Stato attuale .github/:" -ForegroundColor Cyan
@(
    [PSCustomObject]@{ Dir = '.github/skills'; Label = 'Skills' }
    [PSCustomObject]@{ Dir = '.github/instructions'; Label = 'Instructions' }
    [PSCustomObject]@{ Dir = '.github/agents'; Label = 'Agents' }
) | ForEach-Object {
    $abs = Join-Path $RepoRoot $_.Dir
    $count = if (Test-Path $abs) { (Get-ChildItem $abs -Recurse -Filter '*.md').Count } else { 0 }
    Write-Host "    $($_.Label.PadRight(14)) $($_.Dir)  →  $count file .md"
}

