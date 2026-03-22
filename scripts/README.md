# 🛠️ Scripts

Cross-platform developer scripts for setting up and managing a local development environment. Every script is available for all three supported platforms and behaves identically across them.

## Folder structure

```
scripts/
├── windows/          # PowerShell 7+ (.ps1)
│   ├── setup/
│   ├── dev/
│   └── docker/
├── linux/            # Bash 4+ (.sh)  — all distributions
│   ├── setup/
│   ├── dev/
│   └── docker/
└── macos/            # Zsh 5.8+ (.zsh)
    ├── setup/
    ├── dev/
    └── docker/
```

## Platform requirements

| Platform | Shell      | Minimum version | Install backend |
|----------|------------|-----------------|-----------------|
| Windows  | PowerShell | 7.0             | `winget`        |
| Linux    | bash       | 4.0             | apt / dnf / pacman |
| macOS    | zsh        | 5.8             | Homebrew        |

## Script index

### `setup/` — Environment preparation

| Script                     | Platforms           | Description                                              |
|----------------------------|---------------------|----------------------------------------------------------|
| `setup-devenv`             | Windows, Linux, macOS | **One-command setup** — runs all other setup scripts in order |
| `install-prereqs`          | Windows, Linux, macOS | Verify (and optionally install) package managers (Chocolatey, Scoop, Homebrew) and .NET SDK 10+ |
| `container-runtime`        | Windows, Linux, macOS | Detect Docker or Podman; install Podman CLI + Desktop if neither found. On Windows also runs `container-prereqs` |
| `container-prereqs`        | **Windows only**    | Verify (and optionally install/update) Hyper-V, WSL2, and Ubuntu |
| `dev-env`                  | Windows, Linux, macOS | Install Git, VS Code, PowerShell, Oh My Posh (M365Princess theme + MesloLGS NF font); auto-configure all detected shell profiles |
| `mcp-setup`                | Windows, Linux, macOS | Install GitHub Copilot CLI, `npx`, `uvx`, `dnx`, and all npm MCP packages required by `.mcp.json` |
| `install-ide`              | Windows, Linux, macOS | Install the development IDE — Visual Studio 2026 Professional (Windows default) or JetBrains Rider |

**Usage — `setup-devenv`** ← _start here on a new machine_

```powershell
# Windows — check what is missing (no changes)
.\scripts\windows\setup\setup-devenv.ps1

# Windows — install everything (VS 2026 Professional)
.\scripts\windows\setup\setup-devenv.ps1 -Install

# Windows — install everything with JetBrains Rider as IDE
.\scripts\windows\setup\setup-devenv.ps1 -Install -Rider
```

```bash
# Linux — check only
bash scripts/linux/setup/setup-devenv.sh

# Linux — install everything
bash scripts/linux/setup/setup-devenv.sh --install
```

```zsh
# macOS — check only
zsh scripts/macos/setup/setup-devenv.zsh

# macOS — install everything
zsh scripts/macos/setup/setup-devenv.zsh --install
```

---

**Usage — `install-prereqs`**

```powershell
# Windows — check only
.\scripts\windows\setup\install-prereqs.ps1

# Windows — auto-install missing tools
.\scripts\windows\setup\install-prereqs.ps1 -Install
```

```bash
# Linux — check only
bash scripts/linux/setup/install-prereqs.sh

# Linux — auto-install missing tools
bash scripts/linux/setup/install-prereqs.sh --install
```

```zsh
# macOS — check only
zsh scripts/macos/setup/install-prereqs.zsh

# macOS — auto-install via Homebrew
zsh scripts/macos/setup/install-prereqs.zsh --install
```

> **Note:** `install-prereqs` is the **foundation layer** — it installs only the package
> managers (Chocolatey + Scoop on Windows, Homebrew on Linux/macOS) and the .NET SDK.
> All other tools (Docker, Git, Node.js, IDE) are handled by the dedicated scripts above.
> Run `setup-devenv` to install everything in one step.

**Usage — `container-runtime`**

