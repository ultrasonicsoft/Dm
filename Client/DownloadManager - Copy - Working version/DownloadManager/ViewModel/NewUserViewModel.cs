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
using Ultrasonic.DownloadManager.View;

namespace Ultrasonic.DownloadManager.ViewModel
{
    public class NewUserViewModel : ReactiveObject
    {
        private ServiceRepository _serviceRepository;
        private User _newUser;
        private NewUser _newUserView;

        public NewUser NewUserView
        {
            get { return _newUserView; }
            set { this.RaiseAndSetIfChanged(v => v.NewUserView, ref _newUserView, value); }
        }
        
        public DelegateCommand RegisterCommand { get; private set; }

        public User NewUser
        {
            get { return _newUser; }
            set { this.RaiseAndSetIfChanged(v => v.NewUser, ref _newUser, value); }
        }

        public NewUserViewModel()
        {
            _newUser = new User();
            RegisterCommand = new DelegateCommand(ExecuteRegisterCommand, () => true);
            _serviceRepository = new ServiceRepository();
        }
        private void ExecuteRegisterCommand()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            if (_serviceRepository.RegisterNewUser(NewUser.UserName, NewUser.Password, NewUser.Email))
            {
                MainWindow mainWindow = new MainWindow() {LoggedInUser = NewUser};
                mainWindow.Show();
                NewUserView.Close();
            }
            Mouse.OverrideCursor = null;
        }
        
    }
}
