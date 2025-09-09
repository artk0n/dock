using System.Windows.Forms;
namespace DockTop.Utils
{
    public static class MonitorHelper
    {
        public static string GetPrimaryId() { var s = Screen.PrimaryScreen; return s?.DeviceName ?? "Primary"; }
    }
}
