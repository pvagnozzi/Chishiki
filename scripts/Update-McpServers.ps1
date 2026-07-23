#Requires -Version 7.0
<#
.SYNOPSIS
    Aggiorna i server MCP configurati in .mcp.json.

.DESCRIPTION
    Controlla le versioni npm dei server MCP stdio, aggiorna i package
    pinned alla versione stabile più recente e valida la connettività
    degli endpoint HTTP.

.PARAMETER DryRun
    Mostra l'analisi senza modificare .mcp.json.

.PARAMETER SkipConnectivityCheck
    Salta il ping degli endpoint HTTP (utile in ambienti offline).

.EXAMPLE
    .\Update-McpServers.ps1
    .\Update-McpServers.ps1 -DryRun
#>
[CmdletBinding()]
param(
    [switch] $DryRun,
    [switch] $SkipConnectivityCheck
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$RepoRoot = Split-Path $PSScriptRoot -Parent
$McpConfig = Join-Path $RepoRoot '.mcp.json'

Write-Host "`n=== Chishiki — Aggiornamento MCP Servers ===" -ForegroundColor Cyan
Write-Host "  Config : $McpConfig`n"

if (-not (Test-Path $McpConfig)) {
    Write-Error ".mcp.json non trovato in $RepoRoot"
    return
}

$config = Get-Content $McpConfig -Raw | ConvertFrom-Json

# ── Controllo connettività endpoint HTTP ─────────────────────────────────────
if (-not $SkipConnectivityCheck) {
    Write-Host "  Verifica endpoint HTTP..." -ForegroundColor Yellow
    foreach ($name in $config.servers.PSObject.Properties.Name) {
        $srv = $config.servers.$name
        if ($srv.type -eq 'http') {
            try {
                $resp = Invoke-WebRequest -Uri $srv.url -Method Head -TimeoutSec 5 -ErrorAction SilentlyContinue
                $status = if ($resp) { "$($resp.StatusCode)" } else { 'N/A' }
                Write-Host "    $name ($($srv.url)) → HTTP $status"
            }
            catch {
                Write-Host "    $name ($($srv.url)) → UNREACHABLE" -ForegroundColor DarkYellow
            }
        }
    }
}

# ── Controllo versioni npm dei server stdio ───────────────────────────────────
Write-Host "`n  Verifica versioni npm dei server stdio..." -ForegroundColor Yellow

$changed = $false
foreach ($name in $config.servers.PSObject.Properties.Name) {
    $srv = $config.servers.$name
    if ($srv.type -ne 'stdio') { continue }

    # Estrae il nome del package npm dagli args (es. npx -y @pkg/name@ver)
    $pkgArg = $srv.args | Where-Object { $_ -notmatch '^-' -and $_ -notmatch '^npx$' } | Select-Object -First 1
    if (-not $pkgArg) { continue }

    # Separa package e versione  (  @scope/name@version  oppure  name@version )
    if ($pkgArg -match '^(@?[^@]+)@(.+)$') {
        $pkgName = $Matches[1]
        $pkgVersion = $Matches[2]
    }
    else {
        $pkgName = $pkgArg
        $pkgVersion = 'latest'
    }

    # Salta i tag dinamici — già aggiornati automaticamente
    if ($pkgVersion -in @('latest', 'next', '*')) {
        Write-Host "    $name  $pkgName@$pkgVersion  → [auto-latest, nessun pin]" -ForegroundColor DarkGray
        continue
    }

    # Recupera la versione npm più recente
    try {
        $latestVersion = (npm show $pkgName version 2>/dev/null).Trim()
    }
    catch {
        Write-Host "    $name  $pkgName → impossibile contattare npm" -ForegroundColor DarkYellow
        continue
    }

    if ($latestVersion -and $latestVersion -ne $pkgVersion) {
        Write-Host "    $name  $pkgName  $pkgVersion → $latestVersion" -ForegroundColor Green

        if (-not $DryRun) {
            # Aggiorna il singolo argomento nel JSON
            $newArg = "$pkgName@$latestVersion"
            $argIdx = $srv.args.IndexOf($pkgArg)
            if ($argIdx -ge 0) {
                $srv.args[$argIdx] = $newArg
                $changed = $true
            }
        }
    }
    else {
        Write-Host "    $name  $pkgName@$pkgVersion  → aggiornato" -ForegroundColor DarkGray
    }
}

# ── Salva .mcp.json se modificato ─────────────────────────────────────────────
if ($changed -and -not $DryRun) {
    $config | ConvertTo-Json -Depth 10 | Set-Content $McpConfig -Encoding utf8
    Write-Host "`n  .mcp.json aggiornato." -ForegroundColor Green
}
elseif ($DryRun) {
    Write-Host "`n  [DryRun] Nessuna modifica applicata." -ForegroundColor Magenta
}
else {
    Write-Host "`n  Nessun aggiornamento necessario." -ForegroundColor Green
}

# ── Riepilogo config attuale ──────────────────────────────────────────────────
Write-Host "`n  Server MCP attivi:" -ForegroundColor Cyan
foreach ($name in $config.servers.PSObject.Properties.Name) {
    $srv = $config.servers.$name
    $detail = if ($srv.type -eq 'http') { $srv.url } else { "$($srv.command) $($srv.args -join ' ')" }
    Write-Host "    [$($srv.type.ToUpper())]  $name  →  $detail"
}

