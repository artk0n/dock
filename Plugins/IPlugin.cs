
using System.Collections.Generic;

namespace DockTop.Plugins
{
    public interface IPlugin
    {
        string Id { get; }
        string Name { get; }
        string Version { get; }
        bool Enabled { get; set; }

        void Initialize(PluginContext context);
        void Dispose();

        // Optional: expose commands for command palette
        IEnumerable<PluginCommand> GetCommands();
    }

    public record PluginCommand(string Id, string Title, string Description);
}
