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

        private void BuildItems()
        {
            DisplayItems.Clear();
            foreach (var it in Items.Items)
                if (!it.Disabled) DisplayItems.Add(it);
            DisplayItems.Add(new DockItem { Title = "+ Add", AddSlot = true });
            DisplayItems.Add(new DockItem { Title = "+ Add", AddSlot = true });
        }

        private void ToggleDock() { if (_hidden) ShowDock(); else HideDock(); }

        private void HideDock()
        {
            _hidden = true;
            var edge = Settings.Current.DockEdge;
            var sw = SystemParameters.PrimaryScreenWidth;
            var sh = SystemParameters.PrimaryScreenHeight;
            var peek = Math.Max(0, Settings.Current.AutoHidePeekPx);
            switch (edge)
            {
                case "Top": Top = -(Height - peek); Left = 0; Width = sw; break;
                case "Bottom": Top = sh - peek; Left = 0; Width = sw; break;
                case "Left": Left = -(Width - peek); Top = 0; Height = sh; break;
                case "Right": Left = sw - peek; Top = 0; Height = sh; break;
                default: Top = -(Height - peek); Left = 0; break;
            }
        }
        }

        private void ShowDock()
        {
            _hidden = false;
            var edge = Settings.Current.DockEdge;
            var sw = SystemParameters.PrimaryScreenWidth;
            var sh = SystemParameters.PrimaryScreenHeight;
            switch (edge)
            {
                case "Top": Top = 0; Left = 0; Width = sw; break;
                case "Bottom": Top = sh - Height; Left = 0; Width = sw; break;
                case "Left": Left = 0; Top = 0; Height = sh; break;
                case "Right": Left = sw - Width; Top = 0; Height = sh; break;
                default: Top = 0; Left = 0; break;
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
            // Thickness: for vertical edges, use DockHeight as width to keep consistent 'thickness'
            var sw = SystemParameters.PrimaryScreenWidth;
            var sh = SystemParameters.PrimaryScreenHeight;
            if (Settings.Current.DockEdge == "Left" || Settings.Current.DockEdge == "Right")
            {
                Width = DockHeight; // thickness
                Height = sh;
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