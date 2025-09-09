
using System;
using System.Collections.Generic;
using System.Diagnostics;
using DockTop.Plugins;
using DockTop.Services;

namespace DockTop.Plugins.BuiltIn
{
    public class QuickAppsPlugin : IPlugin
    {
        public string Id => "quick.apps";
        public string Name => "Quick Apps";
        public string Version => "0.1.0";
        public bool Enabled { get; set; } = true;

        private PluginContext _ctx;

        public void Initialize(PluginContext context)
        {
            _ctx = context;
            // Expose some default commands to the Search/Command palette
            _ctx.Search.RegisterProvider("Apps", Query);
        }

        private IEnumerable<(string title,string subtitle, Action action)> Query(string q)
        {
            var list = new (string title, string path)[] {
                ("Notepad", "notepad.exe"),
                ("Paint", "mspaint.exe"),
                ("Command Prompt", "cmd.exe"),
                ("PowerShell", "powershell.exe")
            };

            foreach (var (title, path) in list)
            {
                if (string.IsNullOrEmpty(q) || title.Contains(q, StringComparison.OrdinalIgnoreCase))
                {
                    yield return (title, "Quick Apps", () => {
                        try { Process.Start(new ProcessStartInfo{ FileName = path, UseShellExecute = true }); } catch {}
                    });
                }
            }
        }

        public IEnumerable<PluginCommand> GetCommands()
        {
            yield return new PluginCommand("refresh", "Refresh App List", "Rebuilds the quick apps index.");
        }

        public void Dispose() { }
    }
}
