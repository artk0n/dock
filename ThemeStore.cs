using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DockTop
{
    public class DockTheme
    {
        public string Id { get; set; } = "pastel-cloud";
        public string Name { get; set; } = "Pastel Cloud";
        public string GradientStart { get; set; } = "#E6F3FEFF";
        public string GradientEnd { get; set; } = "#F7E9FFFF";
        public string Accent { get; set; } = "#B38CF5";
        public int BlurStrength { get; set; } = 10;
        public double GlowIntensity { get; set; } = 0.9;

        // Optional tokens
        public string? Surface { get; set; } = "#14FFFFFF";
        public string? SurfaceAlt { get; set; } = "#0FFFFFFF";
        public string? Border { get; set; } = "#22FFFFFF";
        public string? Shadow { get; set; } = "#22000000";
    }

    public static class ThemeStore
    {
        private static readonly string ThemesPath = Path.Combine(AppContext.BaseDirectory, "themes.json");

        public static IReadOnlyList<DockTheme> LoadAll()
        {
            try
            {
                if (File.Exists(ThemesPath))
                {
                    var json = File.ReadAllText(ThemesPath);
                    var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var list = JsonSerializer.Deserialize<List<DockTheme>>(json, opts);
                    if (list is { Count: > 0 }) return list;
                }
            }
            catch { }
            return Defaults();
        }

        public static DockTheme GetById(string id)
        {
            foreach (var t in LoadAll())
                if (string.Equals(t.Id, id, StringComparison.OrdinalIgnoreCase))
                    return t;
            return LoadAll()[0];
        }

        private static List<DockTheme> Defaults() => new()
        {
            new DockTheme { Id="pastel-cloud", Name="Pastel Cloud",
                GradientStart="#E6F3FEFF", GradientEnd="#F7E9FFFF", Accent="#B38CF5", BlurStrength=10, GlowIntensity=0.9,
                Surface="#14FFFFFF", SurfaceAlt="#0FFFFFFF", Border="#22FFFFFF", Shadow="#22000000" },
            new DockTheme { Id="neo-noir-glass", Name="Neo Noir Glass",
                GradientStart="#CC0E0E10", GradientEnd="#99070709", Accent="#AE88FF", BlurStrength=8, GlowIntensity=0.8,
                Surface="#10000000", SurfaceAlt="#08000000", Border="#22000000", Shadow="#33000000" }
        };
    }
}
