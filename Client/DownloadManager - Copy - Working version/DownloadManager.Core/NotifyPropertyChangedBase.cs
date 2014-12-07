using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Ultrasonic.DownloadManager.Core
{
    [Serializable]
    public class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var evt = PropertyChanged;
            if (evt != null)
            {
                evt(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
