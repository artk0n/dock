using System;

namespace DockTop.Models
{
    // Keep this minimal and compile-safe. Settings.Current (elsewhere) holds an instance of this.
    public class UserSettings
    {
        // Dock placement and sizing
        public string DockEdge { get; set; } = "Top"; // Top, Bottom, Left, Right
        public int ThicknessHorizontal { get; set; } = 64;
        public int ThicknessVertical   { get; set; } = 72;
        public int TilesPerPage        { get; set; } = 0;   // 0 = auto

        // Behavior
        public bool AutoHide               { get; set; } = true;
        public int  AutoHidePeekPx         { get; set; } = 2;
        public bool ClickThroughWhenHidden { get; set; } = true;
        public bool Topmost                { get; set; } = true;
        public bool AutoStartWithWindows   { get; set; } = false;

        // Appearance
        public double CornerRadius   { get; set; } = 10;
        public double GlowOpacity    { get; set; } = 0.25;
        public int    RevealAnimationMs { get; set; } = 180;
        public int    HideAnimationMs   { get; set; } = 140;

        // Hotkeys
        public string? HotkeyToggle { get; set; } = "Ctrl+Alt+D";
        public string? HotkeySearch { get; set; } = "Ctrl+K";

        // Profile/multi-monitor info
        public ProfileSettings Profile { get; set; } = new ProfileSettings();

        public class ProfileSettings
        {
            public string ScreenId { get; set; } = "Primary";
        }
    }
}
