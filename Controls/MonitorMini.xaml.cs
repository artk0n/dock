using System;
using System.Windows.Controls;
using System.Windows.Threading;
using DockTop.Utils;

namespace DockTop.Controls
{
    public partial class MonitorMini : UserControl
    {
        private PerfMonitor _perf = new();
        private DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(1000) };

        public MonitorMini()
        {
            InitializeComponent();
            _timer.Tick += (_, __) => Tick();
            _timer.Start();
        }

        private void Tick()
        {
            var s = _perf.Sample();
            Cpu.Value = s.CpuPercent;
            Ram.Value = s.RamPercent;
            Net.Text = ((int)(s.NetBps/1024)).ToString();
        }
    }
}
