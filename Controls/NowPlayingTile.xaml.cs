using System;
using System.Windows.Controls;
using System.Windows.Threading;
using DockTop.Services;

namespace DockTop.Controls
{
    public partial class NowPlayingTile : UserControl
    {
        private readonly NowPlayingService _svc = new();
        private readonly DispatcherTimer _uiTimer = new() { Interval = TimeSpan.FromMilliseconds(900) };

        public NowPlayingTile()
        {
            InitializeComponent();

            Loaded += async (_, __) =>
            {
                _svc.Start();
                await _svc.Refresh();          // initial pull
                UpdateText();

                _uiTimer.Tick += async (_, __2) =>
                {
                    await _svc.Refresh();
                    UpdateText();
                };
                _uiTimer.Start();
            };

            Unloaded += (_, __) => _uiTimer.Stop();
        }

        private void UpdateText()
        {
            Txt.Text = _svc.Available
                ? $"{_svc.Title} — {_svc.Artist}".TrimEnd(' ', '—')
                : "Not playing";
            Txt.ToolTip = Txt.Text;
        }
    }
}
