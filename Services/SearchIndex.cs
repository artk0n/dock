
using System;
using System.Collections.Generic;
using System.Linq;

namespace DockTop.Services
{
    public class SearchIndex
    {
        private readonly Dictionary<string, Func<string, IEnumerable<(string title, string subtitle, Action action)>>> _providers = new();

        public void RegisterProvider(string name, Func<string, IEnumerable<(string title, string subtitle, Action action)>> provider)
        {
            _providers[name] = provider;
        }

        public IEnumerable<(string title,string subtitle, Action action)> Query(string text)
        {
            foreach (var p in _providers.Values)
            {
                IEnumerable<(string,string,Action)> result = Enumerable.Empty<(string,string,Action)>();
                try { result = p(text); } catch {}
                foreach (var x in result) yield return x;
            }
        }
    }
}
