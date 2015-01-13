using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ultrasonic.DownloadManager
{
    class AsyncFileDownloader
    {
         DateTime lastUpdate;
        long lastBytes = 0;
        async Task MyTask(string serverUri, string fileName)
        {
            var wc = new WebClient();
            wc.Credentials = new NetworkCredential("balram", "balram");

            wc.DownloadProgressChanged += (sender, args) =>
            {
                Console.WriteLine("{0} - {1} % complete", ProgressChanged(args.BytesReceived), args.ProgressPercentage);
            };

            Task.Delay(150000).ContinueWith(ant =>
            {
                wc.CancelAsync();
                Console.WriteLine("ABORTED!");
            });

            await wc.DownloadFileTaskAsync(serverUri, fileName);
        }

        long ProgressChanged(long bytes)
        {
            if (lastBytes == 0)
            {
                lastUpdate = DateTime.Now;
                lastBytes = bytes;
                return 0;
            }

            var now = DateTime.Now;
            var timeSpan = now - lastUpdate;
            var bytesChange = bytes - lastBytes;
            var bytesPerSecond = timeSpan.Seconds != 0 ? bytesChange / timeSpan.Seconds : 0;

            lastBytes = bytes;
            lastUpdate = now;

            return bytesPerSecond;
        }
    }
}
