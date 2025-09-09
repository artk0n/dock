DockTop — Floating Top Dock (WPF, .NET 8)
===============================================

Features implemented:
- Hover trigger window (full-width × 8px) to slide dock in
- Dock window centered at top, default size 500×125, auto-sizes to content width
- WrapPanel with sample app icons
- Five premium hover behaviors:
  1) MacOS magnify (neighbor falloff via distance-based scaling)
  2) Matte highlight (subtle plate glow)
  3) Glow by app color (radial gradient + blur)
  4) Pressed-in (downshift while mouse is pressed)
  5) Liquid morph (gentle skew on hover)

How to build:
- Requires .NET 8 SDK on Windows
- Open a terminal in this folder and run:
    dotnet build
    dotnet run

Customize:
- Add your icons into Assets and call SetIconSource on each IconDockItem
- Change AppColor per icon for brand-colored glow
- Wire up Click events in MainWindow to launch real apps
- Tweak magnify parameters in MainWindow: MaxScale, Influence, GaussianSigma
