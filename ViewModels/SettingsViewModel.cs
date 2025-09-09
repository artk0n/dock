using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DockTop.Models;

namespace DockTop.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private DockTop.Models.UserSettings _settings;
        public DockTop.Models.UserSettings Settings { get => _settings; set { _settings = value; OnPropertyChanged(); } }

        public ObservableCollection<ThemeOption> Themes { get; } = new();

        public SettingsViewModel(DockTop.Models.UserSettings s) { _settings = s; }

        public void LoadThemes()
        {
            Themes.Clear();
            foreach (var t in ThemeStore.LoadAll())
                Themes.Add(new ThemeOption { Id = t.Id, Name = t.Name });
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class ThemeOption { public string Id { get; set; } = ""; public string Name { get; set; } = ""; }
}
