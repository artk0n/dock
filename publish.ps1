param(
  [ValidateSet("FDD","SCD")][string]$Mode = "FDD"
)

$ErrorActionPreference = "Stop"

# Pick profile
$profile = if ($Mode -eq "SCD") {
  "Properties\PublishProfiles\SingleFile_SCD.pubxml"
} else {
  "Properties\PublishProfiles\SingleFile_FDD.pubxml"
}

Write-Host "Publishing DockTop Pro ($Mode) using $profile ..." -ForegroundColor Cyan
dotnet restore
dotnet publish -p:PublishProfile="$profile"

# Find output
$tfm = "net8.0-windows"
$rid = "win-x64"
$out = Join-Path "bin\Release\$tfm\$rid\publish" ""
Write-Host "`nOutput:" -ForegroundColor Green
Get-ChildItem $out | Format-Table Name,Length,LastWriteTime

Write-Host "`nDone. Launch from:" -ForegroundColor Yellow
Write-Host (Join-Path $out "DockTopPro.exe")

# Added single-file self-contained publish
param([string]$Mode = 'SCD')
if ($Mode -eq 'SCD') { dotnet publish DockTop.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true }
if ($Mode -eq 'FDD') { dotnet publish DockTop.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:PublishTrimmed=true }
