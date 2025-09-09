using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using DockTop.Models;
using DockTop.Services;

namespace DockTop.ViewModels
{
    public class DesignerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public SettingsService Settings { get; }
        public DockItemsService ItemsService { get; }

        public ObservableCollection<string> Tabs { get; }
        public ObservableCollection<DockItem> Items => ItemsService.Items;
        public ObservableCollection<DockItem> Suggestions { get; } = new();

        private string _activeTab;
        public string ActiveTab { get => _activeTab; set { _activeTab = value; Settings.Current.ActiveTab = value; OnPropertyChanged(); } }
        public DockItem? Selected { get; set; }

        public DesignerViewModel(SettingsService settings, DockItemsService items)
        {
            Settings = settings;
            ItemsService = items;
            Tabs = new ObservableCollection<string>(settings.Current.Tabs);
            _activeTab = settings.Current.ActiveTab;
        }

        public void LoadSuggestions()
        {
            Suggestions.Clear();
            foreach (var it in AppScanner.ScanStartMenu())
                Suggestions.Add(it);
        }

        public void AddTab(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            if (!Tabs.Contains(name)) { Tabs.Add(name); Settings.Current.Tabs = Tabs.ToArray(); }
        }

        public void AddItem(DockItem it) { it.Group = ActiveTab; Items.Add(it); }
        public void RemoveSelected() { if (Selected != null) Items.Remove(Selected); }
        public void MoveUp() { if (Selected == null) return; var i = Items.IndexOf(Selected); if (i > 0) Items.Move(i, i - 1); }
        public void MoveDown() { if (Selected == null) return; var i = Items.IndexOf(Selected); if (i >= 0 && i < Items.Count - 1) Items.Move(i, i + 1); }
    }
}
