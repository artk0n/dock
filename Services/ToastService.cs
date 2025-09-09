
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace DockTop.Services
{
    public class ToastService
    {
        public void Info(string message) => Show(message);

        private void Show(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var wnd = new Window
                {
                    Width = 360,
                    Height = 80,
                    WindowStyle = WindowStyle.None,
                    AllowsTransparency = true,
                    Background = Brushes.Transparent,
                    Topmost = true,
                    ShowInTaskbar = false
                };

                var grid = new Grid
                {
                    Background = (Brush)Application.Current.Resources["Brush.Surface"],
                    Margin = new Thickness(8),
                };
                grid.Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Opacity = 0.4,
                    BlurRadius = 16,
                    ShadowDepth = 0,
                    Color = Colors.Black
                };
                grid.Children.Add(new TextBlock
                {
                    Text = message,
                    Margin = new Thickness(16),
                    Foreground = (Brush)Application.Current.Resources["Brush.Text"],
                    TextWrapping = TextWrapping.Wrap
                });
                wnd.Content = grid;

                // bottom-right
                var area = SystemParameters.WorkArea;
                wnd.Left = area.Right - wnd.Width - 20;
                wnd.Top = area.Bottom - wnd.Height - 20;

                wnd.Show();

                var t = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.5) };
                t.Tick += (s,e)=> { t.Stop(); try { wnd.Close(); } catch {} };
                t.Start();
            });
        }
    }
}
