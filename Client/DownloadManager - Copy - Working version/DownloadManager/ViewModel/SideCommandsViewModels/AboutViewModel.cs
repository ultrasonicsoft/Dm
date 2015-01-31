using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace Ultrasonic.DownloadManager.ViewModel.SideCommandsViewModels
{
    public class AboutViewModel : ReactiveObject
    {
        private string appVersion;

        public string AppVersion
        {
            get { return appVersion; }
            set { this.RaiseAndSetIfChanged(v => v.AppVersion, ref appVersion, value); }
        }

        public AboutViewModel()
        {
            AppVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
        }
    }
}
