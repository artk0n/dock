using System;
using System.Collections.Generic;
using System.IO;
using DockTop.Models;

namespace DockTop.Services
{
    public static class AppScanner
    {
        public static List<DockItem> ScanStartMenu()
        {
            var items = new List<DockItem>();
            foreach (var dir in GetStartMenuDirs())
            {
                if (!Directory.Exists(dir)) continue;
                foreach (var lnk in Directory.EnumerateFiles(dir, "*.lnk", SearchOption.AllDirectories))
                {
                    var title = System.IO.Path.GetFileNameWithoutExtension(lnk);
                    items.Add(new DockItem { Title = title, Path = lnk });
                }
            }
            return items;
        }

        private static IEnumerable<string> GetStartMenuDirs()
        {
            var list = new List<string>();
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var progData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            list.Add(System.IO.Path.Combine(appData, @"Microsoft\Windows\Start Menu\Programs"));
            list.Add(System.IO.Path.Combine(progData, @"Microsoft\Windows\Start Menu\Programs"));
            return list;
        }
    }
}
