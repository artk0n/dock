using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using DockTop.Models;
using DockTop.Services;

namespace DockTop
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public SettingsService Settings { get; }
        public DockItemsService Items { get; }
        public ObservableCollection<DockItem> DisplayItems { get; } = new();

        // Screen width; we want full-width like a taskbar
        public double DockWidth => SystemParameters.PrimaryScreenWidth;

        // Taskbar-like height: icon (24/32/50) + padding, clamped 44..64
        public double DockHeight
        {
            get
            {
                int icon = Settings.Current.IconSize;
                double h = icon + 20;          // 10px top/bottom padding
                if (h < 44) h = 44;
                if (h > 64) h = 64;
                return h;
            }
        }

        private bool _hidden;
        private GlobalHotkeys? _hotkeys;
        private int _hkToggleId, _hkSearchId;
        private DispatcherTimer? _edgeTimer;

        // Hotkeys
        private const int HOTKEY_ID_TOGGLE = 1;
        private const int HOTKEY_ID_SEARCH = 2;
        private const uint MOD_CONTROL = 0x0002;
        private const uint VK_UP = 0x26;
        private const uint VK_K = 0x4B;

        [DllImport("user32.dll")] static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")] static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public MainWindow()
        {
            Settings = new SettingsService();
            Items = new DockItemsService();

            // bind BEFORE InitializeComponent so Height/Width bindings resolve
            DataContext = this;
            InitializeComponent();

            // Hook drag events for reordering
            TileList.PreviewMouseLeftButtonDown += OnTileMouseDown;
            TileList.MouseMove += OnTileMouseMove;
            TileList.Drop += OnTileDrop;
            TileList.DragOver += (s,e)=> e.Effects = System.Windows.DragDropEffects.Move;


            if (Settings.Current.AutoHide)
            {
                // Apply initial layout and start hidden just off the chosen edge
                ApplyEdgeLayout();
                HideDock();
                _edgeTimer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) }
;
                _edgeTimer.Tick += (_, __) => EdgePeekCheck();
                _edgeTimer.Start();
            }

            BuildItems();
            RebuildPageDots();
