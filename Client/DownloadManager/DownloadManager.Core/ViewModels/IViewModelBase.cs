using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Ultrasonic.DownloadManager.Core.Utils;

namespace Ultrasonic.DownloadManager.Core.ViewModels
{
    public interface IViewModelBase : INotifyPropertyChanged, IDataErrorInfo, IRaiseNotifyPropertyChanged
    {
        string ValidateProperty(string propertyName, IPropertyHandler propertyHandler);
    }
}
