using DockTop.Models;

namespace DockTop.Services
{
    public class SettingsService
    {
        public UserSettings Current { get; private set; }
        public SettingsService() { Current = SettingsStore.Load(); }
        public void Save() => SettingsStore.Save(Current);
    }
}
