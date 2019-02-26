using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace KinectV2MouseControl.Views.Tasks
{
    /// <summary>
    /// Interaction logic for Background.xaml
    /// </summary>
    public partial class Background : Window
    {
        private KinectCursorViewModel viewModel;

        public Background()
        {
            InitializeComponent();


            viewModel = KinectCursorViewModel.Instance;
            DataContext = viewModel;

            viewModel.WriteData += ViewModel_WriteData;

            DispatcherTimer timer = new DispatcherTimer(
                new TimeSpan(0, 0, 1),
                DispatcherPriority.Normal,
                delegate
                {
                    time.Text = DateTime.Now.ToString("HH:mm:ss");
                },
                Dispatcher
            );
        }

        private void ViewModel_WriteData(object sender, Models.Data e)
        {
            pos.Text = e.ToString();
        }
    }
}
