
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DockTop.Plugins;

namespace DockTop.Services
{
    public class PluginManager
    {
        private readonly List<IPlugin> _plugins = new();
        private readonly PluginContext _ctx;
        private readonly string _pluginsFolder;

        public IReadOnlyList<IPlugin> Plugins => _plugins;

        public PluginManager(PluginContext ctx)
        {
            _ctx = ctx;
            _pluginsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DockTop", "Plugins");
            Directory.CreateDirectory(_pluginsFolder);
        }

        public void LoadBuiltIns()
        {
            // Built-in plugins are compiled in this assembly
            var asm = Assembly.GetExecutingAssembly();
            var types = asm.GetTypes().Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
            foreach (var t in types)
            {
                if (t.FullName!.Contains(".BuiltIn."))
                {
                    try
                    {
                        var plugin = (IPlugin)Activator.CreateInstance(t)!;
                        plugin.Initialize(_ctx);
                        plugin.Enabled = true;
                        _plugins.Add(plugin);
                    }
                    catch { }
                }
            }
        }

        public void LoadExternal()
        {
            // *.dll in plugins folder
            foreach (var dll in Directory.GetFiles(_pluginsFolder, "*.dll"))
            {
                try
                {
                    var alc = new System.Runtime.Loader.AssemblyLoadContext(Path.GetFileNameWithoutExtension(dll), true);
                    using var fs = new FileStream(dll, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    var assembly = alc.LoadFromStream(fs);
                    var types = assembly.GetTypes().Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                    foreach (var t in types)
                    {
                        var plugin = (IPlugin)Activator.CreateInstance(t)!;
                        plugin.Initialize(_ctx);
                        plugin.Enabled = true;
                        _plugins.Add(plugin);
                    }
                }
                catch { }
            }
        }
    }
}
