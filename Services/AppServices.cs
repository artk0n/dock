
namespace DockTop.Services
{
    public class AppServices
    {
        public SettingsService Settings { get; }
        public ThemeManager Theme { get; }
        public RuleEngine Rules { get; }
        public PluginManager Plugins { get; }
        public SearchIndex Search { get; }
        public ToastService Toasts { get; }

        public AppServices()
        {
            Settings = new SettingsService();
            Theme = new ThemeManager();
            Search = new SearchIndex();
            Toasts = new ToastService();
            Rules = new RuleEngine(Theme, Toasts);
            var ctx = new DockTop.Plugins.PluginContext(Settings, Theme, Rules, Search, Toasts);
            Plugins = new PluginManager(ctx);
        }
    }
}
