
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace DockTop.Utils
{
    public static class WindowStyles
    {
        const int GWL_EXSTYLE = -20;
        const int WS_EX_TRANSPARENT = 0x00000020;
        const int WS_EX_LAYERED = 0x00080000;

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public static void SetClickThrough(Window w, bool enabled)
        {
            var hwnd = new WindowInteropHelper(w).Handle;
            int ex = GetWindowLong(hwnd, GWL_EXSTYLE);
            if (enabled) ex |= (WS_EX_TRANSPARENT | WS_EX_LAYERED);
            else ex &= ~(WS_EX_TRANSPARENT);
            SetWindowLong(hwnd, GWL_EXSTYLE, ex);
        }
    }
}
