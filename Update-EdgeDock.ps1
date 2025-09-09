<# 
Update-EdgeDock.ps1
Lightweight patcher for source-level updates without re-zipping the whole project.
- Apply a "partial patch" zip (just changed files) on top of your working copy.
- Validates with a patch-manifest.json (optional but recommended).
- Makes a timestamped backup and supports rollback.
- Dry-run supported.
Usage examples:
  # Apply from a local zip
  .\Update-EdgeDock.ps1 -PatchZip 'C:\Downloads\edgedock_patch_2025-09-09.zip'
  # Apply from URL
  .\Update-EdgeDock.ps1 -Url 'https://example.com/edgedock_patch_2025-09-09.zip'
  # Dry run only
  .\Update-EdgeDock.ps1 -PatchZip .\patch.zip -WhatIf
  # Rollback last backup
  .\Update-EdgeDock.ps1 -Rollback
#>

param(
  [string]$PatchZip,
  [string]$Url,
  [switch]$WhatIf,
  [switch]$Rollback
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-Hash256($Path) {
  if (!(Test-Path $Path)) { return $null }
  return (Get-FileHash -Path $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Write-Info($msg) { Write-Host "[INFO] $msg" -ForegroundColor Cyan }
function Write-Warn($msg) { Write-Host "[WARN] $msg" -ForegroundColor Yellow }
function Write-Err ($msg) { Write-Host "[ERR ] $msg" -ForegroundColor Red }

$projectRoot = (Get-Location).Path
$patchTemp   = Join-Path $env:TEMP ("edgedock_patch_" + [guid]::NewGuid())
$backupsDir  = Join-Path $projectRoot "_patch_backups"

if ($Rollback) {
  if (!(Test-Path $backupsDir)) { Write-Err "No backups folder found."; exit 1 }
  $last = Get-ChildItem $backupsDir -Directory | Sort-Object LastWriteTime -Descending | Select-Object -First 1
  if (!$last) { Write-Err "No backup to restore."; exit 1 }
  Write-Info "Restoring backup: $($last.FullName)"
  if ($WhatIf) { Write-Info "(WhatIf) Would restore files from backup"; exit 0 }
  Copy-Item -Path (Join-Path $last.FullName "*") -Destination $projectRoot -Recurse -Force
  Write-Info "Rollback complete."
  exit 0
}

if (-not $PatchZip -and -not $Url) {
  Write-Err "Provide -PatchZip or -Url."
  exit 1
}
if ($Url) {
  $PatchZip = Join-Path $env:TEMP ("edgedock_download_" + [guid]::NewGuid() + ".zip")
  Write-Info "Downloading patch from $Url"
  Invoke-WebRequest -Uri $Url -OutFile $PatchZip
}

New-Item -ItemType Directory -Force -Path $patchTemp | Out-Null

Write-Info "Extracting patch to temp..."
Expand-Archive -Path $PatchZip -DestinationPath $patchTemp -Force

# Look for a manifest (recommended). If missing, we still proceed without hash checks.
$manifestPath = Get-ChildItem -Path $patchTemp -Recurse -Filter "patch-manifest.json" | Select-Object -First 1
$manifest = $null
if ($manifestPath) {
  $manifest = Get-Content $manifestPath.FullName -Raw | ConvertFrom-Json
  Write-Info "Manifest found: $($manifestPath.FullName)"
}

# Compute a list of files inside the patch folder (ignoring manifest)
$patchFiles = Get-ChildItem -Path $patchTemp -Recurse -File | Where-Object { $_.Name -ne "patch-manifest.json" }

# Preview changes
$changes = @()
foreach ($f in $patchFiles) {
  $rel = $f.FullName.Substring($patchTemp.Length + 1)
  $dest = Join-Path $projectRoot $rel
  $exists = Test-Path $dest
  $oldHash = if ($exists) { Get-Hash256 $dest } else { $null }
  $newHash = Get-Hash256 $f.FullName
  $status = if (-not $exists) { "Add" } elseif ($oldHash -ne $newHash) { "Update" } else { "Same" }
  # Validate manifest if present
  if ($manifest -and $manifest.files) {
    $m = $manifest.files | Where-Object { $_.path -eq $rel }
    if ($m) {
      if ($m.sha256 -and ($m.sha256.ToLower() -ne $newHash)) {
        Write-Err "Hash mismatch for $rel (manifest vs patch)."
        exit 1
      }
    }
  }
  $changes += [pscustomobject]@{ Path=$rel; Status=$status }
}

Write-Info "Planned changes:"
$changes | Format-Table -AutoSize

if ($WhatIf) { Write-Info "(WhatIf) No changes applied."; exit 0 }

# Backup
New-Item -ItemType Directory -Force -Path $backupsDir | Out-Null
$stamp = (Get-Date).ToString("yyyyMMdd_HHmmss")
$backup = Join-Path $backupsDir $stamp
New-Item -ItemType Directory -Force -Path $backup | Out-Null

Write-Info "Backing up changed files to: $backup"
foreach ($c in $changes) {
  if ($c.Status -eq "Same") { continue }
  $src = Join-Path $projectRoot $c.Path
  if (Test-Path $src) {
    $backupPath = Join-Path $backup $c.Path
    New-Item -ItemType Directory -Force -Path (Split-Path $backupPath) | Out-Null
    Copy-Item -Path $src -Destination $backupPath -Force
  }
}

# Apply
Write-Info "Applying patch..."
foreach ($f in $patchFiles) {
  $rel = $f.FullName.Substring($patchTemp.Length + 1)
  $dest = Join-Path $projectRoot $rel
  New-Item -ItemType Directory -Force -Path (Split-Path $dest) | Out-Null
  Copy-Item -Path $f.FullName -Destination $dest -Force
}

Write-Info "Patch applied. You can rollback with: .\\Update-EdgeDock.ps1 -Rollback"
Remove-Item -Recurse -Force $patchTemp -ErrorAction SilentlyContinue
if ($Url) { Remove-Item -Force $PatchZip -ErrorAction SilentlyContinue }
