
using System;
using System.Collections.Generic;

namespace DockTop.Models
{
    public record Rule(string Id, string Name, bool Enabled, Trigger Trigger, List<ActionDef> Actions);
    public abstract record Trigger;
    public record TimeTrigger(int MinuteInterval) : Trigger;
    public record HotCornerTrigger(string Corner) : Trigger; // "TopLeft", "TopRight", "BottomLeft", "BottomRight"

    public abstract record ActionDef;
    public record LaunchAction(string Path, string Args) : ActionDef;
    public record ThemeAction(string ThemeName) : ActionDef;
    public record ToastAction(string Message) : ActionDef;
}
