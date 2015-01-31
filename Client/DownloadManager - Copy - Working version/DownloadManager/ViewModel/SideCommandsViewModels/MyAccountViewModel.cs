using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace Ultrasonic.DownloadManager.ViewModel.SideCommandsViewModels
{
    public class MyAccountViewModel : ReactiveObject
    {
        private string userName;

        public string UserName
        {
            get { return userName; }
            set { this.RaiseAndSetIfChanged(v => v.UserName, ref userName, value); }
        }

        public MyAccountViewModel()
        {
            UserName = Properties.Settings.Default.LoggedInUser;
        }
    }
}
