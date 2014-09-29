using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Data;

namespace Ultrasonic.DownloadManager
{
    public class MainWindowViewModel
    {
        public ICollectionView FilteredFiles { get; private set; }


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


        }
    }
}
