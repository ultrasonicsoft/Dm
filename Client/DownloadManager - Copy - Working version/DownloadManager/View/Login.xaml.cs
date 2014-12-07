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
using Ultrasonic.DownloadManager.View;
using Ultrasonic.DownloadManager.ViewModel;

namespace Ultrasonic.DownloadManager
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            this.DataContext = new LoginViewModel(){LoginView = this};
            InitializeComponent();
        }

        private void _registerUser_OnClick(object sender, RoutedEventArgs e)
        {
            NewUser newUser = new NewUser();
            newUser.Show();
            this.Close();
        }
    }
}
