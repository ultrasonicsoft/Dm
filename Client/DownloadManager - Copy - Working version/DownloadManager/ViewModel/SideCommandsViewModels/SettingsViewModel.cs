using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using ReactiveUI;


namespace Ultrasonic.DownloadManager.ViewModel.SideCommandsViewModels
{
    public class SettingsViewModel : ReactiveObject
    {
        private string _numberOfThreads;
        private string _downloadFolder;

        public string NumberOfThreads
        {
            get { return _numberOfThreads; }
            set { this.RaiseAndSetIfChanged(v => v.NumberOfThreads, ref _numberOfThreads, value); }
        }
        public string DownloadFolder
        {
            get { return _downloadFolder; }
            set { this.RaiseAndSetIfChanged(v => v.DownloadFolder, ref _downloadFolder, value); }
        }
        public DelegateCommand SaveChangesCommand { get; set; }

        public SettingsViewModel()
        {
            NumberOfThreads = Properties.Settings.Default.NumberOfThreads.ToString();
            
            string downloadFolderPath = Properties.Settings.Default.DefaultDownloadPath;
            
            if(string.IsNullOrEmpty(downloadFolderPath))
                downloadFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Downloaded\\";
            DownloadFolder = downloadFolderPath;

            SaveChangesCommand = new DelegateCommand(ExecuteSaveChangesCommands, () => true);
        }

        private void ExecuteSaveChangesCommands()
        {
            SaveNumberOfThreadsSetting();
            SaveDownloadFolderSetting();
        }

        private void SaveDownloadFolderSetting()
        {
            Properties.Settings.Default.DefaultDownloadPath = DownloadFolder;
        }

        private void SaveNumberOfThreadsSetting()
        {
            int numberOfThreads = 2;
            if (int.TryParse(NumberOfThreads, out numberOfThreads))
            {
                if (numberOfThreads < 2 && numberOfThreads > 10)
                {
                    MessageBox.Show("Number of threads should be in between 2-10 only!", "Download Manager",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                Properties.Settings.Default.NumberOfThreads = numberOfThreads;
                Properties.Settings.Default.Save();
            }
            else
            {
                MessageBox.Show("Number of threads should be in between 2-10 only!", "Download Manager",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
    }
}