Topmost = Settings.Current.Topmost;
            DwmBackdrop.Apply(this, Settings.Current.Backdrop);
        }

        private (int x, int y) GetCursorPos()
        {
            POINT p; GetCursorPos(out p); return (p.X, p.Y);
        }
        [DllImport("user32.dll")] static extern bool GetCursorPos(out POINT lpPoint);
        public struct POINT { public int X; public int Y; public int x => X; public int y => Y; }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var h = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(h).AddHook(WndProc);
            RegisterHotKey(h, HOTKEY_ID_TOGGLE, MOD_CONTROL, VK_UP);
            RegisterHotKey(h, HOTKEY_ID_SEARCH, MOD_CONTROL, VK_K);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                if (id == HOTKEY_ID_TOGGLE) ToggleDock();
                else if (id == HOTKEY_ID_SEARCH) BtnSearch_Click(this, new RoutedEventArgs());
                handled = true;
            }
            return IntPtr.Zero;
        }

        private 
        void BuildItems()
        {
            // auto-calc per page if not set (>0 uses explicit)
            int per = Settings.Current.TilesPerPage;
            if (per <= 0)
            {
                // estimate based on window width and tile cell ~88px
                double w = this.ActualWidth > 0 ? this.ActualWidth : SystemParameters.PrimaryScreenWidth;
                per = System.Math.Max(6, (int)System.Math.Floor((w - 80) / 88.0));
            }

            var all = new System.Collections.Generic.List<DockItem>(Items.All.Where(i => !i.Disabled));
            int total = all.Count;
            int pages = System.Math.Max(1, (int)System.Math.Ceiling(total / (double)per));
            _pageIndex = System.Math.Clamp(_pageIndex, 0, pages - 1);

            var pageItems = all.Skip(_pageIndex * per).Take(per).ToList();

            DisplayItems.Clear();
            foreach (var it in pageItems) DisplayItems.Add(it);

            // two add slots
            DisplayItems.Add(new DockItem { Title = "+ Add", AddSlot = true });
            DisplayItems.Add(new DockItem { Title = "+ Add", AddSlot = true });
        }
    );
            DisplayItems.Add(new DockItem { Title = "+ Add", AddSlot = true });
        }

        private void ToggleDock() { if (_hidden) ShowDock(); else HideDock(); }

        private void HideDock()
        {
            _hidden = true;
            if (Settings.Current.ClickThroughWhenHidden) DockTop.Utils.WindowStyles.SetClickThrough(this, true);
            var edge = Settings.Current.DockEdge;
            var sw = SystemParameters.PrimaryScreenWidth;
            var sh = SystemParameters.PrimaryScreenHeight;
            var peek = System.Math.Max(0, Settings.Current.AutoHidePeekPx);
            if (edge == "Top") { Left = 0; Width = sw; AnimateTo(Window.TopProperty, -(Height - peek), Settings.Current.HideAnimationMs); }
            else if (edge == "Bottom") { Left = 0; Width = sw; AnimateTo(Window.TopProperty, sh - peek, Settings.Current.HideAnimationMs); }
            else if (edge == "Left") { Top = 0; Height = sh; AnimateTo(Window.LeftProperty, -(Width - peek), Settings.Current.HideAnimationMs); }
            else { Top = 0; Height = sh; AnimateTo(Window.LeftProperty, sw - peek, Settings.Current.HideAnimationMs); }
            AnimateTo(Window.OpacityProperty, 0.001, Settings.Current.HideAnimationMs);
        }
        }
        }

        private void ShowDock()
        {
            _hidden = false;
            DockTop.Utils.WindowStyles.SetClickThrough(this, false);
            var edge = Settings.Current.DockEdge;
            var sw = SystemParameters.PrimaryScreenWidth;
            var sh = SystemParameters.PrimaryScreenHeight;
            if (edge == "Top") { Left = 0; Width = sw; AnimateTo(Window.TopProperty, 0, Settings.Current.RevealAnimationMs); }
            else if (edge == "Bottom") { Left = 0; Width = sw; AnimateTo(Window.TopProperty, sh - Height, Settings.Current.RevealAnimationMs); }
            else if (edge == "Left") { Top = 0; Height = sh; AnimateTo(Window.LeftProperty, 0, Settings.Current.RevealAnimationMs); }
            else { Top = 0; Height = sh; AnimateTo(Window.LeftProperty, sw - Width, Settings.Current.RevealAnimationMs); }
            AnimateTo(Window.OpacityProperty, 0.98, Settings.Current.RevealAnimationMs);
        }
        }
        }

        private void DockItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.DataContext is DockItem it)
            {
                if (it.AddSlot) { AddAppViaDialog(); return; }
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(it.Path)
                    { UseShellExecute = true });
                }
                catch { }
            }
        }

        public void AddAppViaDialog()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Choose app (.exe or .lnk)",
                Filter = "Apps|*.exe;*.lnk|All|*.*"
            };
            if (dlg.ShowDialog() == true)
            {
                var item = new DockItem
                {
                    Title = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName),
                    Path = dlg.FileName
                };
                Items.Items.Add(item);
                Items.Save();
                BuildItems();
            RebuildPageDots();
            }
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            var w = new SettingsWindow { Owner = this };
            w.ShowDialog();

            // re-apply in case settings changed
            Topmost = Settings.Current.Topmost;
            DwmBackdrop.Apply(this, Settings.Current.Backdrop);
            Height = DockHeight; // reflect icon size change
            if (Settings.Current.AutoHide && !_hidden) HideDock();
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            var w = new Controls.SearchOverlay { Owner = this };
            w.ShowDialog();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

        public void Window_Loaded(object? sender, RoutedEventArgs e) { }

        public void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

        private void ApplyEdgeLayout()
        {
            var sw = SystemParameters.PrimaryScreenWidth;
            var sh = SystemParameters.PrimaryScreenHeight;
            if (Settings.Current.DockEdge == "Left" || Settings.Current.DockEdge == "Right")
            {
                Width = System.Math.Max(44, Settings.Current.ThicknessVertical);
                Height = sh;
            }
            else
            {
                Width = sw;
                Height = System.Math.Max(44, Settings.Current.ThicknessHorizontal);
            }
            ShowDock();
        }
            else
            {
                Width = sw;
                Height = DockHeight;
            }
            ShowDock(); // snap to current edge
        }

        private void EdgePeekCheck()
        {
            var peek = Math.Max(0, Settings.Current.AutoHidePeekPx);
            var sw = SystemParameters.PrimaryScreenWidth;
            var sh = SystemParameters.PrimaryScreenHeight;
            var p = GetCursorPos();

            bool shouldShow = Settings.Current.DockEdge switch
            {
                "Top" => p.y <= peek,
                "Bottom" => p.y >= (sh - 1 - peek),
                "Left" => p.x <= peek,
                "Right" => p.x >= (sw - 1 - peek),
                _ => false
            };

            if (shouldShow) ShowDock();
            else
            {
                // hide if pointer not over dock bounds
                var over = p.x >= Left && p.x <= Left + Width && p.y >= Top && p.y <= Top + Height;
                if (!over) HideDock();
            }
        }
    
    }
}

