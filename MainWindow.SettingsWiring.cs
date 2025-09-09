using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DockTop.Models;
using DockTop.Services;

namespace DockTop
{
    public partial class MainWindow : Window
    {
        private SettingsService? _settingsSvc;
        private AutoHideManager? _autoHide;
        private Panel? _dockRoot;     // try to find a named root panel in your XAML
        private Panel? _itemsPanel;   // try to find items/WrapPanel of your dock icons

        private void EnsureRefs()
        {
            _dockRoot ??= this.FindName("DockRoot") as Panel;
            _itemsPanel ??= this.FindName("ItemsPanel") as Panel;
        }

        // Call this from your Settings button handler
        private void OpenSettings()
        {
            _settingsSvc ??= new SettingsService();
            var win = new SettingsWindow(_settingsSvc, ApplySettings);
            win.Owner = this;
            win.ShowDialog();
        }

        // Applies ALL settings live: theme, layout, auto-hide, basic window props
        private void ApplySettings(UserSettings s)
        {
            EnsureRefs();

            // theme
            ThemeApplier.ApplyTheme(s.ThemeId, this);

            // basic window properties
            this.Topmost = s.Topmost;
            this.Opacity = s.Opacity;
            this.MaxWidth = s.DockMaxWidth;

            // corner radius on a known border or root
            if (_dockRoot is Border b)
            {
                b.CornerRadius = new CornerRadius(s.CornerRadius);
                b.Background = (Brush)Application.Current.Resources["DockBackgroundBrush"];
            }
            else if (_dockRoot != null)
            {
                _dockRoot.Background = (Brush)Application.Current.Resources["DockBackgroundBrush"];
            }
            else
            {
                // fallback to window
                this.Background = (Brush)Application.Current.Resources["DockBackgroundBrush"];
            }

            // icon size & rows (generic: if your items panel is a WrapPanel)
            if (_itemsPanel is WrapPanel wrap)
            {
                wrap.ItemWidth = Math.Max(24, s.IconSize + 8);
                wrap.ItemHeight = s.IconSize + 16;
            }

            // TODO: If you use DataTemplates, bind Image/Path Width/Height to s.IconSize
            // TODO: If you grid icons manually, adjust rows here

            // animation speed (you likely have Storyboards — set their Duration from s.AnimationSpeed)

            // auto-hide
            _autoHide ??= new AutoHideManager(this);
            _autoHide.Configure(s.AutoHide, s.AutoHideDelayMs);
        }
    }
}