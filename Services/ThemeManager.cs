
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace DockTop.Services
{
    public class ThemeManager
    {
        private readonly Dictionary<string, ResourceDictionary> _themes = new();

        public ThemeManager()
        {
            // Register built-in themes (token dictionaries)
            _themes["PastelCloud"] = LoadFromTokens(
                bg:"#181a20", surface:"#1f2230", accent:"#7bd6ff", accent2:"#ffd1f7",
                text:"#e6ecf1", subtle:"#aeb6c1", glow:true);
            _themes["UltraGlowIce"] = LoadFromTokens(
                bg:"#0f1116", surface:"#131722", accent:"#bfe7ff", accent2:"#e6f2ff",
                text:"#f5fbff", subtle:"#9fb2c7", glow:true);
            _themes["NeoNoirGlass"] = LoadFromTokens(
                bg:"#0b0b0f", surface:"#11131c", accent:"#9a6bff", accent2:"#ff77aa",
                text:"#eae9f5", subtle:"#9792ad", glow:true);
            _themes["MatteGraphite"] = LoadFromTokens(
                bg:"#151515", surface:"#1a1a1a", accent:"#8bb3ff", accent2:"#8ef5c6",
                text:"#e8e8ea", subtle:"#9c9ca3", glow:false);
            _themes["AuroraIridescent"] = LoadFromTokens(
                bg:"#0b0e12", surface:"#13181f", accent:"#8de2d6", accent2:"#9aa8ff",
                text:"#eef8ff", subtle:"#97a3b3", glow:true);
            _themes["SolarNight"] = LoadFromTokens(
                bg:"#0c1014", surface:"#131923", accent:"#ffd479", accent2:"#ff9580",
                text:"#f5efe7", subtle:"#bbaea1", glow:false);
        }

        private ResourceDictionary LoadFromTokens(string bg, string surface, string accent, string accent2, string text, string subtle, bool glow)
        {
            var dict = new ResourceDictionary();
            dict["Color.Background"] = (Color)ColorConverter.ConvertFromString(bg);
            dict["Color.Surface"] = (Color)ColorConverter.ConvertFromString(surface);
            dict["Color.Accent"] = (Color)ColorConverter.ConvertFromString(accent);
            dict["Color.Accent2"] = (Color)ColorConverter.ConvertFromString(accent2);
            dict["Color.Text"] = (Color)ColorConverter.ConvertFromString(text);
            dict["Color.Subtle"] = (Color)ColorConverter.ConvertFromString(subtle);

            dict["Brush.Background"] = new SolidColorBrush((Color)dict["Color.Background"]);
            dict["Brush.Surface"] = new SolidColorBrush((Color)dict["Color.Surface"]);
            dict["Brush.Accent"] = new SolidColorBrush((Color)dict["Color.Accent"]);
            dict["Brush.Accent2"] = new SolidColorBrush((Color)dict["Color.Accent2"]);
            dict["Brush.Text"] = new SolidColorBrush((Color)dict["Color.Text"]);
            dict["Brush.Subtle"] = new SolidColorBrush((Color)dict["Color.Subtle"]);

            dict["Corner.Radius"] = 16.0;
            dict["Metrics.Padding"] = 8.0;
            dict["Metrics.IconSize"] = 32.0;
            dict["Effects.GlowEnabled"] = glow;

            return dict;
        }

        public IEnumerable<string> GetThemeNames() => _themes.Keys;

        public void Apply(string name)
        {
            if (!_themes.TryGetValue(name, out var rd)) return;

            // Merge into Application resources (replace previous tokens)
            var app = Application.Current;
            if (app == null) return;

            // Remove prior token keys
            var toRemove = new List<object>();
            foreach (System.Collections.DictionaryEntry entry in app.Resources)
            {
                var key = entry.Key as string;
                if (key != null && (key.StartsWith("Color.") || key.StartsWith("Brush.") || key.StartsWith("Corner.") || key.StartsWith("Metrics.") || key.StartsWith("Effects.")))
                    toRemove.Add(entry.Key);
            }
            foreach (var k in toRemove) app.Resources.Remove(k);

            foreach (var k in rd.Keys)
                app.Resources[k] = rd[k];
        }
    }
}
