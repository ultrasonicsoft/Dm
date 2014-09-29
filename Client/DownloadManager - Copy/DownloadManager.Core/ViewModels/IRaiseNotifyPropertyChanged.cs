using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ultrasonic.DownloadManager.Core.ViewModels
{
    public interface IRaiseNotifyPropertyChanged
    {
        void RaisePropertyChanged(string propertyName);
    }
}
