
using Microsoft.Win32;

namespace DockTop.Services
{
    public static class AutoStartService
    {
        private const string RUN_KEY = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private const string APP_NAME = "DockTop";
        public static void SetAutoStart(bool enabled, string exePath)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RUN_KEY, true) ?? Registry.CurrentUser.CreateSubKey(RUN_KEY, true);
                if (enabled)
                    key.SetValue(APP_NAME, $"\"{exePath}\"");
                else if (key.GetValue(APP_NAME) != null)
                    key.DeleteValue(APP_NAME);
            }
            catch {}
        }
        public static bool IsEnabled()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RUN_KEY, false);
                return key?.GetValue(APP_NAME) != null;
            } catch { return false; }
        }
    }
}
