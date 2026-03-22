#Requires -Version 7.0
<#
.SYNOPSIS
    🤖  Install and verify all MCP server dependencies on Windows.

.DESCRIPTION
    Installs and verifies every dependency required by the MCP servers
    declared in .mcp.json:

      • Node.js 20+ / npm / npx     — npm-based MCP servers
      • uv / uvx                    — Python runner   (mcp-server-fetch)
      • gh CLI                      — GitHub CLI
      • GitHub Copilot extension    — gh extension install github/gh-copilot
      • dnx (.NET NuGet runner)     — Azure.Mcp  and  NuGet.Mcp.Server
      • @modelcontextprotocol/server-sequential-thinking
      • @modelcontextprotocol/server-postgres
      • @modelcontextprotocol/server-memory

    HTTP servers (github, microsoft-learn) require no installation.

.PARAMETER Install
    Install any missing prerequisite and all MCP tooling.

.PARAMETER Help
    Show this help message and exit.

.EXAMPLE
    .\mcp-setup.ps1
    .\mcp-setup.ps1 -Install

.NOTES
    Requires: PowerShell 7+, Windows 10/11, winget
    Idempotent: safe to re-run — skips steps already complete.
#>
[CmdletBinding(SupportsShouldProcess)]
param(
    [switch]$Install,
    [switch]$Help
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ── ANSI Colors ───────────────────────────────────────────────────────────────
$E = [char]27
$R = "$E[0m";  $B = "$E[1m"
$RED    = "$E[31m"; $GREEN  = "$E[32m"; $YELLOW = "$E[33m"
$BLUE   = "$E[34m"; $CYAN   = "$E[36m"; $WHITE  = "$E[37m"

function Write-Header { param([string]$Msg)
    Write-Host "`n$B$BLUE╔══════════════════════════════════════════════╗$R"
    Write-Host "$B$BLUE║  $WHITE$Msg$BLUE$((' ' * [Math]::Max(0, 44 - $Msg.Length)))║$R"
    Write-Host "$B$BLUE╚══════════════════════════════════════════════╝$R"
}
function Write-Step  { param([string]$Msg) Write-Host "`n$B$CYAN  ──$R $Msg" }
function Write-Ok    { param([string]$Msg) Write-Host "    $GREEN✔$R  $Msg" }
function Write-Warn  { param([string]$Msg) Write-Host "    $YELLOW⚠$R  $Msg" }
function Write-Fail  { param([string]$Msg) Write-Host "    $RED✖$R  $Msg" }
function Write-Info  { param([string]$Msg) Write-Host "    $CYAN·$R  $Msg" }

if ($Help) { Get-Help $PSCommandPath -Detailed; exit 0 }

# ── Helpers ───────────────────────────────────────────────────────────────────
function Get-ToolVersion { param([string]$Cmd, [string]$Flag = '--version')
    if (Get-Command $Cmd -ErrorAction SilentlyContinue) {
        try { return ((& $Cmd $Flag 2>&1 | Select-Object -First 1).ToString().Trim()) }
        catch { return '(version unknown)' }
    }
    return $null
}

function Invoke-Winget { param([string]$Id, [string]$Name)
    if (-not (Get-Command winget -ErrorAction SilentlyContinue)) {
        Write-Fail 'winget not found — install App Installer from the Microsoft Store'
        exit 1
    }
    if ($PSCmdlet.ShouldProcess($Name, "winget install $Id")) {
        Write-Info "Installing $Name via winget..."
        winget install --id $Id --silent --accept-source-agreements --accept-package-agreements
        # Refresh PATH so newly installed binaries are available in this session
        $env:PATH = [System.Environment]::GetEnvironmentVariable('PATH', 'Machine') + ';' +
                    [System.Environment]::GetEnvironmentVariable('PATH', 'User')
        Write-Ok "$Name installed"
    }
}

function Install-NpmPackage { param([string]$Package, [string]$Binary)
    if (Get-Command $Binary -ErrorAction SilentlyContinue) {
        Write-Ok "$Binary — already installed"
        return
    }
    if ($Install) {
        Write-Info "npm install -g $Package..."
        npm install -g $Package
        Write-Ok "$Binary installed"
    } else {
        Write-Warn "$Binary not found"
        $script:issues.Add("$Binary not installed  (re-run with -Install)")
    }
}

function Install-DotnetTool { param([string]$ToolId, [string]$DisplayName)
    $listed = dotnet tool list -g 2>&1 | Where-Object { $_ -imatch $ToolId }
    if ($listed) {
        $ver = ($listed -split '\s+')[1]
        Write-Ok "$DisplayName — v$ver (global)"
        return
    }
    if ($Install) {
        Write-Info "dotnet tool install --global $ToolId..."
        dotnet tool install --global $ToolId
        $env:PATH = $env:PATH + ";$env:USERPROFILE\.dotnet\tools"
        Write-Ok "$DisplayName installed"
    } else {
        Write-Warn "$DisplayName not found"
        $script:issues.Add("$DisplayName not installed  (re-run with -Install)")
    }
}

# ── Main ──────────────────────────────────────────────────────────────────────
Write-Header '🤖  MCP Servers Setup'
Write-Info "Mode: $(if ($Install) { "${YELLOW}${B}check + install${R}" } else { "check only ${CYAN}(use -Install to fix)${R}" })"

$issues = [System.Collections.Generic.List[string]]::new()

# ── 1/5  Prerequisites ────────────────────────────────────────────────────────
Write-Step '1/5  Prerequisites'

# Node.js (required for npm MCP packages)
$v = Get-ToolVersion 'node'
if ($v) { Write-Ok "Node.js — $v" }
elseif ($Install) { Invoke-Winget 'OpenJS.NodeJS.LTS' 'Node.js LTS' }
else { Write-Warn 'Node.js not found — required for npm MCP packages'; $issues.Add('Node.js not installed') }

# npx (ships with Node.js)
$v = Get-ToolVersion 'npx'
if ($v) { Write-Ok "npx — $v" }
else { Write-Warn 'npx not found (comes with Node.js)'; $issues.Add('npx not found') }

# uv / uvx (required for mcp-server-fetch)
$v = Get-ToolVersion 'uvx'
if ($v) { Write-Ok "uvx (uv) — $v" }
elseif ($Install) { Invoke-Winget 'astral-sh.uv' 'uv' }
else { Write-Warn 'uvx not found — required for mcp-server-fetch'; $issues.Add('uv / uvx not installed') }

# .NET SDK (required for dnx global tool)
$v = Get-ToolVersion 'dotnet'
if ($v) { Write-Ok ".NET SDK — $v" }
else { Write-Warn '.NET SDK not found — required for dnx'; $issues.Add('.NET SDK not installed') }

# gh CLI (required for GitHub Copilot extension)
$v = Get-ToolVersion 'gh'
if ($v) { Write-Ok "gh CLI — $($v -split "`n" | Select-Object -First 1)" }
elseif ($Install) { Invoke-Winget 'GitHub.cli' 'GitHub CLI' }
else { Write-Warn 'gh CLI not found — required for GitHub Copilot extension'; $issues.Add('gh CLI not installed') }

# ── 2/5  GitHub Copilot CLI ───────────────────────────────────────────────────
Write-Step '2/5  GitHub Copilot CLI'
if (Get-Command gh -ErrorAction SilentlyContinue) {
    $extList = gh extension list 2>&1
    if ($extList -match 'gh-copilot') {
        Write-Ok 'GitHub Copilot extension — already installed'
    } elseif ($Install) {
        Write-Info 'Installing GitHub Copilot extension...'
        gh extension install github/gh-copilot
        Write-Ok 'GitHub Copilot extension installed'
    } else {
        Write-Warn 'GitHub Copilot extension not installed'
        $issues.Add('gh-copilot extension not installed  (re-run with -Install)')
    }
} else {
    Write-Warn 'gh CLI not available — skipping Copilot extension'
}

# ── 3/5  dnx — .NET NuGet package runner ─────────────────────────────────────
Write-Step '3/5  dnx (.NET NuGet runner)'
Write-Info 'Used by: azure MCP server (Azure.Mcp) and nuget MCP server (NuGet.Mcp.Server)'
if (Get-Command dotnet -ErrorAction SilentlyContinue) {
    Install-DotnetTool 'dnx' 'dnx'
} else {
    Write-Warn '.NET SDK not available — skipping dnx installation'
}

# ── 4/5  npm MCP packages ─────────────────────────────────────────────────────
Write-Step '4/5  npm MCP packages'
if (Get-Command npm -ErrorAction SilentlyContinue) {
    Write-Info 'sequential-thinking server'
    Install-NpmPackage '@modelcontextprotocol/server-sequential-thinking' 'mcp-server-sequential-thinking'
    Write-Info 'postgres server'
    Install-NpmPackage '@modelcontextprotocol/server-postgres' 'mcp-server-postgres'
    Write-Info 'memory server'
    Install-NpmPackage '@modelcontextprotocol/server-memory' 'mcp-server-memory'
} else {
    Write-Warn 'npm not found — skipping MCP npm package installation'
    $issues.Add('npm not found — MCP npm packages not installed')
}

# ── 5/5  Summary ──────────────────────────────────────────────────────────────
Write-Step '5/5  Summary'
Write-Info 'HTTP servers (no installation required):'
Write-Ok   '  github          — https://api.githubcopilot.com/mcp/'
Write-Ok   '  microsoft-learn — https://learn.microsoft.com/api/mcp'
Write-Info 'stdio servers:'
Write-Ok   '  azure           — dnx Azure.Mcp → azmcp server start'
Write-Ok   '  nuget           — dnx NuGet.Mcp.Server'
Write-Ok   '  fetch           — uvx mcp-server-fetch'
Write-Ok   '  sequential-thinking — mcp-server-sequential-thinking'
Write-Ok   '  postgres        — mcp-server-postgres'
Write-Ok   '  memory          — mcp-server-memory'

if ($issues.Count -eq 0) {
    Write-Host ""
    Write-Ok 'All MCP server dependencies are satisfied'
} else {
    Write-Warn "$($issues.Count) issue(s) found:"
    foreach ($i in $issues) { Write-Info "  $i" }
    Write-Info "Re-run with ${YELLOW}-Install${R} to resolve"
    Write-Host ""
    exit 1
}
Write-Host ""
