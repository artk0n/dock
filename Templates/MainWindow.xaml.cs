using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using DockTop.Utils;

namespace DockTop
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<DockItem> DisplayItems { get; } = new();

        private bool _hidden;
        private System.Windows.Threading.DispatcherTimer? _edgeTimer;
        private GlobalHotkeys? _hotkeys;
        private int _hkToggleId, _hkSearchId;
        private int _pageIndex = 0;

        public MainWindow()
        {
            InitializeComponent();
            PreviewMouseWheel += OnMouseWheelScroll;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            try { Settings.Current.Profile.ScreenId = MonitorHelper.GetPrimaryId(); } catch {}
            try {
                _hotkeys = new GlobalHotkeys(this);
                _hkToggleId = _hotkeys.Bind(Settings.Current.HotkeyToggle ?? "Ctrl+Alt+D", ToggleDock);
                _hkSearchId = _hotkeys.Bind(Settings.Current.HotkeySearch ?? "Ctrl+K", () => BtnSearch_Click(this, new RoutedEventArgs()));
            } catch {}
            ApplyEdgeLayout();
            BuildItems();
            if (Settings.Current.AutoHide) {
                HideDock();
                _edgeTimer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
                _edgeTimer.Tick += (_, __) => EdgePeekCheck();
                _edgeTimer.Start();
            } else {
                ShowDock();
            }
        }

        private void ApplyEdgeLayout()
        {
            var sw = SystemParameters.PrimaryScreenWidth;
            var sh = SystemParameters.PrimaryScreenHeight;
            if (Settings.Current.DockEdge == "Left" || Settings.Current.DockEdge == "Right") { Width = Math.Max(44, Settings.Current.ThicknessVertical); Height = sh; }
            else { Width = sw; Height = Math.Max(44, Settings.Current.ThicknessHorizontal); }
            ShowDock();
        }

        private void EdgePeekCheck()
        {
            var peek = Math.Max(0, Settings.Current.AutoHidePeekPx);
            var sw = SystemParameters.PrimaryScreenWidth;
            var sh = SystemParameters.PrimaryScreenHeight;
            GetCursorPos(out var p);
            bool shouldShow = Settings.Current.DockEdge switch {
                "Top" => p.y <= peek,
                "Bottom" => p.y >= (sh - 1 - peek),
                "Left" => p.x <= peek,
                "Right" => p.x >= (sw - 1 - peek),
                _ => false
            };
            if (shouldShow) ShowDock();
            else {
                var over = p.x >= Left && p.x <= Left + Width && p.y >= Top && p.y <= Top + Height;
                if (!over) HideDock();
            }
        }

        private void AnimateTo(DependencyProperty prop, double to, int ms)
        {
            var anim = new DoubleAnimation { To = to, Duration = new Duration(TimeSpan.FromMilliseconds(ms)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            BeginAnimation(prop, anim);
        }

        private void ShowDock()
        {
            _hidden = false;
            WindowStyles.SetClickThrough(this, false);
            var edge = Settings.Current.DockEdge;
            var sw = SystemParameters.PrimaryScreenWidth;
            var sh = SystemParameters.PrimaryScreenHeight;
            if (edge == "Top") { Left = 0; Width = sw; AnimateTo(Window.TopProperty, 0, Settings.Current.RevealAnimationMs); }
            else if (edge == "Bottom") { Left = 0; Width = sw; AnimateTo(Window.TopProperty, sh - Height, Settings.Current.RevealAnimationMs); }
            else if (edge == "Left") { Top = 0; Height = sh; AnimateTo(Window.LeftProperty, 0, Settings.Current.RevealAnimationMs); }
            else { Top = 0; Height = sh; AnimateTo(Window.LeftProperty, sw - Width, Settings.Current.RevealAnimationMs); }
            AnimateTo(Window.OpacityProperty, 0.98, Settings.Current.RevealAnimationMs);
        }

        private void HideDock()
        {
            _hidden = true;
            if (Settings.Current.ClickThroughWhenHidden) WindowStyles.SetClickThrough(this, true);
            var edge = Settings.Current.DockEdge;
            var sw = SystemParameters.PrimaryScreenWidth;
            var sh = SystemParameters.PrimaryScreenHeight;
            var peek = Math.Max(0, Settings.Current.AutoHidePeekPx);
            if (edge == "Top") { Left = 0; Width = sw; AnimateTo(Window.TopProperty, -(Height - peek), Settings.Current.HideAnimationMs); }
            else if (edge == "Bottom") { Left = 0; Width = sw; AnimateTo(Window.TopProperty, sh - peek, Settings.Current.HideAnimationMs); }
            else if (edge == "Left") { Top = 0; Height = sh; AnimateTo(Window.LeftProperty, -(Width - peek), Settings.Current.HideAnimationMs); }
            else { Top = 0; Height = sh; AnimateTo(Window.LeftProperty, sw - peek, Settings.Current.HideAnimationMs); }
            AnimateTo(Window.OpacityProperty, 0.001, Settings.Current.HideAnimationMs);
        }

        private void ToggleDock() { if (_hidden) ShowDock(); else HideDock(); }

        private void BuildItems()
        {
            int per = Settings.Current.TilesPerPage;
            if (per <= 0) { double w = this.ActualWidth > 0 ? this.ActualWidth : SystemParameters.PrimaryScreenWidth;
                per = Math.Max(6, (int)Math.Floor((w - 80) / 88.0)); }
            var all = Items.All.Where(i => !i.Disabled).ToList();
            int total = all.Count;
            int pages = Math.Max(1, (int)Math.Ceiling(total / (double)per));
            _pageIndex = Math.Clamp(_pageIndex, 0, pages - 1);
            var pageItems = all.Skip(_pageIndex * per).Take(per).ToList();
            DisplayItems.Clear();
            foreach (var it in pageItems) DisplayItems.Add(it);
            DisplayItems.Add(new DockItem { Title = "+ Add", AddSlot = true });
            DisplayItems.Add(new DockItem { Title = "+ Add", AddSlot = true });
            RebuildPageDots(pages);
        }

        private void RebuildPageDots(int pages)
        {
            if (PART_PageDots == null) return;
            PART_PageDots.Children.Clear();
            for (int i = 0; i < pages; i++) {
                var b = new Button { Content = (i == _pageIndex ? "●" : "○"),
                    Margin = new Thickness(3, 0, 3, 0), Padding = new Thickness(0) };
                int idx = i;
                b.Click += (_, __) => { _pageIndex = idx; BuildItems(); };
                PART_PageDots.Children.Add(b);
            }
        }

        private void OnMouseWheelScroll(object sender, MouseWheelEventArgs e)
        {
            int per = Math.Max(1, Settings.Current.TilesPerPage <= 0 ? 10 : Settings.Current.TilesPerPage);
            int total = Items.All.Count;
            int pages = Math.Max(1, (int)Math.Ceiling(total / (double)per));
            if (pages <= 1) return;
            _pageIndex = Math.Clamp(_pageIndex + (e.Delta < 0 ? 1 : -1), 0, pages - 1);
            BuildItems(); e.Handled = true;
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            var w = new SettingsWindow { Owner = this }; w.ShowDialog();
            Topmost = Settings.Current.Topmost; Height = DockHeight;
            if (Settings.Current.AutoHide && !_hidden) HideDock();
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e) { BtnSettings_Click(sender, e); }
        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

        private void DockItem_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is DockItem it) {
                if (it.AddSlot) { AddAppViaDialog(); return; }
                try { System.Diagnostics.Process.Start(
                        new System.Diagnostics.ProcessStartInfo(it.Path) { UseShellExecute = true }); } catch {}
            }
        }

        public void AddAppViaDialog()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog { Title = "Choose app (.exe or .lnk)",
                Filter = "Apps|*.exe;*.lnk|All|*.*" };
            if (dlg.ShowDialog(this) == true) { Items.AddFromPath(dlg.FileName); Items.Save(); BuildItems(); }
        }

        [DllImport("user32.dll")] static extern bool GetCursorPos(out POINT lpPoint);
        public struct POINT { public int X; public int Y; public int x => X; public int y => Y; }

        protected override void OnClosed(EventArgs e)
        { try { _hotkeys?.Dispose(); } catch {} base.OnClosed(e); }
    }
}
