# DockTop — Full Upgraded Build (with Now Playing)

This package includes:
- Mica/Acrylic toggle (Settings)
- Corner radius 18, glow opacity slider
- Icon sizes 24 / 32 / 50
- Always-on-top, Auto-hide with 1px peek
- Hotkeys: Ctrl+Up (toggle), Ctrl+K (Quick Search)
- Two live “+ Add” slots (click to add EXE/LNK, persisted)
- Tiny **Now Playing** tile (Windows media session)
- Minimal Quick Search overlay (Start Menu match → else Google)
- Startup helper stub (no COM requirements)

## Build
1. Install .NET 8 SDK.
2. From the folder with `DockTop*.csproj`:
```
dotnet restore
dotnet build -c Release
dotnet run -c Release
```
> The project references `Microsoft.Windows.CsWinRT` to access Windows.Media.Control for Now Playing. NuGet restore is required (internet). If restore is blocked, set `NowPlaying=false` in `%APPDATA%\DockTop\user.settings.json` to hide the tile.

## Settings / Data
- Settings: `%APPDATA%\DockTop\user.settings.json`
- Items: `%APPDATA%\DockTop\dock.items.json`

Pre-seeded pins:
- PowerShell 7
- Visual Studio Code
- Google Drive

## Notes
- If Now Playing shows “Not playing”, ensure some media is active (Spotify, YouTube, etc.).
- Search overlay tries Start Menu matches first, otherwise opens Google search.
