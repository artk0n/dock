using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DockTop.Models;

namespace DockTop.Controls
{
    public partial class IconDockItem : UserControl
    {
        public event EventHandler? LaunchRequested;
        public event EventHandler? UnpinRequested;

        public DockItem Model { get; private set; } = new DockItem();

        public IconDockItem() { InitializeComponent(); }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(IconDockItem), new PropertyMetadata("App"));
        public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }

        public static readonly DependencyProperty AppColorProperty =
            DependencyProperty.Register(nameof(AppColor), typeof(Color), typeof(IconDockItem),
                new PropertyMetadata(Color.FromRgb(0x66,0xAA,0xFF)));
        public Color AppColor { get => (Color)GetValue(AppColorProperty); set => SetValue(AppColorProperty, value); }

        public void Bind(DockItem item, double iconSize)
        {
            Model = item;
            Title = item.Title;
            AppColor = (Color)ColorConverter.ConvertFromString(item.AppColor);
            if (!string.IsNullOrEmpty(item.IconPath) && File.Exists(item.IconPath))
            {
                IconImg.Source = new BitmapImage(new Uri(Path.GetFullPath(item.IconPath)));
            }
            Width = iconSize + 20;
            Height = iconSize + 20;
        }

        private void Unpin_Click(object sender, RoutedEventArgs e) => UnpinRequested?.Invoke(this, EventArgs.Empty);

        protected override void OnMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            LaunchRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
