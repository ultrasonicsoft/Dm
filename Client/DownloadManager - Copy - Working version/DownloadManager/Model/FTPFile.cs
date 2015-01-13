using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace Ultrasonic.DownloadManager.Model
{
    public class FTPFile : ReactiveObject
    {
        private string fileName;
        private string statusText;
        private long totalSize;
        private long downloadSize;
        private double progress;

        public double Progress
        {
            get { return progress; }
            set { this.RaiseAndSetIfChanged(v => v.Progress, ref progress, value); }
        }

        public long DownloadSize
        {
            get { return downloadSize; }
            set { this.RaiseAndSetIfChanged(v => v.DownloadSize, ref downloadSize, value); }
        }
        public long TotalSize
        {
            get { return totalSize; }
            set { this.RaiseAndSetIfChanged(v => v.TotalSize, ref totalSize, value); }
        }
        public string StatusText
        {
            get { return statusText; }
            set { this.RaiseAndSetIfChanged(v => v.StatusText, ref statusText, value); }
        }

        public string FileName
        {
            get { return fileName; }
            set { this.RaiseAndSetIfChanged(v => v.FileName, ref fileName, value); }
        }

    }
}
