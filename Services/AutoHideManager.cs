using System;
using System.Windows;
using System.Windows.Threading;

namespace DockTop.Services
{
    public class AutoHideManager : IDisposable
    {
        private readonly Window _host;
        private readonly DispatcherTimer _hideTimer;
        private bool _enabled;
        private int _delayMs = 1200;
        private double _hiddenOffset = -80; // negative Y to move off-screen at top

        public AutoHideManager(Window host)
        {
            _host = host;
            _hideTimer = new DispatcherTimer();
            _hideTimer.Tick += (_, __) => HideDock();
            _host.MouseEnter += (_, __) => CancelHide();
            _host.MouseLeave += (_, __) => StartHideCountdown();
        }

        public void Configure(bool enabled, int delayMs)
        {
            _enabled = enabled;
            _delayMs = Math.Max(0, delayMs);
            if (!_enabled)
            {
                CancelHide();
                ShowDock();
            }
        }

        private void StartHideCountdown()
        {
            if (!_enabled) return;
            _hideTimer.Interval = TimeSpan.FromMilliseconds(_delayMs);
            _hideTimer.Stop();
            _hideTimer.Start();
        }

        private void CancelHide()
        {
            _hideTimer.Stop();
            ShowDock();
        }

        private void HideDock()
        {
            if (!_enabled) return;
            try
            {
                _host.Top = -_hiddenOffset; // reserve a thin hover area
                _host.Opacity = Math.Max(0.01, _host.Opacity - 0.05);
            } catch { }
        }

        private void ShowDock()
        {
            try
            {
                // Snap to top edge
                _host.Top = 0;
                // restore opacity (don't touch if user set)
                if (_host.Opacity < 0.85) _host.Opacity = 0.85;
            } catch { }
        }

        public void Dispose()
        {
            _hideTimer.Stop();
        }
    }
}