using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Ultrasonic.DownloadManager.ViewModel;

namespace Ultrasonic.DownloadManager.View
{
    /// <summary>
    /// Interaction logic for NewUser.xaml
    /// </summary>
    public partial class NewUser : Window
    {
        public NewUser()
        {
            this.DataContext = new NewUserViewModel(){NewUserView = this};

            InitializeComponent();
        }

        private void _cancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
