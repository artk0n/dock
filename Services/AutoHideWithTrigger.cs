using System;
using System.Windows;
using System.Windows.Threading;
using DockTop.Models;

namespace DockTop.Services
{
    public class AutoHideWithTrigger : IDisposable
    {
        private readonly Window _dock;
        private readonly TriggerWindow _trigger;
        private readonly DispatcherTimer _hideTimer = new DispatcherTimer();
        private UserSettings _settings;

        public AutoHideWithTrigger(Window dock, UserSettings settings)
        {
            _dock = dock;
            _settings = settings;
            _trigger = new TriggerWindow();
            _trigger.MouseEnter += (s, e) => ShowDock();
            _dock.MouseLeave += (s, e) => StartHide();
            _hideTimer.Tick += (s, e) => HideDock();
            Configure(settings);
        }

        public void Configure(UserSettings s)
        {
            _settings = s;
            _hideTimer.Interval = TimeSpan.FromMilliseconds(Math.Max(0, s.AutoHideDelayMs));
            _trigger.Width = SystemParameters.PrimaryScreenWidth;
            _trigger.Height = 2;
            EdgeManager.PositionWindow(_trigger, s.Edge);
            EdgeManager.PositionWindow(_dock, s.Edge);
            if (s.AutoHide) _trigger.Show(); else _trigger.Hide();
        }

        private void StartHide()
        {
            if (!_settings.AutoHide) return;
            _hideTimer.Stop(); _hideTimer.Start();
        }

        private void HideDock()
        {
            _hideTimer.Stop();
            if (!_settings.AutoHide) return;
            switch (_settings.Edge)
            {
                case "Top": _dock.Top = -(_dock.ActualHeight - 2); break;
                case "Bottom": _dock.Top = SystemParameters.PrimaryScreenHeight - 2; break;
                case "Left": _dock.Left = -(_dock.ActualWidth - 2); break;
                case "Right": _dock.Left = SystemParameters.PrimaryScreenWidth - 2; break;
            }
        }

        private void ShowDock() => EdgeManager.PositionWindow(_dock, _settings.Edge);
        public void Dispose() { try { _trigger.Close(); } catch { } _hideTimer.Stop(); }
    }
}
