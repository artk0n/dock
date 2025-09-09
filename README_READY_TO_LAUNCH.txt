DockTop — Ready-To-Launch (FULL + Click-to-Launch)
==================================================

Everything wired:
- Designer (tabs/groups, drag-drop, Start Menu scan, Browse…, import/export)
- Tabs in dock (live filter)
- Pressed-in tile buttons
- Click-to-launch: exe/lnk/bat (+ arguments for exe/bat), optional RunAsAdmin, WorkingDir
- Auto-hide via TriggerWindow (top edge by default) + remember last edge
- Theme system with animated gradient + tokens
- Hotkey toggle (window-scoped Ctrl+Alt+D)
- Toast bar for non-fatal errors

Quick start
-----------
1) Add all files to your WPF project (keep folder structure).
2) Place themes.json beside your built DockTop.exe.
3) Build and run. Use the Designer to add items/tabs.

Notes
-----
- Global hotkey: swap HotkeyManager for Win32 RegisterHotKey if desired.
- Launch errors show in the toast bar at the bottom of the dock.
- Items/settings persist under %APPDATA%\DockTop.
