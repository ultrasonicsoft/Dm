using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Data;
using ReactiveUI;
using Ultrasonic.DownloadManager.Model;

namespace Ultrasonic.DownloadManager
{
    public class MainWindowViewModel : ReactiveObject
    {
        public ICollectionView FilteredFiles { get; private set; }

        private ObservableCollection<FTPFile> fileDownloads;

        public ObservableCollection<FTPFile> FileDownloads
        {
            get { return fileDownloads; }
            set { this.RaiseAndSetIfChanged(v => v.FileDownloads, ref fileDownloads, value); }
        }

        public MainWindowViewModel()
        {
            var _files = new List<FileInformation>
                                 {
                                     new FileInformation
                                         {
                                            FileName = "file1",
                                            IsSelected = true,
                                            //DownloadUri = new Uri("http://www.google.com")
                                            DownloadUri = string.Empty

                                         },
                                     new FileInformation
                                         {
                                             FileName = "file2",
                                            IsSelected = false,
                                           //DownloadUri = new Uri("http://www.google.com")
                                            DownloadUri = string.Empty
                                         },
                                     new FileInformation
                                         {
                                             FileName = "file3",
                                            IsSelected = true,
                                            //DownloadUri = new Uri("http://www.google.com")
                                            DownloadUri = string.Empty
                                         }
                                 };

            FilteredFiles = CollectionViewSource.GetDefaultView(_files);

            FileDownloads = new ObservableCollection<FTPFile>();

        }
    }
}
