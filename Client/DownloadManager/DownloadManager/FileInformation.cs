using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Ultrasonic.DownloadManager
{
    public class FileInformation
    {
        private bool isSelected;
        private string fileName;
        private string downloadUri;
        private string downloadText;

        public string DownloadText
        {
            get { return "Download"; }
            set
            {
                downloadText = value;
                NotifyPropertyChanged("DownloadText");
            }
        }

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                NotifyPropertyChanged("IsSelected");
            }
        }

        public string FileName
        {
            get { return fileName; }
            set
            {
                fileName = value;
                NotifyPropertyChanged("FileName");
            }
        }

        public string DownloadUri
        {
            get { return downloadUri; }
            set
            {
                downloadUri = value;
                NotifyPropertyChanged("DownloadUri");
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Private Helpers

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
