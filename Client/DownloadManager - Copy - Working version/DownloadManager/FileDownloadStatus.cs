using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ultrasonic.DownloadManager
{
    public class FileDownloadStatus
    {
        private int totalParts;
        private int downloadedParts;
        private bool isCompleted;

        public bool IsCompleted
        {
            get { return isCompleted; }
            set { isCompleted = value; }
        }

        public string FirstFilePart { get; set; }

        public string FileName { get; set; }

        public int TotalParts
        {
            get { return totalParts; }
            set { totalParts= value; }
        }
        public int DownloadedParts
        {
            get { return downloadedParts; }
            set
            {
                downloadedParts = value;
                isCompleted = downloadedParts == totalParts;
            }
        }
    }
}
