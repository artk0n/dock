using System.Windows;
using DockTop.Services;

namespace DockTop
{
    public partial class SettingsWindow : Window
    {
        private readonly SettingsService _svc = new();
        public SettingsWindow()
        {
            InitializeComponent();
            // load
            CmbBackdrop.SelectedIndex = _svc.Current.Backdrop.StartsWith("Acr") ? 1 : 0;
            SldRadius.Value = _svc.Current.CornerRadius;
            SldGlow.Value = _svc.Current.GlowOpacity;
            CmbEdge.SelectedIndex = _svc.Current.DockEdge == "Top" ? 0 : (_svc.Current.DockEdge == "Bottom" ? 1 : (_svc.Current.DockEdge == "Left" ? 2 : 3));
            if (_svc.Current.IconSize == 24) CmbIcon.SelectedIndex = 0;
            else if (_svc.Current.IconSize == 50) CmbIcon.SelectedIndex = 2;
            else CmbIcon.SelectedIndex = 1;
            ChkTop.IsChecked = _svc.Current.Topmost;
            ChkAuto.IsChecked = _svc.Current.AutoHide;
            SldPeek.Value = _svc.Current.AutoHidePeekPx;
            ChkClock.IsChecked = _svc.Current.Clock12h;
            ChkNow.IsChecked = _svc.Current.NowPlaying;
            ChkPing.IsChecked = _svc.Current.NetworkPing;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _svc.Current.Backdrop = (CmbBackdrop.SelectedIndex==1) ? "Acrylic" : "Mica";
            _svc.Current.CornerRadius = (int)SldRadius.Value;
            _svc.Current.GlowOpacity = SldGlow.Value;
            _svc.Current.DockEdge = (CmbEdge.SelectedIndex==0?"Top":(CmbEdge.SelectedIndex==1?"Bottom":(CmbEdge.SelectedIndex==2?"Left":"Right")));
            _svc.Current.ThicknessHorizontal = (int)SldThH.Value;
            _svc.Current.ThicknessVertical = (int)SldThV.Value;
            if (int.TryParse(TxtAnimIn.Text, out var ain)) _svc.Current.RevealAnimationMs = ain;
            if (int.TryParse(TxtAnimOut.Text, out var aout)) _svc.Current.HideAnimationMs = aout;
            _svc.Current.ClickThroughWhenHidden = ChkClickThrough.IsChecked==true;
            _svc.Current.AutoStartWithWindows = ChkAutoStart.IsChecked==true;
            _svc.Current.HotkeyToggle = TxtHotToggle.Text;
            _svc.Current.HotkeySearch = TxtHotSearch.Text;
            _svc.Current.IconSize = CmbIcon.SelectedIndex==0 ? 24 : (CmbIcon.SelectedIndex==2 ? 50 : 32);
            _svc.Current.Topmost = ChkTop.IsChecked==true;
            _svc.Current.AutoHide = ChkAuto.IsChecked==true;
            _svc.Current.DockEdge = (CmbEdge.SelectedIndex==0?"Top":(CmbEdge.SelectedIndex==1?"Bottom":(CmbEdge.SelectedIndex==2?"Left":"Right")));
            _svc.Current.AutoHidePeekPx = (int)SldPeek.Value;
            _svc.Current.Clock12h = ChkClock.IsChecked==true;
            _svc.Current.NowPlaying = ChkNow.IsChecked==true;
            _svc.Current.NetworkPing = ChkPing.IsChecked==true;
            _svc.Save();
            using DockTop.Services;
using System.Diagnostics;

var exe = Process.GetCurrentProcess().MainModule?.FileName ?? "";
AutoStartService.SetAutoStart(_svc.Current.AutoStartWithWindows, exe);

            Close();
        }
        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
