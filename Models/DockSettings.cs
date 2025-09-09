
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace DockTop.Models
{
    public class DockSettings : INotifyPropertyChanged
    {
        private string _theme = "PastelCloud";
        private double _scale = 1.0;
        private bool _autoHide = false;
        private string _dockEdge = "Bottom"; // Top/Bottom/Left/Right
        private bool _enableAcrylic = true;
        private bool _enableGlow = true;
        private bool _labelsVisible = true;
        private bool _animations = true;
        private string _hotkey = "Ctrl+Space";

        public string Theme { get => _theme; set { _theme = value; OnPropertyChanged(); } }
        public double Scale { get => _scale; set { _scale = value; OnPropertyChanged(); } }
        public bool AutoHide { get => _autoHide; set { _autoHide = value; OnPropertyChanged(); } }
        public string DockEdge { get => _dockEdge; set { _dockEdge = value; OnPropertyChanged(); } }
        public bool EnableAcrylic { get => _enableAcrylic; set { _enableAcrylic = value; OnPropertyChanged(); } }
        public bool EnableGlow { get => _enableGlow; set { _enableGlow = value; OnPropertyChanged(); } }
        public bool LabelsVisible { get => _labelsVisible; set { _labelsVisible = value; OnPropertyChanged(); } }
        public bool Animations { get => _animations; set { _animations = value; OnPropertyChanged(); } }
        public string Hotkey { get => _hotkey; set { _hotkey = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
