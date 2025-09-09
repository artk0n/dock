using System.Linq;
using System.IO;
using DockTop.Models;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace DockTop.Controls
{
    public partial class SettingsFlyout : UserControl
    {
        public SettingsFlyout()
        {
            InitializeComponent();
            Loaded += (_, __) =>
            {
                ThemeCombo.ItemsSource = ThemeStore.LoadThemes().Select(t => t.Name).ToList();
            };
            ImportThemesBtn.Click += (_, __) => ImportThemes();
            ExportThemesBtn.Click += (_, __) => ExportThemes();
            ApplyBtn.Click += (_, __) => MessageBox.Show("Applied. (Live apply is basic in this sample; reopen dock to fully refresh theme.)");
            CloseBtn.Click += (_, __) => this.Visibility = Visibility.Collapsed;
        }

        private void ImportThemes()
        {
            try
            {
                var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "themes.import.json");
                if (File.Exists(path))
                {
                    File.Copy(path, "themes.json", true);
                    MessageBox.Show("Imported themes from themes.import.json");
                }
                else MessageBox.Show("Place a file named themes.import.json next to the EXE then click Import.");
            } catch {}
        }

        private void ExportThemes()
        {
            try
            {
                File.Copy("themes.json", "themes.export.json", true);
                MessageBox.Show("Exported to themes.export.json");
            } catch {}
        }
    }
}
