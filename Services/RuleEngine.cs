
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using DockTop.Models;

namespace DockTop.Services
{
    public class RuleEngine
    {
        private readonly string _rulesFile;
        private readonly List<Rule> _rules = new();
        private readonly ThemeManager _theme;
        private readonly ToastService _toast;

        private DispatcherTimer _timer;
        private DateTime _lastHotCornerTrigger = DateTime.MinValue;

        public RuleEngine(ThemeManager theme, ToastService toast)
        {
            _theme = theme;
            _toast = toast;
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DockTop");
            Directory.CreateDirectory(folder);
            _rulesFile = Path.Combine(folder, "rules.json");
            Load();
            SetupTimers();
        }

        private void SetupTimers()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (s,e)=>Tick();
            _timer.Start();
        }

        private void Tick()
        {
            foreach (var r in _rules.Where(r=>r.Enabled))
            {
                switch (r.Trigger)
                {
                    case TimeTrigger t:
                        if (DateTime.Now.Minute % Math.Max(1,t.MinuteInterval) == 0 && DateTime.Now.Second == 0)
                            Execute(r);
                        break;
                    case HotCornerTrigger hc:
                        if (IsInHotCorner(hc.Corner) && (DateTime.Now - _lastHotCornerTrigger).TotalSeconds > 3)
                        {
                            _lastHotCornerTrigger = DateTime.Now;
                            Execute(r);
                        }
                        break;
                }
            }
        }

        
[System.Runtime.InteropServices.DllImport("user32.dll")]
private static extern bool GetCursorPos(out POINT lpPoint);
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
private struct POINT { public int X; public int Y; }

private bool IsInHotCorner(string corner)
{
    try
    {
        if (!GetCursorPos(out var p)) return false;
        int tol = 3;
        // Primary screen dimensions
        int sw = (int)SystemParameters.PrimaryScreenWidth;
        int sh = (int)SystemParameters.PrimaryScreenHeight;
        return corner switch
        {
            "TopLeft" => p.X <= 0 + tol && p.Y <= 0 + tol,
            "TopRight" => p.X >= sw - tol && p.Y <= 0 + tol,
            "BottomLeft" => p.X <= 0 + tol && p.Y >= sh - tol,
            "BottomRight" => p.X >= sw - tol && p.Y >= sh - tol,
            _ => false
        };
    } catch { return false; }
}

private void Execute(Rule r)
        {
            foreach (var a in r.Actions)
            {
                try
                {
                    switch (a)
                    {
                        case LaunchAction la:
                            Process.Start(new ProcessStartInfo { FileName = la.Path, Arguments = la.Args ?? "", UseShellExecute = true });
                            break;
                        case ThemeAction ta:
                            _theme.Apply(ta.ThemeName);
                            _toast.Info($"Theme '{ta.ThemeName}' applied via rule");
                            break;
                        case ToastAction to:
                            _toast.Info(to.Message);
                            break;
                    }
                } catch { }
            }
        }

        public void Load()
        {
            try
            {
                if (File.Exists(_rulesFile))
                {
                    var json = File.ReadAllText(_rulesFile);
                    var rules = JsonSerializer.Deserialize<List<Rule>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (rules != null) { _rules.Clear(); _rules.AddRange(rules); }
                }
                else
                {
                    // Seed with an example hot-corner rule
                    var sample = new Rule(
                        "sample-1",
                        "TopLeft → Toggle theme",
                        true,
                        new HotCornerTrigger("TopLeft"),
                        new List<Models.ActionDef> { new ThemeAction("NeoNoirGlass"), new ToastAction("Hot corner activated") }
                    );
                    _rules.Clear();
                    _rules.Add(sample);
                    Save();
                }
            } catch { }
        }

        public void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(_rules, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_rulesFile, json);
            } catch { }
        }

        public IReadOnlyList<Rule> Rules => _rules;
    }
}
