using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Commands;
using ReactiveUI;
using Ultrasonic.DownloadManager.View;
using Ultrasonic.DownloadManager.View.SideCommands;

namespace Ultrasonic.DownloadManager.ViewModel
{
    enum SelectedCommand
    {
        MainView,
        DownloadHistory,
        Settings,
        MyAccount,
        Help,
        About
    }

    public class ContainerViewModel : ReactiveObject
    {
        private object currentView;

        private bool _isMainViewSelected;
        private bool _isDownloadHistorySelected;
        private bool _isSettingsSelected;
        private bool _isMyAccountSelected;
        private bool _isHelpSelected;
        private bool _isAboutSelected;
        private string currentViewName;

        private SelectedCommand currentlySelectedCommand;

        private MainDownloadView mainDownloadView;
        private DownloadHistoryView downloadHistoryView;
        private SettingView settingView;
        private MyAccountView myAccountView;
        private HelpView helpView;
        private AboutView aboutView ;

        public DelegateCommand ShowMainViewCommand { get; set; }
        public DelegateCommand ShowDownloadHistoryCommand { get; set; }
        public DelegateCommand ShowSettingsCommand { get; set; }
        public DelegateCommand ShowMyAccountCommand { get; set; }
        public DelegateCommand ShowHelpCommand { get; set; }
        public DelegateCommand ShowAboutCommand { get; set; }

        public bool IsMainViewSelected
        {
            get { return _isMainViewSelected; }
            set { this.RaiseAndSetIfChanged(v => v.IsMainViewSelected, ref _isMainViewSelected, value); }
        }

        public bool IsDownloadHistorySelected
        {
            get { return _isDownloadHistorySelected; }
            set { this.RaiseAndSetIfChanged(v => v.IsDownloadHistorySelected, ref _isDownloadHistorySelected, value); }
        }

        public bool IsSettingsSelected
        {
            get { return _isSettingsSelected; }
            set { this.RaiseAndSetIfChanged(v => v.IsSettingsSelected, ref _isSettingsSelected, value); }
        }

        public bool IsMyAccountSelected
        {
            get { return _isMyAccountSelected; }
            set { this.RaiseAndSetIfChanged(v => v.IsMyAccountSelected, ref _isMyAccountSelected, value); }
        }

        public bool IsHelpSelected
        {
            get { return _isHelpSelected; }
            set { this.RaiseAndSetIfChanged(v => v.IsHelpSelected, ref _isHelpSelected, value); }
        }

        public bool IsAboutSelected
        {
            get { return _isAboutSelected; }
            set { this.RaiseAndSetIfChanged(v => v.IsAboutSelected, ref _isAboutSelected, value); }
        }

        public object CurrentView
        {
            get { return currentView; }
            set { this.RaiseAndSetIfChanged(v => v.CurrentView, ref currentView, value); }
        }

        public string CurrentViewName
        {
            get { return currentViewName; }
            set { this.RaiseAndSetIfChanged(v => v.CurrentViewName, ref currentViewName, value); }
        }

        public ContainerViewModel()
        {
            mainDownloadView = new MainDownloadView();
            downloadHistoryView= new DownloadHistoryView();
            settingView = new SettingView();

            ShowMainViewCommand = new DelegateCommand(ExecuteShowMainViewCommand, ()=> true);
            ShowDownloadHistoryCommand = new DelegateCommand(ExecuteShowDownloadHistoryCommand, ()=> true);
            ShowSettingsCommand = new DelegateCommand(ExecuteShowSettingsCommand, () => true);
            ShowMyAccountCommand = new DelegateCommand(ExecuteShowMyAccountCommand, () => true);
            ShowHelpCommand = new DelegateCommand(ExecuteShowHelpCommand, () => true);
            ShowAboutCommand = new DelegateCommand(ExecuteShowAboutCommand, () => true);

            IsMainViewSelected = true;

            CurrentView = mainDownloadView;
        }

        private void ExecuteShowMainViewCommand()
        {
            CurrentView = mainDownloadView;
            UpdateCommandSelection(SelectedCommand.MainView);
        }

        private void ExecuteShowDownloadHistoryCommand()
        {
            CurrentView = downloadHistoryView;
            UpdateCommandSelection(SelectedCommand.DownloadHistory);
        }

        private void ExecuteShowSettingsCommand()
        {
            CurrentView = settingView;
            UpdateCommandSelection(SelectedCommand.Settings);
        }

        private void ExecuteShowMyAccountCommand()
        {
            CurrentView = myAccountView;
            UpdateCommandSelection(SelectedCommand.MyAccount);
        }

        private void ExecuteShowHelpCommand()
        {
            CurrentView = helpView;
            UpdateCommandSelection(SelectedCommand.Help);
        }

        private void ExecuteShowAboutCommand()
        {
            CurrentView = aboutView;
            UpdateCommandSelection(SelectedCommand.About);
        }
        private void UpdateCommandSelection(SelectedCommand selectedCommand)
        {
            switch (selectedCommand)
            {
                case SelectedCommand.MainView:
                    IsMainViewSelected = true;
                    IsDownloadHistorySelected = false;
                    IsSettingsSelected = false;
                    IsMyAccountSelected= false;
                    IsHelpSelected = false;
                    IsAboutSelected= false;
                    break;
                case SelectedCommand.DownloadHistory:
                    IsMainViewSelected = false;
                    IsDownloadHistorySelected = true;
                    IsSettingsSelected = false;
                    IsMyAccountSelected= false;
                    IsHelpSelected = false;
                    IsAboutSelected= false;
                    break;
                case SelectedCommand.Settings:
                    IsMainViewSelected = false;
                    IsDownloadHistorySelected = false;
                    IsSettingsSelected = true;
                    IsMyAccountSelected= false;
                    IsHelpSelected = false;
                    IsAboutSelected= false;
                    break;
                case SelectedCommand.MyAccount:
                    IsMainViewSelected = false;
                    IsDownloadHistorySelected = false;
                    IsSettingsSelected = false;
                    IsMyAccountSelected= true;
                    IsHelpSelected = false;
                    IsAboutSelected= false;
                    break;
                case SelectedCommand.Help:
                    IsMainViewSelected = false;
                    IsDownloadHistorySelected = false;
                    IsSettingsSelected = false;
                    IsMyAccountSelected= false;
                    IsHelpSelected = true;
                    IsAboutSelected= false;
                    break;
                case SelectedCommand.About:
                    IsMainViewSelected = false;
                    IsDownloadHistorySelected = false;
                    IsSettingsSelected = false;
                    IsMyAccountSelected= false;
                    IsHelpSelected = false;
                    IsAboutSelected= true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("selectedCommand");
            }
        }
    }
}
