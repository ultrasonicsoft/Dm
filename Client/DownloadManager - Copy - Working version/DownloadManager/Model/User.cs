using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReactiveUI;

namespace Ultrasonic.DownloadManager.Model
{
    public class User : ReactiveObject
    {
        private string _userName;
        private string _password;
        private string _email;
        private double _subscribedQuota;
        private double _consumedQuota;

        public double SubscribedQuota
        {
            get { return _subscribedQuota; }
            set { this.RaiseAndSetIfChanged(v => v.SubscribedQuota, ref _subscribedQuota, value); }
        }
        public double ConsumedQuota
        {
            get { return _consumedQuota; }
            set { this.RaiseAndSetIfChanged(v => v.ConsumedQuota, ref _consumedQuota, value); }
        }

        public string Email
        {
            get { return _email; }
            set { this.RaiseAndSetIfChanged(v => v.Email, ref _email, value); }
        }

        public string UserName
        {
            get { return _userName; }
            set { this.RaiseAndSetIfChanged(v => v.UserName, ref _userName, value); }
        }

        public string Password
        {
            get { return _password; }
            set { this.RaiseAndSetIfChanged(v => v.Password, ref _password, value); }
        }

    }
}