```powershell
# Windows — check only
.\scripts\windows\setup\container-runtime.ps1

# Windows — install Podman if no runtime found (also runs container-prereqs -Install)
.\scripts\windows\setup\container-runtime.ps1 -Install
```

```bash
# Linux — check only
bash scripts/linux/setup/container-runtime.sh

# Linux — install Podman CLI + Desktop via Flatpak if no runtime found
bash scripts/linux/setup/container-runtime.sh --install
```

```zsh
# macOS — check only
zsh scripts/macos/setup/container-runtime.zsh

# macOS — install Podman CLI + Desktop via Homebrew if no runtime found
zsh scripts/macos/setup/container-runtime.zsh --install
```

**Usage — `container-prereqs` (Windows only)**

```powershell
# Check Hyper-V, WSL2, and Ubuntu status; update WSL2 kernel and Ubuntu packages if already installed
.\scripts\windows\setup\container-prereqs.ps1

# Check and install any missing component (requires Administrator)
.\scripts\windows\setup\container-prereqs.ps1 -Install
```

> **Note:** This script is intentionally Windows-only. Hyper-V and WSL2 are Windows-exclusive
> features with no equivalent on Linux or macOS.

**Usage — `dev-env`**

```powershell
# Windows — check only
.\scripts\windows\setup\dev-env.ps1

# Windows — install missing tools and configure Oh My Posh in the PowerShell profile
.\scripts\windows\setup\dev-env.ps1 -Install
```

```bash
# Linux — check only
bash scripts/linux/setup/dev-env.sh

# Linux — install missing tools and configure Oh My Posh in all detected shell profiles
bash scripts/linux/setup/dev-env.sh --install
```

```zsh
# macOS — check only
zsh scripts/macos/setup/dev-env.zsh

# macOS — install missing tools via Homebrew and configure Oh My Posh in all detected shell profiles
zsh scripts/macos/setup/dev-env.zsh --install
```

