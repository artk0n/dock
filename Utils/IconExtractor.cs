
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace DockTop.Utils
{
    public static class IconExtractor
    {
        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int ExtractIconEx(string lpszFile, int nIconIndex,
            out IntPtr phiconLarge, out IntPtr phiconSmall, uint nIcons);

        public static BitmapSource? FromExecutable(string path, bool large = true)
        {
            try
            {
                IntPtr largeIco; IntPtr smallIco;
                var count = ExtractIconEx(path, 0, out largeIco, out smallIco, 1);
                if (count > 0)
                {
                    using var ico = Icon.FromHandle(large ? largeIco : smallIco);
                    using var bmp = ico.ToBitmap();
                    return ToBitmapSource(bmp);
                }
            } catch {}
            return null;
        }

        private static BitmapSource ToBitmapSource(Bitmap bitmap)
        {
            using var ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Seek(0, SeekOrigin.Begin);
            var img = new BitmapImage();
            img.BeginInit();
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.StreamSource = ms;
            img.EndInit();
            img.Freeze();
            return img;
        }
    }
}
