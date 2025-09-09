
DockTop — Full Wiring Patch (theme + settings + auto-hide)
=========================================================

This patch does two things:
1) Replaces the Settings placeholder with a real, persistent UI (with your chosen defaults).
2) Wires live application of **theme + layout + auto-hide** when you hit *Apply*.

What’s included
---------------
- Models/UserSettings.cs                 (updated defaults you chose)
- Services/SettingsService.cs            (persist to %APPDATA%\DockTop\user.settings.json)
- ViewModels/SettingsViewModel.cs
- Theming/ThemeResources.xaml            (DynamicResource keys)
- Services/ThemeApplier.cs               (applies gradient/accent/blur/glow to resources)
- Services/AutoHideManager.cs            (simple edge auto-hide with delay)
- SettingsWindow.xaml / .cs              (functional settings UI)
- MainWindow.SettingsWiring.cs           (partial class: OpenSettings + ApplySettings)

How to integrate
----------------
1) Add all the files in this patch to your project.
   - Put `Theming/ThemeResources.xaml` anywhere under the project.
2) Merge the theme resources into your app or main window:
   - In **App.xaml** (recommended) or **MainWindow.xaml**, add:

```
<Application.Resources>
  <ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
      <ResourceDictionary Source="Theming/ThemeResources.xaml" />
    </ResourceDictionary.MergedDictionaries>
  </ResourceDictionary>
</Application.Resources>
```

3) Ensure your XAML gives names that the wiring looks for (or update the code):
   - Root container: give it `x:Name="DockRoot"` (Border or Panel) so corner radius & background apply cleanly.
   - Icons container: if you use a `WrapPanel`, name it `x:Name="ItemsPanel"` (so icon size/rows apply).
   - If your names differ, tweak `EnsureRefs()` in `MainWindow.SettingsWiring.cs`.

4) Hook the Settings button to `OpenSettings();` and call `ApplySettings(_settingsSvc.Current);`
   once at startup (so defaults apply on first launch).

Notes
-----
- Theme brushes are exposed as `DynamicResource` keys: `DockBackgroundBrush`, `AccentBrush` etc.
- `ThemeApplier` updates those at runtime so the UI switches instantly when changing themes.
- `AutoHideManager` is intentionally simple. Swap/extend it with your `TriggerWindow` hover logic later.