private System.Windows.Point _dragStart;
private bool _isDragging = false;

private void OnTileMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
{
    _dragStart = e.GetPosition(null);
    _isDragging = false;
}

private void OnTileMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
{
    if (e.LeftButton != System.Windows.Input.MouseButtonState.Pressed) return;
    var pos = e.GetPosition(null);
    if (_isDragging == false && (System.Math.Abs(pos.X - _dragStart.X) > 5 || System.Math.Abs(pos.Y - _dragStart.Y) > 5))
    {
        _isDragging = true;
        if (TileList.SelectedItem != null)
            System.Windows.DragDrop.DoDragDrop(TileList, TileList.SelectedItem, System.Windows.DragDropEffects.Move);
    }
}

private void OnTileDrop(object sender, System.Windows.DragEventArgs e)
{
    var data = e.Data.GetData(typeof(DockItem)) as DockItem;
    var target = (e.OriginalSource as FrameworkElement)?.DataContext as DockItem;
    if (data == null || target == null || data == target) return;

    var list = new System.Collections.Generic.List<DockItem>(Items.All);
    int oldIndex = list.IndexOf(data);
    int newIndex = list.IndexOf(target);
    if (oldIndex >= 0 && newIndex >= 0)
    {
        list.RemoveAt(oldIndex);
        list.Insert(newIndex, data);
        Items.ReplaceAll(list);
        Items.Save();
        BuildItems();
            RebuildPageDots();
    }
}


private void AnimateTo(DependencyProperty prop, double to, int ms)
{
    var anim = new System.Windows.Media.Animation.DoubleAnimation
    {
        To = to,
        Duration = new System.Windows.Duration(System.TimeSpan.FromMilliseconds(ms)),
        EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut }
    };
    this.BeginAnimation(prop, anim);
}


private int _pageIndex = 0;
private void RebuildPageDots()
{
    try
    {
        if (Items == null || PART_PageDots == null) return;
        int per = System.Math.Max(1, Settings.Current.TilesPerPage);
        int total = Items.All.Count;
        int pages = System.Math.Max(1, (int)System.Math.Ceiling(total / (double)per));
        PART_PageDots.Children.Clear();
        for (int i=0;i<pages;i++)
        {
            var b = new System.Windows.Controls.Button { Content = (i==_pageIndex?"●":"○"), Margin = new System.Windows.Thickness(3,0,3,0), Padding = new System.Windows.Thickness(0) };
            int idx = i;
            b.Click += (_, __) => { _pageIndex = idx; BuildItems(); RebuildPageDots(); };
            PART_PageDots.Children.Add(b);
        }
    } catch {}
}


        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Monitor ID
            try { Settings.Current.Profile.ScreenId = MonitorHelper.GetPrimaryId(); } catch {}

            // Hotkeys
            try
            {
                _hotkeys = new GlobalHotkeys(this);
                _hkToggleId = _hotkeys.Bind(Settings.Current.HotkeyToggle ?? "Ctrl+Alt+D", ToggleDock);
                _hkSearchId = _hotkeys.Bind(Settings.Current.HotkeySearch ?? "Ctrl+K", () => { try { BtnSettings_Click(this, new RoutedEventArgs()); } catch {} });
            } catch {}
        }
    

        private void OnMouseWheelScroll(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            int per = System.Math.Max(1, Settings.Current.TilesPerPage);
            int total = Items.All.Count;
            int pages = System.Math.Max(1, (int)System.Math.Ceiling(total / (double)per));
            if (pages <= 1) return;
            _pageIndex = System.Math.Clamp(_pageIndex + (e.Delta < 0 ? 1 : -1), 0, pages - 1);
            BuildItems();
            RebuildPageDots();
            e.Handled = true;
        }
    
        protected override void OnClosed(EventArgs e)
        {
            try
            {
                _hotkeys?.Dispose();
            } catch {}
            base.OnClosed(e);
        }
    