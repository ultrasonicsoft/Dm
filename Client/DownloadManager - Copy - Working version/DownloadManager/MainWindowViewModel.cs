using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using ReactiveUI;
using Ultrasonic.DownloadManager.Model;

namespace Ultrasonic.DownloadManager
{
    public class MainWindowViewModel : DependencyObject, INotifyPropertyChanged
    {
        public ICollectionView FilteredFiles { get; private set; }

        private ObservableCollection<FTPFile> fileDownloads;

        public ObservableCollection<FTPFile> FileDownloads
        {
            get { return fileDownloads; }
            set
            {
                if (value == fileDownloads)
                    return;
                fileDownloads = value; 
                OnPropertyChanged("FileDownloads"); 
            }
        }

        #region Public Dependency Properties

        public static readonly DependencyProperty ViewItemsSourceProperty = DependencyProperty.Register("ViewItemsSource", typeof(ObservableCollection<MyComboViewModel>), typeof(DownloadManagerView));
        public static readonly DependencyProperty ViewItemsSourceInformationProperty = DependencyProperty.Register("ViewItemsSourceInformation", typeof(ObservableCollection<MyComboViewModel>), typeof(DownloadManagerView));

        #endregion

        #region Public Properties

        public ObservableCollection<MyComboViewModel> ViewItemsSource
        {
            get { return (ObservableCollection<MyComboViewModel>) GetValue(ViewItemsSourceProperty); }
            set { SetValue(ViewItemsSourceProperty, value); }
        }
        public ObservableCollection<MyComboViewModel> ViewItemsSourceInformation
        {
            get { return (ObservableCollection<MyComboViewModel>)GetValue(ViewItemsSourceInformationProperty); }
            set { SetValue(ViewItemsSourceInformationProperty, value); }
        }

        #endregion

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

            //Fill Categories combo box
            ViewItemsSource = new ObservableCollection<MyComboViewModel>(ComboViewShowcaseHelper.GetSource(0));

            //Fill Information combo box
            ViewItemsSourceInformation = new ObservableCollection<MyComboViewModel>(ComboViewShowcaseHelper.GetSource(1));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string Property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(Property));
            }
        } 
    }
}