> **Note:** After running with `--install` / `-Install`, restart your terminal (or source your shell
> profile) to activate the M365Princess prompt. If icons render as boxes, set your terminal's font
> to **MesloLGS NF** or another [Nerd Font](https://www.nerdfonts.com/).

**Usage — `mcp-setup`**

```powershell
# Windows — check only
.\scripts\windows\setup\mcp-setup.ps1

# Windows — install GitHub Copilot CLI, dnx, npm MCP packages, and all prerequisites
.\scripts\windows\setup\mcp-setup.ps1 -Install
```

```bash
# Linux — check only
bash scripts/linux/setup/mcp-setup.sh

# Linux — install everything needed for all .mcp.json stdio servers
bash scripts/linux/setup/mcp-setup.sh --install
```

```zsh
# macOS — check only
zsh scripts/macos/setup/mcp-setup.zsh

# macOS — install via Homebrew + npm + dotnet global tool
zsh scripts/macos/setup/mcp-setup.zsh --install
```

> **Note:** The `github` and `microsoft-learn` MCP servers are HTTP-based and require no local
> installation. The `azure` and `nuget` servers use `dnx` (a .NET NuGet package runner) to
> download and run their packages on first use. Run `gh auth login` before using the GitHub MCP
> server.

**Usage — `install-ide`**

```powershell
# Windows — check only (defaults to Visual Studio 2026 Professional)
.\scripts\windows\setup\install-ide.ps1

# Windows — install Visual Studio 2026 Professional
.\scripts\windows\setup\install-ide.ps1 -Install

# Windows — install JetBrains Rider instead
.\scripts\windows\setup\install-ide.ps1 -Install -Rider
```

```bash
# Linux — check only
bash scripts/linux/setup/install-ide.sh

# Linux — install JetBrains Rider (snap → flatpak → JetBrains Toolbox)
bash scripts/linux/setup/install-ide.sh --install
```

```zsh
# macOS — check only
zsh scripts/macos/setup/install-ide.zsh

# macOS — install JetBrains Rider via Homebrew cask
zsh scripts/macos/setup/install-ide.zsh --install
```

> **Note:** On Linux, `install-ide.sh` tries `snap` first, then `flatpak`, then falls back to
> downloading JetBrains Toolbox. On macOS, Homebrew must be installed first (`install-prereqs.zsh`).
> On Windows, the `-Rider` flag switches the target from Visual Studio 2026 to JetBrains Rider.

---

### `dev/` — Local development lifecycle

| Script        | Description                                                         |
|---------------|---------------------------------------------------------------------|
| `start`       | Launch the full stack via Aspire AppHost (dashboard: `http://localhost:15888`) |
| `stop`        | Gracefully terminate the AppHost and remove lingering containers    |
| `reset-data`  | ⚠️ Stop the stack and permanently delete all local data volumes     |

**Usage**

```powershell
# Windows
.\scripts\windows\dev\start.ps1           # foreground
.\scripts\windows\dev\start.ps1 -Detach   # background window
.\scripts\windows\dev\stop.ps1
.\scripts\windows\dev\reset-data.ps1 -Force
```

```bash
# Linux
bash scripts/linux/dev/start.sh
bash scripts/linux/dev/start.sh --detach
bash scripts/linux/dev/stop.sh
bash scripts/linux/dev/reset-data.sh --force
```

```zsh
# macOS
zsh scripts/macos/dev/start.zsh
zsh scripts/macos/dev/start.zsh --detach
zsh scripts/macos/dev/stop.zsh
zsh scripts/macos/dev/reset-data.zsh --force
```

---

### `docker/` — Image management

| Script      | Description                                                           |
|-------------|-----------------------------------------------------------------------|
| `build-all` | Build every `Dockerfile` in `containers/` and `src/backend/`, tagging each as `chishiki/<name>:latest` |
| `clean`     | Remove all `chishiki/*` images and prune dangling layers              |

**Usage**

```powershell
# Windows
.\scripts\windows\docker\build-all.ps1
.\scripts\windows\docker\build-all.ps1 -NoCache
.\scripts\windows\docker\build-all.ps1 -Filter ollama
.\scripts\windows\docker\clean.ps1 -Force
.\scripts\windows\docker\clean.ps1 -DanglingOnly
```

```bash
# Linux
bash scripts/linux/docker/build-all.sh
bash scripts/linux/docker/build-all.sh --no-cache
bash scripts/linux/docker/build-all.sh --filter ollama
bash scripts/linux/docker/clean.sh --force
bash scripts/linux/docker/clean.sh --dangling-only
```

```zsh
# macOS
zsh scripts/macos/docker/build-all.zsh
zsh scripts/macos/docker/build-all.zsh --no-cache
zsh scripts/macos/docker/build-all.zsh --filter ollama
zsh scripts/macos/docker/clean.zsh --force
zsh scripts/macos/docker/clean.zsh --dangling-only
```

---

## Script standards

All scripts in this repository follow these conventions:

### Required features

- **`--help` / `-Help`** — every script prints its synopsis and exits cleanly
- **ANSI color output** — consistent 6-helper pattern: `header` (box border), `step` (──), `ok` (✔ green), `warn` (⚠ yellow), `fail` (✖ red), `info` (· cyan)
- **Idempotent** — running a script multiple times produces the same result as running it once
- **Error handling** — scripts exit on first error (`set -euo pipefail` / `$ErrorActionPreference = 'Stop'`) with a descriptive message
- **Repo-root detection** — scripts resolve the repository root relative to their own path; they work correctly from any working directory

### Naming rules

- Same base name across all three platforms: `install-prereqs.ps1` ↔ `install-prereqs.sh` ↔ `install-prereqs.zsh`
- Same topic folder structure: `setup/`, `dev/`, `docker/`
- Use `kebab-case` for file names

### Adding a new script

1. Create the script in all three platform folders under the same topic subfolder
2. Use the same flags and same observable behavior on every platform
3. Copy the ANSI color helper block from an existing script in the same platform folder
4. Add it to the table above

### Line endings

`.gitattributes` enforces correct line endings automatically:

| Extension | Line ending |
|-----------|-------------|
| `.ps1`    | CRLF        |
| `.sh`     | LF          |
| `.zsh`    | LF          |
