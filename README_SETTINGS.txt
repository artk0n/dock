
DockTop — Settings Panel Patch (real UI + persistence)
======================================================

This patch replaces the "Settings panel placeholder" popup with a functional WPF window that:
- Loads/saves **user.settings.json** (in %APPDATA%\DockTop, fallback next to EXE)
- Lets you configure: Theme, Icon Size, Animation Speed, Auto‑hide (+ delay), Rows, Max Width, Opacity, Corner Radius, Topmost, Toggle Hotkey
- Notifies the host window through an `Apply` callback so you can immediately apply changes

Files included
--------------
- Models/UserSettings.cs
- Services/SettingsService.cs
- ViewModels/SettingsViewModel.cs
- SettingsWindow.xaml
- SettingsWindow.xaml.cs

How to integrate
----------------
1) Add all five files to your project.
2) Wire it from your Settings button (remove the old MessageBox placeholder). Example:

```csharp
// In MainWindow.xaml.cs (or wherever the Settings button is handled)
using DockTop.Models;
using DockTop.Services;

private SettingsService? _settingsSvc;

private void OpenSettings()
{
    _settingsSvc ??= new SettingsService();
    var win = new SettingsWindow(_settingsSvc, applyCallback: ApplySettings);
    win.Owner = this;
    win.ShowDialog();
}

// This gets called when the user hits Apply/Save in the Settings window
private void ApplySettings(UserSettings s)
{
    // Theme application example
    var theme = ThemeStore.GetById(s.ThemeId);
    this.Topmost = s.Topmost;
    this.Opacity = s.Opacity;

    // TODO: wire these into your dock layout logic:
    //  - s.IconSize       → scale dock icons
    //  - s.Rows           → grid rows for items
    //  - s.DockMaxWidth   → max window width or ItemsControl constraint
    //  - s.AnimationSpeed → duration on hover/slide animations
    //  - s.AutoHide + s.AutoHideDelayMs → your TriggerWindow logic
    //  - s.CornerRadius   → panel container style
    //  - s.HotkeyToggle   → hotkey registration manager
}
```

3) Replace the placeholder call with `OpenSettings();` in your Settings command handler.

Known placeholders we found in your tree
----------------------------------------
- **Settings popup**: `MessageBox.Show("Settings panel placeholder…")` — replaced by this patch.
- **ThemeStore.cs**: was an empty stub. Use our robust one from the hotfix, or fill it out.
- **TriggerWindow.xaml**: header is present but no logic; you likely intend to use it for top-edge auto-hide trigger.
- **Hotkeys**: `MainWindow` references a `HotkeyManager` but details weren’t present; confirm you register/unregister on apply.
- **Dock layout scaling**: Ensure icon size / rows apply to your ItemsControl/WrapPanel and image size bindings.

Optional hardening
------------------
Add global crash handlers in `App.xaml.cs` so startup errors never silently kill the app (see our previous hotfix README).

