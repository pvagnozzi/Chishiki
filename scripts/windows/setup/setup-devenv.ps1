#Requires -Version 7.0
<#
.SYNOPSIS
    🚀 Full developer environment setup — runs all setup scripts in sequence.

.DESCRIPTION
    Orchestrates the complete developer environment setup by running every
    setup script in the correct order:

      1/6  install-prereqs   — winget check, Chocolatey, Scoop, .NET SDK
      2/6  container-prereqs — Hyper-V, WSL2, Ubuntu  (Windows only)
      3/6  container-runtime — Docker / Podman
      4/6  dev-env           — Git, VS Code, PowerShell, Oh My Posh
      5/6  mcp-setup         — Node.js, uv, MCP server packages
      6/6  install-ide       — Visual Studio 2026 Professional (or Rider)

    Each script is called with -Install so missing tools are installed
    automatically. Pass -Rider to install JetBrains Rider instead of
    Visual Studio 2026 Professional.

.PARAMETER Install
    Pass -Install to every sub-script to install missing tools automatically.
    Without this flag the scripts run in check-only mode.

.PARAMETER Rider
    Install JetBrains Rider as the IDE instead of Visual Studio 2026 Professional.

.PARAMETER Help
    Show this help message and exit.

.EXAMPLE
    .\setup-devenv.ps1 -Install
    .\setup-devenv.ps1 -Install -Rider
    .\setup-devenv.ps1           # check-only: shows what is missing

.NOTES
    Requires: PowerShell 7+, Windows 10/11, winget
    Idempotent: safe to re-run — each sub-script skips steps already complete.
#>
[CmdletBinding(SupportsShouldProcess)]
param(
    [switch]$Install,
    [switch]$Rider,
    [switch]$Help
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ── ANSI Colors ──────────────────────────────────────────────────────────────
$E = [char]27
$R = "$E[0m";  $B = "$E[1m"
$RED    = "$E[31m"; $GREEN  = "$E[32m"; $YELLOW = "$E[33m"
$BLUE   = "$E[34m"; $CYAN   = "$E[36m"; $WHITE  = "$E[37m"

function Write-Header { param([string]$Msg)
    Write-Host "`n$B$BLUE╔══════════════════════════════════════════════╗$R"
    Write-Host "$B$BLUE║  $WHITE$Msg$BLUE$((' ' * [Math]::Max(0, 44 - $Msg.Length)))║$R"
    Write-Host "$B$BLUE╚══════════════════════════════════════════════╝$R"
}
function Write-Phase { param([string]$Msg) Write-Host "`n$B$BLUE━━━━  $WHITE$Msg$BLUE  ━━━━$R" }
function Write-Step  { param([string]$Msg) Write-Host "`n$B$CYAN  ──$R $Msg" }
function Write-Ok    { param([string]$Msg) Write-Host "    $GREEN✔$R  $Msg" }
function Write-Warn  { param([string]$Msg) Write-Host "    $YELLOW⚠$R  $Msg" }
function Write-Fail  { param([string]$Msg) Write-Host "    $RED✖$R  $Msg" }
function Write-Info  { param([string]$Msg) Write-Host "    $CYAN·$R  $Msg" }

if ($Help) { Get-Help $PSCommandPath -Detailed; exit 0 }

$setupDir = Split-Path $PSCommandPath -Parent

# ── Helper: run a sub-script and report outcome ───────────────────────────────
$failed = [System.Collections.Generic.List[string]]::new()

function Invoke-SetupScript {
    param(
        [string]$Label,
        [string]$Script,
        [string[]]$ExtraArgs = @()
    )
    Write-Phase $Label
    $scriptPath = Join-Path $setupDir $Script
    $args = @()
    if ($Install)      { $args += '-Install' }
    if ($ExtraArgs)    { $args += $ExtraArgs }
    try {
        & $scriptPath @args
        if ($LASTEXITCODE -and $LASTEXITCODE -ne 0) {
            Write-Warn "$Script exited with code $LASTEXITCODE"
            $script:failed.Add($Label)
        }
    } catch {
        Write-Fail "$Script failed: $_"
        $script:failed.Add($Label)
    }
}

# ── Main ──────────────────────────────────────────────────────────────────────
Write-Header '🚀 Dev Environment Setup'
Write-Host "  Platform: $B$WHITE Windows (PowerShell $($PSVersionTable.PSVersion))$R"
Write-Info "Mode:  $(if ($Install) { "${YELLOW}${B}install${R}" } else { "check only ${CYAN}(use -Install to install)${R}" })"
Write-Info "IDE:   $(if ($Rider) { "${CYAN}JetBrains Rider${R}" } else { "${CYAN}Visual Studio 2026 Professional${R}" })"

Invoke-SetupScript '1/6  Prerequisites'       'install-prereqs.ps1'
Invoke-SetupScript '2/6  Container prereqs'   'container-prereqs.ps1'
Invoke-SetupScript '3/6  Container runtime'   'container-runtime.ps1'
Invoke-SetupScript '4/6  Developer tools'     'dev-env.ps1'
Invoke-SetupScript '5/6  MCP servers'         'mcp-setup.ps1'
if ($Rider) {
    Invoke-SetupScript '6/6  IDE'             'install-ide.ps1' @('-Rider')
} else {
    Invoke-SetupScript '6/6  IDE'             'install-ide.ps1'
}

# ── Summary ───────────────────────────────────────────────────────────────────
Write-Step 'Summary'
Write-Host ""
if ($failed.Count -eq 0) {
    Write-Ok 'All setup steps completed successfully'
    Write-Info 'Start the full stack:'
    Write-Info "  ${CYAN}.\scripts\windows\dev\start.ps1${R}"
} else {
    Write-Warn "$($failed.Count) step(s) had issues:"
    foreach ($f in $failed) { Write-Info "  $f" }
    Write-Info "Re-run with ${YELLOW}-Install${R} to retry failed steps"
    Write-Host ""
    exit 1
}
Write-Host ""
