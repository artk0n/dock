using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using DockTop.Models;
using DockTop.Services;

namespace DockTop.ViewModels
{
    public class DockViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public SettingsService Settings { get; }
        public DockItemsService Items { get; }

        public ObservableCollection<string> Tabs { get; }
        public ICollectionView DockItemsView { get; }

        private string _activeTab;
        public string ActiveTab
        {
            get => _activeTab;
            set { if (_activeTab != value) { _activeTab = value; Settings.Current.ActiveTab = value; OnPropertyChanged(); DockItemsView.Refresh(); } }
        }

        public DockViewModel(SettingsService settings, DockItemsService items)
        {
            Settings = settings;
            Items = items;
            Tabs = new ObservableCollection<string>(settings.Current.Tabs);
            _activeTab = settings.Current.ActiveTab;

            DockItemsView = CollectionViewSource.GetDefaultView(Items.Items);
            DockItemsView.Filter = FilterByTab;
        }

        private bool FilterByTab(object obj)
        {
            if (obj is not DockItem it) return false;
            if (string.Equals(ActiveTab, "All", System.StringComparison.OrdinalIgnoreCase)) return true;
            return string.Equals(it.Group ?? "All", ActiveTab, System.StringComparison.OrdinalIgnoreCase);
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
