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
using KinectV2MouseControl.Views.Tasks;

namespace KinectV2MouseControl.Views.Tasks
{
    /// <summary>
    /// Interaction logic for X_Rays.xaml
    /// </summary>
    public partial class X_Rays : Window
    {
        public X_Rays()
        {
            InitializeComponent();
        }

        private void Zoom_In(object sender, RoutedEventArgs e)
        {
            Picture.ZoomCentre(true);
        }
        private void Zoom_Out(object sender, RoutedEventArgs e)
        {
            Picture.ZoomCentre(false);
        }
    }
}
