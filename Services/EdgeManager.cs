using System.Windows;

namespace DockTop.Services
{
    public static class EdgeManager
    {
        public static void PositionWindow(Window window, string edge)
        {
            var sw = SystemParameters.PrimaryScreenWidth;
            var sh = SystemParameters.PrimaryScreenHeight;
            switch (edge)
            {
                case "Top":
                    window.Left = 0; window.Top = 0; break;
                case "Bottom":
                    window.Left = 0; window.Top = sh - window.Height; break;
                case "Left":
                    window.Left = 0; window.Top = 0; break;
                case "Right":
                    window.Left = sw - window.Width; window.Top = 0; break;
                default:
                    window.Left = 0; window.Top = 0; break;
            }
        }
    }
}
