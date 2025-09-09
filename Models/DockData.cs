using System.Collections.ObjectModel;
using System.IO;

namespace DockTop.Models
{
    // Central in-memory item list used by the UI.
    public static class Items
    {
        public static ObservableCollection<DockItem> All { get; } = new ObservableCollection<DockItem>();

        public static void AddFromPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            All.Add(new DockItem
            {
                Title = Path.GetFileNameWithoutExtension(path),
                Path  = path
            });
        }

        public static void Save() { /* no-op (todo: persist to JSON) */ }
        public static void Load() { /* no-op */ }
    }
}
