using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using Ultrasonic.DownloadManager.DownloadManagerService;
using Ultrasonic.DownloadManager.Model;
using WatiN.Core.DialogHandlers;
using WatiN.Core.Logging;


namespace Ultrasonic.DownloadManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Members

        private Dictionary<string, string> FileUriMapping;
        private List<string> allDownloadedFileParts = new List<string>();
        private int totalDownloadParts = 0;
        private static int totalPartsDownloaded = 0;
        private User _loggedInUser;
        private MainWindowViewModel viewModel;
        private static long bytes_total;
        static DateTime lastUpdate;
        static string fileName = @"C:\Downloaded Files\balram.jpg";
        static string serverUri = "ftp://balram@95.89.83.26:64888/ftp/Movies/TestMovie/balram.JPG";
        static long lastBytes = 0;
        private string ftpServerAddress = "ftp://balram@95.89.83.26:64888";
        private bool isDownloading = false;
        private List<FileDownloadStatus> _fileDownloadStatuses = new List<FileDownloadStatus>();
        string downloadFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Downloaded\\";
        private string ftpUserName = "balram";
        private string ftpUserPassword = "balram";
        private int numberOfRunningThreads;
        private int maxAllowedRunningThreads = 2;
        public User LoggedInUser
        {
            get { return _loggedInUser; }
            set { _loggedInUser = value; }
        }

        #endregion

        #region Public Dependency Properties

        public static readonly DependencyProperty ViewItemsSourceProperty = DependencyProperty.Register("ViewItemsSource", typeof(ObservableCollection<MyComboViewModel>), typeof(MainWindow));
        public static readonly DependencyProperty ViewItemsSourceInformationProperty = DependencyProperty.Register("ViewItemsSourceInformation", typeof(ObservableCollection<MyComboViewModel>), typeof(MainWindow));

        #endregion

        #region Public Properties

        public ObservableCollection<MyComboViewModel> ViewItemsSource
        {
            get { return (ObservableCollection<MyComboViewModel>)GetValue(ViewItemsSourceProperty); }
            set { SetValue(ViewItemsSourceProperty, value); }
        }
        public ObservableCollection<MyComboViewModel> ViewItemsSourceInformation
        {
            get { return (ObservableCollection<MyComboViewModel>)GetValue(ViewItemsSourceInformationProperty); }
            set { SetValue(ViewItemsSourceInformationProperty, value); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {

            LogHelper.logger.Info("In MainWindow constructor.");
            viewModel = new MainWindowViewModel();
            this.DataContext = viewModel;
            InitializeComponent();

            txtVersionInfo.Text = Assembly.GetEntryAssembly().GetName().Version.ToString();
            //Fill Categories combo box
            ViewItemsSource = new ObservableCollection<MyComboViewModel>(ComboViewShowcaseHelper.GetSource(0));

            //Fill Information combo box
            ViewItemsSourceInformation = new ObservableCollection<MyComboViewModel>(ComboViewShowcaseHelper.GetSource(1));
            LogHelper.logger.Info("MainWindow object constructed.");
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the DropDownClosed event of the cbCategories control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void cbCategories_DropDownClosed(object sender, EventArgs e)
        {
            UpdateFileListForCategories();
        }

        /// <summary>
        /// Handles the DropDownClosed event of the cbInformation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void cbInformation_DropDownClosed(object sender, EventArgs e)
        {
            UpdateFileListForCategories();

        }

        /// <summary>
        /// Called when [hyper link click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnHyperLinkClick(object sender, RoutedEventArgs e)
        {
            Task.Run(() => EnqueueDownload(e));
        }

        private void EnqueueDownload(RoutedEventArgs e)
        {
            try
            {
                List<string> downloadUriParts = null;
                Application.Current.Dispatcher.Invoke(() => downloadUriParts = ((Hyperlink)e.OriginalSource).NavigateUri.OriginalString.Split(Helper.DOWNLOAD_URI_SEPARATOR_CHAR)
                        .ToList<string>());

                string fileName = string.Empty;
                string finalDownloadUrl = string.Empty;

                string actualFileName = downloadUriParts[downloadUriParts.Count - 1].Split('#')[1];
                int counter = 1;
                LogHelper.logger.Info(string.Format("Downloading file: {0}. Total parts of files are {1}.", actualFileName,
                    downloadUriParts.Count));

                _fileDownloadStatuses.Add(new FileDownloadStatus
                {
                    TotalParts = downloadUriParts.Count,
                    FileName = actualFileName,
                    FirstFilePart = string.Concat(downloadFolderPath, "\\", actualFileName, ".7z.001")
                });

                LogHelper.logger.Info("Added File to be downloaded in FileDownload queue.");

                totalDownloadParts = downloadUriParts.Count;
                totalPartsDownloaded = 0;

                numberOfRunningThreads = 0;

                foreach (string downloadUri in downloadUriParts)
                {
                    finalDownloadUrl = downloadUri.Contains("#") ? downloadUri.Split('#')[0] : downloadUri;

                    //fileName = tempPath + "\\" + downloadUri.Split('/')[downloadUri.Split('/').Length - 1];
                    if (downloadUriParts.Count < 10)
                    {
                        fileName = downloadFolderPath + "\\" + actualFileName + ".7z.00" + counter.ToString();
                    }
                    else if (downloadUriParts.Count >= 10 && downloadUriParts.Count < 100)
                    {
                        fileName = downloadFolderPath + "\\" + actualFileName + ".7z.0" + counter.ToString();
                    }
                    else if (downloadUriParts.Count >= 100 && downloadUriParts.Count < 1000)
                    {
                        fileName = downloadFolderPath + "\\" + actualFileName + ".7z." + counter.ToString();
                    }
                    if (counter == 1)
                        ;
                    counter++;

                    var parts = finalDownloadUrl.Split('/');
                    string filePartName = string.Empty;
                    if (parts.Length > 0)
                    {
                        filePartName = parts[parts.Length - 1];
                    }

                    LogHelper.logger.Info(string.Format("Adding file part {0} to download async in queue.", filePartName));

                    Application.Current.Dispatcher.Invoke(() => viewModel.FileDownloads.Add(new FTPFile() { FileName = filePartName }));
                    
                    // Start downloading file part Async
                    DownloadFTPFileAsync(finalDownloadUrl, fileName, filePartName, actualFileName);
                    numberOfRunningThreads++;
                    
                    while (numberOfRunningThreads >= maxAllowedRunningThreads)
                    {
                        ; // Spin CPU until its downloading
                    }
                }
            }
            catch (Exception exception)
            {
                LogHelper.logger.Error(exception);
            }
        }

        private void DownloadFTPFileAsync(string finalDownloadUrl, string fileName, string filePartName, string actualFileName)
        {
            try
            {
                //Task.Factory.StartNew(() => StartDownloadFileParts(finalDownloadUrl, fileName, filePartName, actualFileName));
                StartDownloadFileParts(finalDownloadUrl, fileName, filePartName, actualFileName);
                Console.WriteLine("done!");
            }
            catch (Exception exception)
            {
                LogHelper.logger.Error(exception);
            }
        }

        void StartDownloadFileParts(string finalDownloadUrl, string fileName, string filePartName, string actualFileName)
        {
            try
            {
                var wc = new WebClient();
                wc.Credentials = new NetworkCredential(ftpUserName, ftpUserPassword);
                LogHelper.logger.Info(string.Format("Connecting to server with credentials: {0}/{1}", ftpUserName, ftpUserPassword));

                wc.DownloadFileCompleted += delegate(object sender, AsyncCompletedEventArgs args)
                {
                    if (args.Cancelled)
                        MessageBox.Show("Download Cancelled.");
                    else if (args.Error != null)
                    {
                        MessageBox.Show("Error while downloading file. " + args.Error.Message);
                        return;
                    }
                    else
                    {
                        var currentFile = _fileDownloadStatuses.FirstOrDefault(v => v.FileName == actualFileName);
                        if (currentFile != null)
                        {
                            currentFile.DownloadedParts++;
                            if (currentFile.IsCompleted)
                            {
                                MessageBox.Show("completed");
                                UnZipFileUsing7Zip(currentFile.FirstFilePart, downloadFolderPath, "balram");
                                _fileDownloadStatuses.Remove(currentFile);

                                var currentFileProgressBar =
                                    viewModel.FileDownloads.FirstOrDefault(x => x.FileName == filePartName && x.IsCompleted==false);
                                if (currentFileProgressBar != null)
                                {
                                    currentFileProgressBar.IsCompleted = true;
                                    //viewModel.FileDownloads.Remove(currentFileProgressBar);
                                }
                            }
                            numberOfRunningThreads--;
                        }
                    }
                };

                wc.DownloadProgressChanged += (sender, args) =>
                {
                    var currentProgressBar = viewModel.FileDownloads.FirstOrDefault(x => x.FileName == filePartName);
                    if (currentProgressBar != null)
                    {
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
                        currentProgressBar.Progress = args.ProgressPercentage;
                        currentProgressBar.StatusText = string.Format("{0} % complete", args.ProgressPercentage);
                    }
                    //Console.WriteLine("{0} - {1} % complete", ProgressChanged(args.BytesReceived), args.ProgressPercentage);
                    Console.WriteLine("{0} % complete", args.ProgressPercentage);
                    //var progressDispatcher = progressBar1.Dispatcher;
                    //Action action = () =>
                    //{
                    //    progressBar1.Value = args.ProgressPercentage;
                    //};
                    //if (progressDispatcher.CheckAccess())
                    //    action();
                    //else
                    //{
                    //    progressDispatcher.Invoke(action);
                    //}

                };

                //Task.Delay(150000).ContinueWith(ant =>
                //{
                //    wc.CancelAsync();
                //    Console.WriteLine("ABORTED!");
                //});

                //await wc.DownloadFileTaskAsync(serverUri, fileName);

                finalDownloadUrl = ftpServerAddress + finalDownloadUrl;

                LogHelper.logger.Info(string.Format("Starting download => Weburl: {0}, Local FileName: {1}", finalDownloadUrl, fileName));
                wc.DownloadFileAsync(new Uri(finalDownloadUrl), fileName);
            }
            catch (WebException exception)
            {
                LogHelper.logger.Error(exception);
            }
            catch (Exception exception)
            {
                LogHelper.logger.Error(exception);
            }
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
        /// <summary>
        /// Handles the Click event of the btnDownloadAll control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnDownloadAll_Click(object sender, RoutedEventArgs e)
        {
            //string fileName = @"""C:\Program Files\7-Zip\testzipt.7z""";
            string fileName = @"""F:\Balram Data\Important\Project\Client in C#.NET wpf\Supporting files\0450cdcc28""";

            string outputDir = @"f:\zipOutput";
            //string password = "password";
            string password = "uploaded.net";

            UnZipFileUsing7Zip(fileName, outputDir, password);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates the file list for categories.
        /// </summary>
        private void UpdateFileListForCategories()
        {
            try
            {
                DownloadDataProviderServiceClient client = new DownloadDataProviderServiceClient();
                var categoryData = client.GetFileList();
                XDocument doc = XDocument.Parse(categoryData);

                //XDocument doc = XDocument.Load(XmlReader.Create(Helper.FILELIST_XML_PATH));

                //Read FTP user name
                string ftpUserName = string.Empty;
                foreach (var item in doc.Descendants((XName.Get(Helper.FTP_USER_NODE_TEXT))))
                {
                    ftpUserName = item.Value;
                }
                //Read FTP password
                string ftpPassword = string.Empty;
                foreach (var item in doc.Descendants((XName.Get(Helper.FTP_PASSWORD_NODE_TEXT))))
                {
                    ftpPassword = item.Value;
                }

                ObservableCollection<FileInformation> fileList = new ObservableCollection<FileInformation>();

                string currentFileFromServer = string.Empty;
                string[] allParts;
                List<string> selectedFiles = new List<string>();

                var allFiles = doc.Descendants(XName.Get("file"));
                foreach (var currentFile in allFiles)
                {
                    if (currentFile.HasAttributes && currentFile.Attribute(XName.Get(Helper.FILENAME_TEXT)) != null)
                    {
                        string fileName = currentFile.Attribute(XName.Get(Helper.FILENAME_TEXT)).Value;
                        string locationName = currentFile.Descendants(XName.Get("location")).ToArray()[0].Value as string;
                        if (!string.IsNullOrEmpty(locationName))
                        {
                            string[] locationParts = locationName.Split('/');
                            if (string.IsNullOrEmpty(locationParts[locationParts.Length - 1]))
                            {
                                locationName = locationParts[locationParts.Length - 2];
                            }
                            else
                            {
                                locationName = locationParts[locationParts.Length - 1];
                            }
                            foreach (var selectedItem in cbCategories.SelectedViewItems)
                            {
                                if (locationName.ToLower().Contains(selectedItem.ToString().ToLower()))
                                {
                                    string downloadUris = GetDownloadUriFromFinishXml(fileName);

                                    if (fileList.Any((x) => x.FileName == currentFileFromServer) == false)
                                    {
                                        fileList.Add(new FileInformation() { FileName = fileName, DownloadUri = downloadUris });
                                        //fileList.Add(new FileInformation() { FileName = currentFileFromServer, DownloadUri = new Uri(path.Value.Replace("/var/www/", Helper.SERVER_URL)) });
                                    }
                                }
                            }
                        }
                    }

                    #region Fill Matching information
                    //Add files matching information
                    foreach (var currentInformation in currentFile.Descendants(XName.Get("informations")).Descendants(XName.Get("item")))
                    {
                        string fileName = currentFile.Attribute(XName.Get(Helper.FILENAME_TEXT)).Value;
                        if (currentInformation.HasAttributes && currentInformation.Attribute(XName.Get("title")) != null)
                        {
                            string information = currentInformation.Attribute(XName.Get("title")).Value;

                            foreach (var selectedItem in cbInformation.SelectedViewItems)
                            {
                                if (information.ToLower().Contains(selectedItem.ToString().ToLower()))
                                {
                                    string downloadUris = GetDownloadUriFromFinishXml(fileName);

                                    if (fileList.Any((x) => x.FileName == fileName) == false)
                                    {
                                        fileList.Add(new FileInformation() { FileName = fileName, DownloadUri = downloadUris });
                                        //fileList.Add(new FileInformation() { FileName = currentFileFromServer, DownloadUri = new Uri(path.Value.Replace("/var/www/", Helper.SERVER_URL)) });
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }

                dgFileList.ItemsSource = fileList;
                FileUriMapping = new Dictionary<string, string>();
                foreach (var item in fileList)
                {
                    FileUriMapping.Add(item.FileName, item.DownloadUri);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Gets the download URI from finish XML.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        private string GetDownloadUriFromFinishXml(string fileName)
        {
            StringBuilder downloadUris = new StringBuilder();

            DownloadDataProviderServiceClient client = new DownloadDataProviderServiceClient();
            var categoryData = client.GetDownloadInformation();
            XDocument doc = XDocument.Parse(categoryData);

            //XDocument doc = XDocument.Load(XmlReader.Create(Helper.FINISH_XML_PATH));
            foreach (var currentFile in doc.Descendants(XName.Get("file")))
            {
                if (currentFile.HasAttributes && currentFile.Attribute(XName.Get("filename")) != null)
                {
                    if (currentFile.Attribute(XName.Get("filename")).Value == fileName)
                    {
                        int linkCounter = 0;
                        var allLinks = currentFile.Descendants(XName.Get("link"));
                        foreach (var parts in allLinks)
                        {
                            downloadUris.Append(parts.Value);
                            if (linkCounter != allLinks.Count() - 1)
                            {
                                downloadUris.Append("^");
                            }
                            else
                            {
                                downloadUris.Append("#");
                                downloadUris.Append(fileName);
                            }

                            linkCounter++;
                        }
                    }
                }
            }

            return downloadUris.ToString();
        }

        /// <summary>
        /// Gets the final download URL.
        /// </summary>
        /// <param name="inputDownloadUrl">The input download URL.</param>
        /// <returns></returns>
        private string GetFinalDownloadUrl(string inputDownloadUrl)
        {
            var client = new CookieAwareWebClient();
            client.BaseAddress = @"http://uploaded.net/io/login";
            var loginData = new NameValueCollection();
            //loginData.Add("id", "7549995");
            //loginData.Add("pw", "admin");
            loginData.Add("id", Properties.Settings.Default.AccountID);
            loginData.Add("pw", Properties.Settings.Default.Password);
            client.UploadValues("http://uploaded.net/io/login", "POST", loginData);

            //Now you are logged in and can request pages    
            //string htmlSource = client.DownloadString("http://ul.to/0eooiv5q");
            string htmlSource = client.DownloadString(inputDownloadUrl);

            string finalDownloadUrl = string.Empty;

            string[] allLines = htmlSource.Split('\n');
            foreach (string currentLine in allLines)
            {
                if (currentLine.Trim(new char[] { '\n' }).Contains("form method=\"post\" action=\"http"))
                {
                    string temp = currentLine.Trim(new char[] { '\t' }).Replace("<form method=\"post\" action=", string.Empty);
                    int startIndex = temp.IndexOf("\"");
                    int endIndex = temp.IndexOf("\"", startIndex + 1);
                    finalDownloadUrl = temp.Substring(startIndex + 1, endIndex - startIndex - 1);
                    break;
                }
            }

            return finalDownloadUrl;
        }

        //public void DownloadFileFromSFTP(string downloadUrl, string localFileName)
        //{
        //    try
        //    {
        //        string host = "95.89.83.26";
        //        string username = "balram";
        //        string password = "balram";
        //        string remoteFileName = downloadUrl;

        //        using (var sftp = new SftpClient(host, 64888, username, password))
        //        {
        //            sftp.Connect();

        //            using (var file = File.OpenWrite(localFileName))
        //            {
        //                sftp.DownloadFile(remoteFileName, file);
        //            }

        //            sftp.Disconnect();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //}

        /// <summary>
        /// Handles the DownloadProgressChanged event of the client control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Net.DownloadProgressChangedEventArgs"/> instance containing the event data.</param>
        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            //progressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());
        }

        /// <summary>
        /// Handles the DownloadFileCompleted event of the client control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.AsyncCompletedEventArgs"/> instance containing the event data.</param>
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            totalPartsDownloaded++;
            if (totalDownloadParts == totalPartsDownloaded)
            {
                CheckAllFilePartsDownloaded();
                //MessageBox.Show("Download finished.");
                //MessageBox.Show("Starting unzip");
                //UnZipFileUsing7Zip(allDownloadedFileParts[0], Helper.OUTPUT_ZIP_FOLDER, Helper.DEFAULT_PASSWORD);               
            }
        }

        /// <summary>
        /// Checks all file parts downloaded.
        /// </summary>
        private void CheckAllFilePartsDownloaded()
        {
            int i = 0;
            UnZipFileUsing7Zip(allDownloadedFileParts[0], Helper.OUTPUT_ZIP_FOLDER, Helper.DEFAULT_PASSWORD);
        }

        /// <summary>
        /// Uns the zip file using7 zip.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="outputDir">The output dir.</param>
        /// <param name="password">The password.</param>
        private void UnZipFileUsing7Zip(string fileName, string outputDir, string password)
        {
            try
            {
                ProcessStartInfo process = new ProcessStartInfo();
                process.FileName = ConfigurationManager.AppSettings["7Zip"]; ;
                process.Arguments = string.Format(" x  {0} -p{1} -o{2}", fileName, password, outputDir);
                process.WindowStyle = ProcessWindowStyle.Hidden;
                Process p = Process.Start(process);
                p.WaitForExit();
                //while (!p.HasExited) ;
                MessageBox.Show("File unzipped.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Adds the file to zip.
        /// </summary>
        /// <param name="zipFilename">The zip filename.</param>
        /// <param name="fileToAdd">The file to add.</param>
        private void AddFileToZip(string zipFilename, string fileToAdd)
        {
            using (Package zip = System.IO.Packaging.Package.Open(zipFilename, FileMode.OpenOrCreate))
            {
                string destFilename = ".\\" + System.IO.Path.GetFileName(fileToAdd);
                Uri uri = PackUriHelper.CreatePartUri(new Uri(destFilename, UriKind.Relative));
                if (zip.PartExists(uri))
                {
                    zip.DeletePart(uri);
                }
                PackagePart part = zip.CreatePart(uri, "", CompressionOption.Normal);
                using (FileStream fileStream = new FileStream(fileToAdd, FileMode.Open, FileAccess.Read))
                {
                    using (Stream dest = part.GetStream())
                    {
                        CopyStream(fileStream, dest);
                    }
                }
            }
        }

        /// <summary>
        /// Copies the stream.
        /// </summary>
        /// <param name="inputStream">The input stream.</param>
        /// <param name="outputStream">The output stream.</param>
        private void CopyStream(System.IO.FileStream inputStream, System.IO.Stream outputStream)
        {
            long bufferSize = inputStream.Length < Helper.BUFFER_SIZE ? inputStream.Length : Helper.BUFFER_SIZE;
            byte[] buffer = new byte[bufferSize];
            int bytesRead = 0;
            long bytesWritten = 0;
            while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                outputStream.Write(buffer, 0, bytesRead);
                bytesWritten += bufferSize;
            }
        }
        #endregion

    }

    /// <summary>
    /// CookieAwareWebClient
    /// </summary>
    public class CookieAwareWebClient : WebClient
    {
        private CookieContainer cookie = new CookieContainer();

        /// <summary>
        /// Returns a <see cref="T:System.Net.WebRequest"/> object for the specified resource.
        /// </summary>
        /// <param name="address">A <see cref="T:System.Uri"/> that identifies the resource to request.</param>
        /// <returns>
        /// A new <see cref="T:System.Net.WebRequest"/> object for the specified resource.
        /// </returns>
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                (request as HttpWebRequest).CookieContainer = cookie;
            }
            return request;
        }
    }
}
