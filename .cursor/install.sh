#!/usr/bin/env bash
# Idempotent bootstrap for the ATS / TaskManagementSystem dev environment.
# Installs system toolchains (.NET 10 SDK, SQL Server 2022 + tools) and restores
# / builds the solutions. Runs after the repository is checked out. Safe to
# re-run: every step is guarded so it converges instead of duplicating state.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DOTNET_ROOT="/usr/share/dotnet"

log() { echo "[install] $*"; }

# --- .NET 10 SDK -----------------------------------------------------------
if ! command -v dotnet >/dev/null 2>&1; then
  log "Installing .NET 10 SDK..."
  curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
  chmod +x /tmp/dotnet-install.sh
  sudo mkdir -p "$DOTNET_ROOT"
  sudo /tmp/dotnet-install.sh --channel 10.0 --install-dir "$DOTNET_ROOT"
  sudo ln -sf "$DOTNET_ROOT/dotnet" /usr/local/bin/dotnet
else
  log ".NET SDK already present: $(dotnet --version)"
fi
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1

# --- SQL Server 2022 + command-line tools ----------------------------------
if [ ! -x /opt/mssql/bin/sqlservr ]; then
  log "Installing SQL Server 2022 and tools..."
  curl -fsSL https://packages.microsoft.com/keys/microsoft.asc \
    | sudo tee /etc/apt/trusted.gpg.d/microsoft.asc >/dev/null
  curl -fsSL https://packages.microsoft.com/config/ubuntu/22.04/mssql-server-2022.list \
    | sudo tee /etc/apt/sources.list.d/mssql-server-2022.list >/dev/null
  curl -fsSL https://packages.microsoft.com/config/ubuntu/22.04/prod.list \
    | sudo tee /etc/apt/sources.list.d/mssql-release.list >/dev/null
  sudo apt-get update -y
  sudo ACCEPT_EULA=Y apt-get install -y mssql-server mssql-tools18 unixodbc-dev
else
  log "SQL Server already installed."
fi

# SQL Server 2022 links against OpenLDAP 2.5, which Ubuntu 24.04 (noble) no
# longer ships (it has 2.6). Provide the 2.5 runtime libs from the 22.04 package.
if [ ! -e /usr/lib/x86_64-linux-gnu/liblber-2.5.so.0 ]; then
  log "Installing OpenLDAP 2.5 runtime libraries for SQL Server..."
  tmp="$(mktemp -d)"
  curl -fsSL -o "$tmp/libldap25.deb" \
    "http://archive.ubuntu.com/ubuntu/pool/main/o/openldap/libldap-2.5-0_2.5.16+dfsg-0ubuntu0.22.04.2_amd64.deb"
  dpkg-deb -x "$tmp/libldap25.deb" "$tmp/extract"
  sudo cp -av "$tmp/extract/usr/lib/x86_64-linux-gnu/"lib*-2.5.so.0* /usr/lib/x86_64-linux-gnu/
  sudo ldconfig
  rm -rf "$tmp"
fi

# --- TaskManagementSystem (modular monolith, .NET 10) ----------------------
log "Building TaskManagementSystem.slnx..."
dotnet build "$REPO_ROOT/TaskManagementSystem/TaskManagementSystem.slnx" -c Debug

# --- Legacy AutomatedTaskSystem API (.NET 10) ------------------------------
log "Building legacy AutomatedTaskSystem API..."
dotnet build "$REPO_ROOT/AutomatedTaskSystem/AutomatedTaskSystem.csproj" -c Debug

# --- Next.js UI client -----------------------------------------------------
# react-date-range pins date-fns 3.x while the app resolves 4.x; --legacy-peer-deps
# is required for the install to succeed.
log "Installing Next.js UI dependencies..."
( cd "$REPO_ROOT/AutomatedTaskSystem.UI/client" && npm install --legacy-peer-deps )

log "Install complete."
