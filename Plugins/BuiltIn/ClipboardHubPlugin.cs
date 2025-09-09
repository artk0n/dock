
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using DockTop.Plugins;
using DockTop.Services;

namespace DockTop.Plugins.BuiltIn
{
    public class ClipboardHubPlugin : IPlugin
    {
        public string Id => "clipboard.hub";
        public string Name => "Clipboard Hub";
        public string Version => "0.1.0";
        public bool Enabled { get; set; }

        private readonly LinkedList<string> _history = new();
        private PluginContext _ctx;
        private DispatcherTimer _timer;

        public void Initialize(PluginContext context)
        {
            _ctx = context;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(800) };
            _timer.Tick += (s,e)=>PollClipboard();
            _timer.Start();

            _ctx.Search.RegisterProvider("Clipboard", Query);
        }

        private void PollClipboard()
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    var t = Clipboard.GetText();
                    if (!string.IsNullOrWhiteSpace(t))
                    {
                        if (_history.First == null || _history.First.Value != t)
                        {
                            _history.AddFirst(t.Length > 500 ? t.Substring(0,500) : t);
                            while (_history.Count > 50) _history.RemoveLast();
                        }
                    }
                }
            } catch {}
        }

        private IEnumerable<(string title,string subtitle, Action action)> Query(string q)
        {
            foreach (var item in _history)
            {
                if (string.IsNullOrEmpty(q) || item.Contains(q, StringComparison.OrdinalIgnoreCase))
                {
                    var preview = item.Replace("\r"," ").Replace("\n"," ");
                    if (preview.Length > 80) preview = preview.Substring(0,80) + "…";
                    yield return ($"Paste: {preview}", "Clipboard Hub", () => {
                        try { Clipboard.SetText(item); _ctx.Toasts.Info("Copied to clipboard"); } catch {}
                    });
                }
            }
        }

        public IEnumerable<PluginCommand> GetCommands()
        {
            yield return new PluginCommand("clear", "Clear Clipboard History", "Removes all stored entries.");
        }

        public void Dispose()
        {
            try { _timer?.Stop(); } catch {}
        }
    }
}
