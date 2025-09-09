using System;
using System.IO;
using System.Linq;
using System.Windows;
using DockTop.Models;
using DockTop.Services;
using DockTop.ViewModels;

namespace DockTop
{
    public partial class DockDesignerWindow : Window
    {
        private readonly DesignerViewModel _vm;
        private readonly Action? _refreshPreview;
        private readonly Action<UserSettings>? _applySettings;
        private readonly SettingsService _settings;
        private readonly DockItemsService _items;

        public DockDesignerWindow(SettingsService settings, DockItemsService items,
                                  Action? refreshPreview = null,
                                  Action<UserSettings>? applySettings = null)
        {
            InitializeComponent();
            _settings = settings;
            _items = items;
            _vm = new DesignerViewModel(settings, items);
            _refreshPreview = refreshPreview;
            _applySettings = applySettings;
            DataContext = _vm;
            Loaded += OnLoaded;
            AllowDrop = true;
            this.Drop += OnDrop;
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            _vm.LoadSuggestions();
            AppsList.ItemsSource = _vm.Suggestions;

            AddTabBtn.Click += (s, a) =>
            {
                var name = "New Tab";
                try
                {
                    name = Microsoft.VisualBasic.Interaction.InputBox("New tab name:", "Add Tab", "New Tab");
                } catch { }
                if (!string.IsNullOrWhiteSpace(name)) _vm.AddTab(name);
            };

            AddBlankBtn.Click += (s, a) => _vm.AddItem(new DockItem { Title = "New Item", Group = _vm.ActiveTab });
            RemoveBtn.Click += (s, a) => _vm.RemoveSelected();
            UpBtn.Click += (s, a) => _vm.MoveUp();
            DownBtn.Click += (s, a) => _vm.MoveDown();
            RefreshAppsBtn.Click += (s, a) => _vm.LoadSuggestions();
            AddSelectedAppBtn.Click += (s, a) =>
            {
                if (AppsList.SelectedItem is DockItem it) _vm.AddItem(new DockItem
                {
                    Title = it.Title, Path = it.Path, Group = _vm.ActiveTab
                });
            };
            ImportBtn.Click += (s, a) => ImportJson();
            ExportBtn.Click += (s, a) => ExportJson();
            BrowseBtn.Click += (s, a) => BrowseAdd();

            ThemeCombo.ItemsSource = ThemeStore.LoadAll();
            ThemeCombo.DisplayMemberPath = "Name";
            ThemeCombo.SelectedValuePath = "Id";
            ThemeCombo.SelectedValue = _settings.Current.ThemeId;
            ApplyThemeBtn.Click += (s, a) =>
            {
                _settings.Current.ThemeId = ThemeCombo.SelectedValue?.ToString() ?? _settings.Current.ThemeId;
                _applySettings?.Invoke(_settings.Current);
                _refreshPreview?.Invoke();
            };

            AutoHideCheck.IsChecked = _settings.Current.AutoHide;
            AutoHideDelaySlider.Value = _settings.Current.AutoHideDelayMs;
            TopmostCheck.IsChecked = _settings.Current.Topmost;

            ApplyBtn.Click += (s, a) => ApplyOnly();
            SaveBtn.Click += (s, a) => { ApplyOnly(); _settings.Save(); _items.Save(); };
        }

        private void ApplyOnly()
        {
            _settings.Current.AutoHide = AutoHideCheck.IsChecked == true;
            _settings.Current.AutoHideDelayMs = (int)AutoHideDelaySlider.Value;
            _settings.Current.Topmost = TopmostCheck.IsChecked == true;
            _applySettings?.Invoke(_settings.Current);
            _refreshPreview?.Invoke();
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    foreach (var f in files.Where(p => p.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) || p.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase) || p.EndsWith(".bat", StringComparison.OrdinalIgnoreCase)))
                    {
                        _vm.AddItem(new DockItem { Title = System.IO.Path.GetFileNameWithoutExtension(f), Path = f, Group = _vm.ActiveTab });
                    }
                }
            } catch { }
        }

        private void BrowseAdd()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "Apps|*.exe;*.lnk;*.bat|All files|*.*" };
            if (dlg.ShowDialog() == true)
            {
                var f = dlg.FileName;
                _vm.AddItem(new DockItem { Title = System.IO.Path.GetFileNameWithoutExtension(f), Path = f, Group = _vm.ActiveTab });
            }
        }

        private void ImportJson()
        {
            try
            {
                var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "JSON files|*.json" };
                if (dlg.ShowDialog() == true)
                {
                    var json = File.ReadAllText(dlg.FileName);
                    var list = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<DockItem>>(json);
                    if (list != null)
                    {
                        _items.Items.Clear();
                        foreach (var it in list) _items.Items.Add(it);
                    }
                }
            } catch { }
        }

        private void ExportJson()
        {
            try
            {
                var dlg = new Microsoft.Win32.SaveFileDialog { Filter = "JSON files|*.json", FileName = "dock.items.json" };
                if (dlg.ShowDialog() == true)
                {
                    var list = _items.Items.ToList();
                    var json = System.Text.Json.JsonSerializer.Serialize(list, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(dlg.FileName, json);
                }
            } catch { }
        }
    }
}
