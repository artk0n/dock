using System;
using System.Windows;
using System.Windows.Input;

namespace DockTop.Services
{
    public class HotkeyManager : IDisposable
    {
        private readonly Window _win;
        private Action? _toggle;

        public HotkeyManager(Window win) { _win = win; }

        public void RegisterToggle(string gesture, Action toggle)
        {
            _toggle = toggle;
            _win.PreviewKeyDown += (s, e) =>
            {
                if (gesture.Equals("Ctrl+Alt+D", StringComparison.OrdinalIgnoreCase) &&
                    (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
                    (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)) &&
                    e.Key == Key.D)
                {
                    toggle();
                    e.Handled = true;
                }
            };
        }

        public void Dispose() { }
    }
}
