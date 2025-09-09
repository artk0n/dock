using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DockTop.Services
{
    public static class ThemeApplier
    {
        public static void ApplyTheme(string themeId, Window? window = null)
        {
            var t = ThemeStore.GetById(themeId);
            var rd = Application.Current.Resources;

            Color accent = (Color)ColorConverter.ConvertFromString(t.Accent);
            rd["AccentBrush"] = new SolidColorBrush(accent);

            Color c1 = (Color)ColorConverter.ConvertFromString(t.GradientStart);
            Color c2 = (Color)ColorConverter.ConvertFromString(t.GradientEnd);
            var bg = new LinearGradientBrush(c1, c2, 45);
            bg.MappingMode = BrushMappingMode.RelativeToBoundingBox;
            bg.StartPoint = new Point(0, 0);
            bg.EndPoint = new Point(1, 1);

            // animated gradient offset (slow)
            var anim = new DoubleAnimation
            {
                From = 0, To = 1, Duration = TimeSpan.FromSeconds(18),
                RepeatBehavior = RepeatBehavior.Forever, AutoReverse = true,
                EasingFunction = new BackEase { Amplitude = 0.2, EasingMode = EasingMode.EaseInOut }
            };
            bg.GradientStops.Add(new GradientStop(c1, 0));
            bg.GradientStops.Add(new GradientStop(c2, 1));
            bg.GradientStops[0].BeginAnimation(GradientStop.OffsetProperty, anim);

            rd["DockBackgroundBrush"] = bg;
            rd["DockBlurRadius"] = (double)t.BlurStrength;
            rd["DockGlowIntensity"] = (double)t.GlowIntensity;

            if (!string.IsNullOrWhiteSpace(t.Surface)) rd["SurfaceBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(t.Surface!));
            if (!string.IsNullOrWhiteSpace(t.SurfaceAlt)) rd["SurfaceAltBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(t.SurfaceAlt!));
            if (!string.IsNullOrWhiteSpace(t.Border)) rd["BorderBrushToken"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(t.Border!));
            if (!string.IsNullOrWhiteSpace(t.Shadow)) rd["ShadowBrushToken"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(t.Shadow!));

            if (window != null) window.Background = (Brush)rd["DockBackgroundBrush"];
        }
    }
}
