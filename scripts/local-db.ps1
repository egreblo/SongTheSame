<#
  Starts a private PostgreSQL cluster for development (port 54329, trust auth, data in .localdb/).
  It does not touch any PostgreSQL service you already have installed.

  Usage:  powershell -File scripts/local-db.ps1            # initialise (first time) and start
          powershell -File scripts/local-db.ps1 -Stop      # stop it
#>
param([switch]$Stop)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$data = Join-Path $root '.localdb/data'
$bin = (Get-ChildItem 'C:\Program Files\PostgreSQL' -Directory | Sort-Object Name -Descending | Select-Object -First 1).FullName + '\bin'
$port = 54329

if ($Stop) {
    & "$bin\pg_ctl.exe" -D $data stop
    return
}

if (-not (Test-Path "$data\PG_VERSION")) {
    New-Item -ItemType Directory -Force (Split-Path $data) | Out-Null
    & "$bin\initdb.exe" -D $data -U postgres --auth=trust -E UTF8 --locale=C
}

# pg_ctl start waits on the console; launching postgres directly keeps this script non-blocking.
Start-Process -FilePath "$bin\postgres.exe" -ArgumentList "-D `"$data`" -p $port" -WindowStyle Hidden `
    -RedirectStandardError (Join-Path $root '.localdb/postgres.log')
Start-Sleep -Seconds 3

$exists = & "$bin\psql.exe" -h localhost -p $port -U postgres -tAc "select 1 from pg_database where datname='songthesame_dev'"
if (-not $exists) {
    & "$bin\psql.exe" -h localhost -p $port -U postgres -c 'create database songthesame_dev'
}

Write-Host "PostgreSQL is running on localhost:$port."
Write-Host "Connection string: Host=localhost;Port=$port;Database=songthesame_dev;Username=postgres"
