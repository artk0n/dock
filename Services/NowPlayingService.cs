using System;
using System.Threading.Tasks;

#if WINDOWS10_0_19041_0_OR_GREATER
using Windows.Media.Control;
#endif

namespace DockTop.Services
{
    /// <summary>
    /// Tiny wrapper around Windows' Global System Media Transport Controls (GSMTC).
    /// Works on Windows 10/11 when targeting net8.0-windows10.0.19041.0 with CsWinRT.
    /// Safe no-op if API unavailable.
    /// </summary>
    public class NowPlayingService
    {
        public string Title  { get; private set; } = "";
        public string Artist { get; private set; } = "";
        public bool   Available { get; private set; } = false;

        public void Start() { /* reserved (no-op) */ }

        public async Task Refresh()
        {
            try
            {
#if WINDOWS10_0_19041_0_OR_GREATER
                var mgr = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
                var session = mgr?.GetCurrentSession();
                if (session != null)
                {
                    var props = await session.TryGetMediaPropertiesAsync();

                    Title = props?.Title ?? "";

                    // Combine artist + album artist without needing LINQ
                    var a = props?.Artist ?? "";
                    var b = props?.AlbumArtist ?? "";
                    if (!string.IsNullOrWhiteSpace(a) && !string.IsNullOrWhiteSpace(b))
                        Artist = a + ", " + b;
                    else
                        Artist = !string.IsNullOrWhiteSpace(a) ? a : b;

                    Available = !string.IsNullOrWhiteSpace(Title);
                    return;
                }
#endif
            }
            catch
            {
                // ignore and fall through to "not available"
            }

            // Fallback: nothing playing / unsupported platform
            Available = false;
            Title = "";
            Artist = "";
        }
    }
}
