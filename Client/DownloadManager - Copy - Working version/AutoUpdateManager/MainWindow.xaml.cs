using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AutoUpdateManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string[] Args { get; set; }
        public MainWindow()
        {
            InitializeComponent();

        }

        private void StartUpdate()
        {
            string downloadManagerPath = Args[0];
            string downloadUrl = Args[1];
            string applicationName = Args[2];
            string ftpUserName = Args[3];
            string ftpUserPassword = Args[4];
            var runningProcess = Process.GetProcessesByName(applicationName);
            foreach (var process in runningProcess)
            {
                process.Kill();
            }

            //var result = Task.Factory.StartNew(() => DownloadLatestVersion(downloadUrl, ftpUserName, ftpUserPassword));
            //result.Wait();
            DownloadLatestVersion(downloadUrl, ftpUserName, ftpUserPassword, downloadManagerPath);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StartUpdate();
        }
        void DownloadLatestVersion(string downloadUrl, string ftpUserName, string ftpUserPassword, string downloadManagerPath)
        {
            var downloadParts = downloadUrl.Split('/');
            string fileName = System.IO.Path.GetTempPath() + downloadParts[downloadParts.Length - 1];
            var wc = new WebClient { Credentials = new NetworkCredential(ftpUserName, ftpUserPassword) };
            wc.DownloadFileCompleted += delegate(object sender, AsyncCompletedEventArgs args)
            {
                if (args.Cancelled)
                {
                    MessageBox.Show("Download cancelled.");
                }
                else
                    if (args.Error != null)
                    {
                        MessageBox.Show("Error while downloading file. " + args.Error.Message);
                    }
                    else
                    {
                        LblDownload.Content = "Applying new update...";
                        UnZipFileUsing7Zip(fileName, downloadManagerPath);
                        LaunchApplication(downloadManagerPath);
                    }
            };
            wc.DownloadProgressChanged += (sender, args) =>
            {
                //Console.WriteLine("{0} % complete", args.ProgressPercentage);
                _downloadProgress.Value = args.ProgressPercentage;
                txtProgressStatus.Text = string.Format("{0} % complete", args.ProgressPercentage);
            };

            wc.DownloadFileAsync(new Uri(downloadUrl), fileName);
        }

        private void LaunchApplication(string downloadManagerPath)
        {
            Process.Start(downloadManagerPath);
            Application.Current.Shutdown(0);
        }

        private void UnZipFileUsing7Zip(string fileName, string outputDir)
        {
            try
            {
                outputDir = System.IO.Path.GetDirectoryName(outputDir);
                DirectoryInfo dir = new DirectoryInfo(outputDir);
                foreach (var file in dir.EnumerateFiles())
                {
                    File.Delete(file.FullName);
                }
                ProcessStartInfo process = new ProcessStartInfo();
                process.FileName = ConfigurationManager.AppSettings["7Zip"];
                process.Arguments = string.Format(" x  \"{0}\" -o\"{1}\"", fileName,  outputDir);
                process.WindowStyle = ProcessWindowStyle.Hidden;
                Process p = Process.Start(process);
                p.WaitForExit();
                
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
