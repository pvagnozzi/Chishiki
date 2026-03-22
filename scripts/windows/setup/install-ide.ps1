#Requires -Version 7.0
<#
.SYNOPSIS
    💻 Install the development IDE on Windows.

.DESCRIPTION
    Checks for (and optionally installs) the IDE used for development.

      Default: Visual Studio 2026 Professional (recommended)
      -Rider:  JetBrains Rider

    Both IDEs fully support .NET 10, ASP.NET Core, and Orleans projects.
    Run install-prereqs.ps1 first to ensure winget and .NET SDK are present.

.PARAMETER Install
    Perform the installation via winget if the chosen IDE is not found.

.PARAMETER Rider
    Target JetBrains Rider instead of Visual Studio 2026 Professional.

.PARAMETER Help
    Show this help message and exit.

.EXAMPLE
    .\install-ide.ps1
    .\install-ide.ps1 -Install
    .\install-ide.ps1 -Install -Rider

.NOTES
    Requires: PowerShell 7+, Windows 10/11, winget
    Idempotent: running multiple times produces the same result.
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
function Write-Step  { param([string]$Msg) Write-Host "`n$B$CYAN  ──$R $Msg" }
function Write-Ok    { param([string]$Msg) Write-Host "    $GREEN✔$R  $Msg" }
function Write-Warn  { param([string]$Msg) Write-Host "    $YELLOW⚠$R  $Msg" }
function Write-Fail  { param([string]$Msg) Write-Host "    $RED✖$R  $Msg" }
function Write-Info  { param([string]$Msg) Write-Host "    $CYAN·$R  $Msg" }

if ($Help) { Get-Help $PSCommandPath -Detailed; exit 0 }

# ── IDE config ────────────────────────────────────────────────────────────────
if ($Rider) {
    $IdeName  = 'JetBrains Rider'
    $WingetId = 'JetBrains.Rider'
    $Patterns = @(
        "$env:LOCALAPPDATA\JetBrains\Toolbox\apps\Rider\*\bin\rider64.exe",
        'C:\Program Files\JetBrains\JetBrains Rider*\bin\rider64.exe'
    )
} else {
    $IdeName  = 'Visual Studio 2026 Professional'
    $WingetId = 'Microsoft.VisualStudio.2026.Professional'
    $Patterns = @(
        'C:\Program Files\Microsoft Visual Studio\2026\Professional\Common7\IDE\devenv.exe',
        'C:\Program Files\Microsoft Visual Studio\2026\Enterprise\Common7\IDE\devenv.exe',
        'C:\Program Files\Microsoft Visual Studio\2026\Community\Common7\IDE\devenv.exe'
    )
}

# ── Detect installed IDE ──────────────────────────────────────────────────────
function Find-IDE {
    foreach ($pattern in $script:Patterns) {
        $hit = Get-Item $pattern -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($hit) { return $hit.FullName }
    }
    return $null
}

# ── Install IDE ───────────────────────────────────────────────────────────────
function Install-IDE {
    if (-not (Get-Command winget -ErrorAction SilentlyContinue)) {
        Write-Fail 'winget not found — install App Installer from the Microsoft Store'
        exit 1
    }
    if ($PSCmdlet.ShouldProcess($script:IdeName, "install via winget ($script:WingetId)")) {
        Write-Info "Installing $script:IdeName via winget..."
        winget install --id $script:WingetId --silent --accept-source-agreements --accept-package-agreements
        Write-Ok "$script:IdeName installed"
    }
}

# ── Main ──────────────────────────────────────────────────────────────────────
Write-Header '💻 IDE Setup'
Write-Host "  Platform: $B$WHITE Windows (PowerShell $($PSVersionTable.PSVersion))$R"
Write-Info "Target IDE:  $B$WHITE$IdeName$R"
Write-Info "Mode:        $(if ($Install) { "${YELLOW}${B}check + install${R}" } else { "check only ${CYAN}(use -Install to install)${R}" })"

# ── 1/1  IDE ──────────────────────────────────────────────────────────────────
Write-Step "1/1  $IdeName"
$found = Find-IDE
if ($found) {
    Write-Ok "$IdeName — $CYAN$found$R"
} elseif ($Install) {
    Install-IDE
} else {
    Write-Warn "$IdeName not found"
    Write-Info "Re-run with ${YELLOW}-Install${R} to install"
    if (-not $Rider) {
        Write-Info "Or use ${YELLOW}-Install -Rider${R} to install JetBrains Rider instead"
    }
    Write-Host ""
    exit 1
}

# ── Summary ───────────────────────────────────────────────────────────────────
Write-Step 'Summary'
Write-Host ""
Write-Ok "IDE ready — $IdeName"
Write-Info 'Next — start the full stack:'
Write-Info "  ${CYAN}.\scripts\windows\dev\start.ps1${R}"
Write-Host ""
