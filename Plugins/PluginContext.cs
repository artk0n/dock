
using DockTop.Services;

namespace DockTop.Plugins
{
    public class PluginContext
    {
        public SettingsService Settings { get; }
        public ThemeManager Theme { get; }
        public RuleEngine Rules { get; }
        public SearchIndex Search { get; }
        public ToastService Toasts { get; }

        public PluginContext(SettingsService s, ThemeManager t, RuleEngine r, SearchIndex search, ToastService toasts)
        {
            Settings = s;
            Theme = t;
            Rules = r;
            Search = search;
            Toasts = toasts;
        }
    }
}
