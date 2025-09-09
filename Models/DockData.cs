using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace DockTop.Models
{
    public class DockItem
    {
        public string Title { get; set; } = "";
        public string Path  { get; set; } = "";
        public bool   Disabled { get; set; } = false;
        public bool   AddSlot  { get; set; } = false;
    }

    // ultra-minimal in-memory store (no persistence needed for compile)
    public static class Items
    {
        public static ObservableCollection<DockItem> All { get; } = new ObservableCollection<DockItem>();

        public static void AddFromPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            All.Add(new DockItem {
                Title = System.IO.Path.GetFileNameWithoutExtension(path),
                Path  = path
            });
        }

        public static void Save() { /* no-op for now */ }
        public static void Load() { /* no-op for now */ }
    }
}
