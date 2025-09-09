using System;
using System.IO;
using System.Text.Json;

namespace DockTop.Models
{
    public class UserSettings
    {
        public string ThemeId { get; set; } = "pastel-cloud";
        public string AccentHex { get; set; } = "#B81F1120"; // hsla(297,30%,9.6%,0.72)
        public bool Topmost { get; set; } = true;
        public bool AutoHide { get; set; } = true;
        public string DockEdge { get; set; } = "Top"; // Top|Bottom|Left|Right
        public int AutoHidePeekPx { get; set; } = 1;
        public int AutoHideShowDelayMs { get; set; } = 100;
        public int AutoHideHideDelayMs { get; set; } = 100;
        public int IconSize { get; set; } = 32; // 24/32/50 via UI
        public double CornerRadius { get; set; } = 18;
        public double GlowOpacity { get; set; } = 0.35;
        public string Backdrop { get; set; } = "Mica";

        public string HotkeyToggle { get; set; } = "Ctrl+Up";
        public string HotkeyQuickSearch { get; set; } = "Ctrl+K";

        public string SearchEngine { get; set; } = "Google";
        public bool IndexDefaults { get; set; } = true;

        public bool StartupShortcut { get; set; } = true;

        public bool Clock12h { get; set; } = true;
        public bool NowPlaying { get; set; } = true;
        public bool NetworkPing { get; set; } = true;
        public string PingHost { get; set; } = "1.1.1.1";
    }

    public static class SettingsStore
    {
        public static string Dir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DockTop");
        public static string FilePath => Path.Combine(Dir, "user.settings.json");
        public static UserSettings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                    return JsonSerializer.Deserialize<UserSettings>(File.ReadAllText(FilePath)) ?? new UserSettings();
            } catch {}
            return new UserSettings();
        }
        public static void Save(UserSettings s)
        {
            try
            {
                Directory.CreateDirectory(Dir);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(s, new JsonSerializerOptions{WriteIndented=true}));
            } catch {}
        }
    }
}
