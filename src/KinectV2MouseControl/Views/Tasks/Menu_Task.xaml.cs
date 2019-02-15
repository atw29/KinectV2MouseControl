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
    /// Interaction logic for Menu_Task.xaml
    /// </summary>
    public partial class Menu_Task : Window
    {
        public Menu_Task()
        {
            InitializeComponent();
        }

        private bool IsMainMenu = true;

        private void SetMainMenu()
        {
            IsMainMenu = true;
            BackButton.IsEnabled = false;
            label.Content = "Main Menu";

            Item1.Content = "Lighting Control";
            Item2.Content = "Interaction Parameters";
            Item3.Content = "Option 3";
            Item4.Content = "Data Search";
            Item5.Content = "Option 5";
        }

        private void SetDataMenu()
        {
            IsMainMenu = false;
            BackButton.IsEnabled = true;
            label.Content = "Patient Data";

            Item1.Content = "Patient Information";
            Item2.Content = "Operation List";
            Item3.Content = "Organ Information";
            Item4.Content = "X-Ray 1";
            Item5.Content = "X-Ray 2";
        }

        private void Menu_Task_Loaded(object sender, RoutedEventArgs e)
        {
            SetMainMenu();
        }
        private void Item1_Click(object sender, RoutedEventArgs e)
        {
            if (!IsMainMenu)
            {
                new MockUp().Show();
            }
            else
            {
                new Lighting_Control().Show();
            }
        }
        private void Item2_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Item3_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Item4_Click(object sender, RoutedEventArgs e)
        {
            if (IsMainMenu)
            {
                SetDataMenu();
            }
            else
            {
                new X_Rays().Show();
            }
        }

        private void Item5_Click(object sender, RoutedEventArgs e)
        {
            if (!IsMainMenu)
            {
                new X_Rays().Show();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsMainMenu)
            {
                SetMainMenu();
            }
        }
    }
}
