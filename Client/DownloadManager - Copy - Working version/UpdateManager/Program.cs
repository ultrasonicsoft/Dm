using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UpdateManager
{
    class Program
    {
        static void Main(string[] args)
        {
            string downloadManagerPath = args[0];
            string downloadUrl = args[1];
            string applicationName = args[2];
            string ftpUserName = args[3];
            string ftpUserPassword = args[4];
            var runningProcess = Process.GetProcessesByName(applicationName);
            foreach (var process in runningProcess)
            {
                process.Kill();
            }

            var result = Task.Factory.StartNew(() => DownloadLatestVersion(downloadUrl, ftpUserName, ftpUserPassword));
            result.Wait();
            //Thread downloadThread = new Thread(() => DownloadLatestVersion(downloadUrl, ftpUserName, ftpUserPassword));
            //downloadThread.Start();
            //downloadThread.Join();
            Console.ReadLine();
        }

        async static Task DownloadLatestVersion(string downloadUrl, string ftpUserName, string ftpUserPassword)
        {
            var wc = new WebClient {Credentials = new NetworkCredential(ftpUserName, ftpUserPassword)};
            wc.DownloadFileCompleted += delegate(object sender, AsyncCompletedEventArgs args)
            {
                if (args.Cancelled)
                    Console.WriteLine("Download Cancelled.");
                else
                    if (args.Error != null)
                    {
                        Console.WriteLine("Error while downloading file. " + args.Error.Message);
                    }
                    else
                    {
                        Console.WriteLine("Download Completed.");
                    }
            };
            wc.DownloadProgressChanged += (sender, args) => Console.WriteLine("{0} % complete", args.ProgressPercentage);

            var downloadParts = downloadUrl.Split('/');
            string fileName = Path.GetTempPath() + downloadParts[downloadParts.Length - 1];
            await wc.DownloadFileTaskAsync(new Uri(downloadUrl), fileName);
        }
    }
}
