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

namespace KinectV2MouseControl.Views.Tasks
{
    /// <summary>
    /// Interaction logic for Lighting_Control.xaml
    /// </summary>
    public partial class Lighting_Control : Window
    {
        public Lighting_Control()
        {
            InitializeComponent();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
