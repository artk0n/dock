using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;

namespace DockTop.Utils
{
    public class PerfMonitor
    {
        public record SampleData(double CpuPercent, double RamPercent, double NetBps);
        private TimeSpan _lastCpu = TimeSpan.Zero;
        private DateTime _lastTime = DateTime.UtcNow;
        private long _lastBytes = GetTotalBytes();

        public SampleData Sample()
        {
            var proc = Process.GetCurrentProcess();
            var now = DateTime.UtcNow;
            double cpu = 0;
            try {
                var total = proc.TotalProcessorTime;
                var deltaCpu = (total - _lastCpu).TotalMilliseconds;
                var deltaTime = (now - _lastTime).TotalMilliseconds * Environment.ProcessorCount;
                cpu = deltaTime > 0 ? Math.Clamp(deltaCpu / deltaTime * 100.0, 0, 100) : 0;
                _lastCpu = total; _lastTime = now;
            } catch { }
            double ram = Math.Clamp(proc.WorkingSet64 / (8.0*1024*1024*1024) * 100.0, 0, 100);
            double net = 0;
            try { var nowBytes = GetTotalBytes(); net = Math.Max(0, nowBytes - _lastBytes); _lastBytes = nowBytes; } catch { }
            return new SampleData(cpu, ram, net);
        }
        static long GetTotalBytes() => NetworkInterface.GetAllNetworkInterfaces()
            .Where(n=>n.OperationalStatus==OperationalStatus.Up)
            .Select(n=>n.GetIPStatistics().BytesReceived + n.GetIPStatistics().BytesSent).Sum();
    }
}
