using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace DockTop
{
    public static partial class DwmBackdrop
    {
        const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        const int DWMWA_SYSTEMBACKDROP_TYPE = 38; // 2 Mica, 4 Acrylic

        [DllImport("dwmapi.dll")] static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        public static void Apply(Window w, int type = 2)
        {
            try
            {
                var h = new WindowInteropHelper(w).Handle;
                int dark = 1; DwmSetWindowAttribute(h, DWMWA_USE_IMMERSIVE_DARK_MODE, ref dark, sizeof(int));
                int backdrop = type; DwmSetWindowAttribute(h, DWMWA_SYSTEMBACKDROP_TYPE, ref backdrop, sizeof(int));
            } catch { }
        }
        public static void Apply(Window w, string mode)
        {
            int kind = 2; // Mica default
            if (!string.IsNullOrWhiteSpace(mode) && mode.StartsWith("Acr", StringComparison.OrdinalIgnoreCase)) kind = 4;
            Apply(w, kind);
        }
    }
}
