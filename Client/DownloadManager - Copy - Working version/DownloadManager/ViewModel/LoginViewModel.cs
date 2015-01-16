using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

            CheckUpdateAvailable();
        }

        private bool CanLoginCommandExecute()
        {
            return true;
        }

        private void ExecuteLoginCommand()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                if (_serviceRepository.IsValidLogin(LoginUser.UserName, LoginUser.Password))
                {
                    MainWindow mainWindow = new MainWindow() { LoggedInUser = LoginUser };
                    mainWindow.Show();
                    LoginView.Close();
                }
                else
                {
                    MessageBox.Show("Login failed");
                }
            }
            catch (Exception exception)
            {
                LogHelper.logger.Error(exception);
            }
            Mouse.OverrideCursor = null;
        }

        private void CheckUpdateAvailable()
        {
            try
            {
                string latestVersion = _serviceRepository.GetLatestVersion();
                if (latestVersion.Equals(Assembly.GetEntryAssembly().GetName().Version.ToString()) == false)
                {
                    var userResponse =
                        MessageBox.Show("You are using old version. You must update your application now. Agree?",
                            "Download Manager", MessageBoxButton.YesNo);
                    if (userResponse == MessageBoxResult.Yes)
                    {
                        UpdateApplication();
                    }
                    else
                    {
                        Application.Current.Shutdown(0);
                    }
                }
            }
            catch (Exception exception)
            {
                LogHelper.logger.Equals(exception);
            }
        }

        private void UpdateApplication()
        {
            try
            {
                string updateManagerPath = ConfigurationManager.AppSettings["UpdateManagerPath"];
                string downloadManagerPath = ConfigurationManager.AppSettings["DownloadManagerPath"];
                string downloadUrl = _serviceRepository.GetDownloadUrl();
                string applicationName = Assembly.GetEntryAssembly().GetName().Name + ".exe";
                string ftpUserName = _serviceRepository.GetFtpUserName();
                string ftpUserPassword = _serviceRepository.GetFtpUserName();

                string arguments = string.Format("\"{0}\" \"{1}\" \"{2}\" \"{3}\" \"{4}\"", downloadManagerPath, 
                    downloadUrl, applicationName,ftpUserName,ftpUserPassword);

                // Launch update application with arguments
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = updateManagerPath;
                startInfo.Arguments = arguments;
                Process.Start(startInfo);

                // Exit application
                Application.Current.Shutdown(0);

            }
            catch (Exception exception)
            {
                LogHelper.logger.Error(exception);
            }
        }
    }
}
