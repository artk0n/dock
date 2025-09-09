using System.Windows;
namespace DockTop
{
    public partial class TriggerWindow : Window
    {
        public TriggerWindow()
        {
            InitializeComponent();
            Left = 0;
            Top = 0;
            Width = SystemParameters.PrimaryScreenWidth;
        }
    }
}
