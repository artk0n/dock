using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace DockTop.Utils
{
    [Flags]
    public enum HotKeyModifiers
    {
        None = 0x0000,
        Alt  = 0x0001,
        Ctrl = 0x0002,
        Shift= 0x0004,
        Win  = 0x0008
    }

    public class GlobalHotkeys : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private readonly HwndSource _source;
        private readonly Dictionary<int, Action> _actions = new();
        private int _nextId = 1;
        private bool _disposed;

        public GlobalHotkeys(Window window)
        {
            var helper = new WindowInteropHelper(window);
            _source = HwndSource.FromHwnd(helper.Handle)!;
            _source.AddHook(HwndHook);
        }

        public int Bind(string chord, Action action)
        {
            if (string.IsNullOrWhiteSpace(chord)) return -1;
            if (!TryParse(chord, out var mods, out var key)) return -1;
            int id = _nextId++;
            RegisterHotKey(_source.Handle, id, (uint)mods, (uint)KeyInterop.VirtualKeyFromKey(key));
            _actions[id] = action;
            return id;
        }

        public void Unbind(int id)
        {
            if (id <= 0) return;
            try { UnregisterHotKey(_source.Handle, id); } catch {}
            _actions.Remove(id);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                if (_actions.TryGetValue(id, out var act))
                {
                    try { act?.Invoke(); } catch {}
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        public static bool TryParse(string chord, out HotKeyModifiers mods, out Key key)
        {
            mods = HotKeyModifiers.None; key = Key.None;
            var parts = chord.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var p in parts)
            {
                var up = p.Trim().ToUpperInvariant();
                if (up is "CTRL" or "CONTROL") mods |= HotKeyModifiers.Ctrl;
                else if (up == "ALT") mods |= HotKeyModifiers.Alt;
                else if (up is "SHIFT") mods |= HotKeyModifiers.Shift;
                else if (up is "WIN" or "WINDOWS" or "META") mods |= HotKeyModifiers.Win;
                else
                {
                    if (Enum.TryParse<Key>(up, true, out var k)) key = k;
                }
            }
            return key != Key.None;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            foreach (var kv in _actions) { try { UnregisterHotKey(_source.Handle, kv.Key); } catch {} }
            _actions.Clear();
            _source.RemoveHook(HwndHook);
        }
    }
}