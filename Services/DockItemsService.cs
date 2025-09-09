using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using DockTop.Models;

namespace DockTop.Services
{
    public class DockItemsService
    {
        public ObservableCollection<DockItem> Items { get; private set; } = new();

        private string Dir => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DockTop");

        private string ItemsPath => Path.Combine(Dir, "dock.items.json");

        public DockItemsService()
        {
            try
            {
                if (File.Exists(ItemsPath))
                {
                    var arr = JsonSerializer.Deserialize<ObservableCollection<DockItem>>(
                        File.ReadAllText(ItemsPath));
                    if (arr != null) Items = arr;
                }
                else
                {
                    Items.Add(new DockItem { Title = "PowerShell 7", Path = @"C:\Users\User\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\pwsh.lnk" });
                    Items.Add(new DockItem { Title = "VS Code", Path = @"C:\Users\User\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Visual Studio Code\Visual Studio Code.lnk" });
                    Items.Add(new DockItem { Title = "Google Drive", Path = @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Google Drive.lnk" });
                }
            }
            catch { }
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Dir);
                File.WriteAllText(ItemsPath,
                    JsonSerializer.Serialize(Items, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { }
        }
    }
}
