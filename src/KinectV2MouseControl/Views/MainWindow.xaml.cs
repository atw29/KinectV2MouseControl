using KinectV2MouseControl.Views.Tasks;
using System.Windows;

namespace KinectV2MouseControl
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CursorViewModel.LoadSettings();

            new Background().Show();
            new Menu_Task().Show();
        }

        private void DefaultButton_Click(object sender, RoutedEventArgs e)
        {
            CursorViewModel.ResetToDefault();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            CursorViewModel.Quit();
        }


    }
}
