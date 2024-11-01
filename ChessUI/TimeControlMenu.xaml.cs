using ChessLogic;
using System.Windows;
using System.Windows.Controls;

namespace ChessUI
{
    /// <summary>
    /// Interaction logic for TimeControlMenu.xaml
    /// </summary>
    public partial class TimeControlMenu : UserControl
    {
        public event Action<TimeMode> TimeModeSelected;
        public TimeControlMenu()
        {
            InitializeComponent();
        }

        private void Standard_Click(object sender, RoutedEventArgs e)
        {
            TimeModeSelected?.Invoke(TimeMode.Standard);
        }

        private void Rapid_Click(object sender, RoutedEventArgs e)
        {
            TimeModeSelected?.Invoke(TimeMode.Rapid);
        }

        private void Blitz_Click(object sender, RoutedEventArgs e)
        {
            TimeModeSelected?.Invoke(TimeMode.Blitz);
        }       
    }
}
