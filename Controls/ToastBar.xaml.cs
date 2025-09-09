using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace DockTop.Controls
{
    public partial class ToastBar : UserControl
    {
        public ToastBar() { InitializeComponent(); }

        public async void Show(string message, int ms = 3000)
        {
            Msg.Text = message;
            this.Visibility = Visibility.Visible;
            this.Opacity = 0;
            if (this.Resources["ShowAnim"] is Storyboard show) show.Begin(this);
            await Task.Delay(ms);
            if (this.Resources["HideAnim"] is Storyboard hide)
            {
                hide.Completed += (s, e) => this.Visibility = Visibility.Collapsed;
                hide.Begin(this);
            }
            else this.Visibility = Visibility.Collapsed;
        }
    }
}
