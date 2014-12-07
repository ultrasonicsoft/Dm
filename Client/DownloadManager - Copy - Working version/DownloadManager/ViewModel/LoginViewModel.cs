using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using ReactiveUI;
using Ultrasonic.DownloadManager.Model;
using Ultrasonic.DownloadManager.Repository;

namespace Ultrasonic.DownloadManager.ViewModel
{
    public class LoginViewModel : ReactiveObject
    {
        private ServiceRepository _serviceRepository;
        private User _loginUser;
        private Login _loginView;

        public Login LoginView
        {
            get { return _loginView; }
            set { _loginView = value; }
        }

        public DelegateCommand LoginCommand { get; private set; }
        public User LoginUser
        {
            get { return _loginUser; }
            set { this.RaiseAndSetIfChanged(v => v.LoginUser, ref _loginUser, value); }
        }
        public LoginViewModel()
        {
            _loginUser = new User{UserName = "admin", Password = "admin"};
            LoginCommand = new DelegateCommand(ExecuteLoginCommand, CanLoginCommandExecute);
            _serviceRepository = new ServiceRepository();
        }

        private bool CanLoginCommandExecute()
        {
            return true;
        }

        private void ExecuteLoginCommand()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            if (_serviceRepository.IsValidLogin(LoginUser.UserName, LoginUser.Password))
            {
                MainWindow mainWindow = new MainWindow(){LoggedInUser = LoginUser};
                mainWindow.Show();
                LoginView.Close();
            }
            else
            {
                MessageBox.Show("Login failed");
            }
            Mouse.OverrideCursor = null;
        }
    }
}
