param([switch]$Apply,[switch]$WhatIf)
Set-StrictMode -Version Latest; $ErrorActionPreference="Stop"
function Backup-File($p,$b){ if(Test-Path $p){ $d=Join-Path $b (Split-Path $p -Leaf); Copy-Item $p $d -Force } }
$root = Get-Location
$templates = Join-Path $root "Templates"
$backupDir = Join-Path $root ("_repair_backups\" + (Get-Date).ToString("yyyyMMdd_HHmmss"))
New-Item -ItemType Directory -Force -Path $backupDir | Out-Null

function Replace-IfBrokenXaml { param($file,$patterns,$tmpl)
  if(!(Test-Path $file)){ return }
  $raw = Get-Content $file -Raw
  $broken = $false
  foreach($p in $patterns){ if($raw -match $p){ $broken=$true; break } }
  if( ($raw | Select-String 'x:Name\s*=\s*"TileList"' -AllMatches).Matches.Count -gt 1 ){ $broken=$true }
  if($broken){
    Write-Host "[FIX] Replacing $file with canonical template" -ForegroundColor Yellow
    if($Apply){ Backup-File $file $backupDir; Copy-Item (Join-Path $templates $tmpl) $file -Force }
  }
}

Replace-IfBrokenXaml -file ".\MainWindow.xaml" -patterns @("\.ItemsPanel","\.ItemTemplate") -tmpl "MainWindow.xaml"

$mwcs = ".\MainWindow.xaml.cs"
if(Test-Path $mwcs){
  $txt = Get-Content $mwcs -Raw
  $open = ($txt.ToCharArray() | ? {$_ -eq "{"}).Count
  $close = ($txt.ToCharArray() | ? {$_ -eq "}"}).Count
  $topElse = $txt -match "^\s*else\s" -or $txt -match "Top-level statements"
  if($open -ne $close -or $topElse){
    Write-Host "[FIX] Replacing MainWindow.xaml.cs (unbalanced/invalid)" -ForegroundColor Yellow
    if($Apply){ Backup-File $mwcs $backupDir; Copy-Item (Join-Path $templates "MainWindow.xaml.cs") $mwcs -Force }
  }
}

$swx = ".\Views\SettingsWindow.xaml"
if(Test-Path $swx){
  $raw = Get-Content $swx -Raw
  $names = "CmbEdge","SldThH","SldThV","TxtAnimIn","TxtAnimOut"
  $hasDupes = $false
  foreach($n in $names){ if(([regex]::Matches($raw,"x:Name\s*=\s*`"$n`"").Count) -gt 1){ $hasDupes=$true } }
  if($hasDupes){
    Write-Host "[FIX] Replacing Behavior section in SettingsWindow.xaml" -ForegroundColor Yellow
    if($Apply){
      Backup-File $swx $backupDir
      $canon = Get-Content (Join-Path $templates "BehaviorBlock.xaml") -Raw
      $pattern = '(?s)<TextBlock\s+Text="Behavior"[\s\S]*?(?=<TextBlock\s+Text="Plugins"|<TextBlock\s+Text="Hotkeys"|</Grid>|</Window>)'
      $new = [regex]::Replace($raw, $pattern, $canon)
      Set-Content -Path $swx -Value $new -Encoding UTF8
    }
  }
}

$map = @{ ".\Utils\HotKeyManager.cs"="HotKeyManager.cs"; ".\Utils\WindowStyles.cs"="WindowStyles.cs"; ".\Utils\IconExtractor.cs"="IconExtractor.cs"; ".\Utils\MonitorHelper.cs"="MonitorHelper.cs" }
foreach($kv in $map.GetEnumerator()){
  $dst=$kv.Key; $src=Join-Path $templates $kv.Value
  if(!(Test-Path $dst)){
    Write-Host "[ADD] $dst" -ForegroundColor Cyan
    if($Apply){ New-Item -ItemType Directory -Force -Path (Split-Path $dst) | Out-Null; Copy-Item $src $dst -Force }
  }
}

$us = ".\Models\UserSettings.cs"
$props = "HotkeyToggle","HotkeySearch","DockEdge","ThicknessHorizontal","ThicknessVertical","RevealAnimationMs","HideAnimationMs","TilesPerPage","ClickThroughWhenHidden","AutoStartWithWindows","Profile"
if(Test-Path $us){
  $raw = Get-Content $us -Raw; $changed=$false
  foreach($p in $props){
    $matches = [regex]::Matches($raw, "public\s+[^\r\n]+?\s+$p\s*{[^}]+}")
    if($matches.Count -gt 1){
      Write-Host "[FIX] Removing duplicate property $p in UserSettings.cs" -ForegroundColor Yellow
      for($i=1;$i -lt $matches.Count;$i++){ $raw = $raw.Remove($matches[$i].Index,$matches[$i].Length); $changed=$true }
    }
  }
  if($changed -and $Apply){ Backup-File $us $backupDir; Set-Content -Path $us -Value $raw -Encoding UTF8 }
}

$proj = Get-ChildItem -Filter "DockTop.csproj" | Select-Object -First 1
if($proj){
  $xml = Get-Content $proj.FullName -Raw
  $isSdk = $xml -match '<Project\s+Sdk='
  if(-not $isSdk -and $xml -notmatch 'Utils\\HotKeyManager.cs'){
    Write-Host "[FIX] Adding <Compile Include> entries to $($proj.Name)" -ForegroundColor Yellow
    if($Apply){
      $inject = @"
  <ItemGroup>
    <Compile Include="Utils\HotKeyManager.cs" />
    <Compile Include="Utils\WindowStyles.cs" />
    <Compile Include="Utils\IconExtractor.cs" />
    <Compile Include="Utils\MonitorHelper.cs" />
    <Compile Include="Services\AutoStartService.cs" />
  </ItemGroup>
</Project>
"@
      $xml = $xml -replace '(?s)</Project>', $inject
      Set-Content -Path $proj.FullName -Value $xml -Encoding UTF8
    }
  }
}

$dockItem = Get-ChildItem -Recurse -Include "*DockItem*.cs" | Select-Object -First 1
if($dockItem -and (Test-Path $mwcs)){
  $ns = (Select-String -Path $dockItem.FullName -Pattern 'namespace\s+([A-Za-z0-9_.]+)' -AllMatches).Matches | Select-Object -First 1
  if($ns){
    $nsName = $ns.Groups[1].Value
    $mwRaw = Get-Content $mwcs -Raw
    if($mwRaw -notmatch [regex]::Escape("using $nsName;")){
      Write-Host "[ADD] using $nsName; in MainWindow.xaml.cs" -ForegroundColor Cyan
      if($Apply){
        Backup-File $mwcs $backupDir
        $mwRaw = $mwRaw -replace '(?s)namespace\s+DockTop', "using $nsName;`r`nnamespace DockTop"
        Set-Content -Path $mwcs -Value $mwRaw -Encoding UTF8
      }
    }
  }
}

Write-Host ""
Write-Host "Done. Backups: $backupDir" -ForegroundColor Green
if(-not $Apply){ Write-Host "(WhatIf) No changes were applied - run again with -Apply" -ForegroundColor Yellow }
