using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DockTop.Controls
{
    public partial class SearchOverlay : Window
    {
        public SearchOverlay()
        {
            InitializeComponent();
            Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
            Top = 100;
            Loaded += (s, e) => Box.Focus();
        }

        private void Box_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) { Close(); return; }
            if (e.Key == Key.Enter)
            {
                var q = Box.Text.Trim();
                if (string.IsNullOrWhiteSpace(q)) { Close(); return; }

                string[] roots = new[]
                {
                    System.Environment.ExpandEnvironmentVariables(@"%PROGRAMDATA%\Microsoft\Windows\Start Menu\Programs"),
                    System.Environment.ExpandEnvironmentVariables(@"%APPDATA%\Microsoft\Windows\Start Menu\Programs"),
                    System.Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\Desktop")
                };

                var candidates = roots
                    .Where(Directory.Exists)
                    .SelectMany(r => Directory.GetFiles(r, "*.lnk", SearchOption.AllDirectories));

                var hit = candidates.FirstOrDefault(p =>
                    Path.GetFileNameWithoutExtension(p).ToLower().Contains(q.ToLower()));

                if (hit != null)
                {
                    Process.Start(new ProcessStartInfo(hit) { UseShellExecute = true });
                    Close();
                    return;
                }

                var url = "https://www.google.com/search?q=" + System.Uri.EscapeDataString(q);
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                Close();
            }
        }
    }
}
