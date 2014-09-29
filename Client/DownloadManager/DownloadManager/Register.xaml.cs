using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using WatiN.Core;

namespace Ultrasonic.DownloadManager
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Register : Window
    {
        private StringBuilder loggingInformation = new StringBuilder();
        internal bool RegistrationSuccessful { get; set; }
        internal string AccountId { get; set; }
        internal string Password { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Login"/> class.
        /// </summary>
        public Register()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the RequestNavigate event of the TermsOfPaysafecard control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Navigation.RequestNavigateEventArgs"/> instance containing the event data.</param>
        private void TermsOfPaysafecard_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start("http://www.paysafecard.com/index.php?id=823&L=39");
        }

        /// <summary>
        /// Handles the Click event of the btnRegister control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            Wait frmWait = new Wait();
            try
            {
                MessageBox.Show(Helper.PLEASE_WAIT_LOG);
                Mouse.OverrideCursor = Cursors.Wait;

                //Settings.Instance.MakeNewIeInstanceVisible = false;
                
                loggingInformation.Append(DateTime.Now.ToShortTimeString() + Environment.NewLine);
                IE explore = new IE();
                explore.ClearCache();
                explore.ClearCookies();
                explore.WaitForComplete();

                loggingInformation.Append(Helper.CREATING_10MINUTE_ID_LOG + Environment.NewLine);
                explore.GoTo(Helper._10_MINUTE_MAIL_URL);
                explore.WaitForComplete();
                string emailID = explore.TextField(Find.ById(Helper._10_MINUTE_EMAIL_TEXTBOX_ID)).Value;

                loggingInformation.Append(Helper.EMAIL_ID_IS_LOG + emailID + Environment.NewLine);

                loggingInformation.Append(Helper.CREATING_UPLOADED_ACCOUNT_LOG + Environment.NewLine);

                CreateUploadLogin(emailID);

                loggingInformation.Append(Helper.ACCOUNT_CREATED_LOG + Environment.NewLine);

                loggingInformation.Append(Helper.READING_WELCOME_MAIL_LOG + Environment.NewLine);

                bool isConfirmationMailArrived = false;

                string confirmationUrl = string.Empty;

                while (!isConfirmationMailArrived)
                {
                    Thread.Sleep(20000);
                    explore.GoTo(Helper.CONFIRMATION_EMAIL_URL);
                    explore.WaitForComplete();

                    loggingInformation.Append(Helper.EXTRACTING_CONFIRMATION_URL_LOG + Environment.NewLine);

                     confirmationUrl = GetConfirmationMailId(explore.Html);
                    if (!string.IsNullOrEmpty(confirmationUrl))
                    {
                        isConfirmationMailArrived = true;
                    }
                }
                loggingInformation.Append(Helper.CONFIRMATION_URL_LOG + Environment.NewLine);

                loggingInformation.Append(Helper.HITTING_CONFIRMATION_URL_LOG + Environment.NewLine);

                IE uploadedExplorer = new IE(confirmationUrl);

                loggingInformation.Append(Helper.READING_USERNAME_PASSWORD_LOG + Environment.NewLine);

                Thread.Sleep(20000);
                explore.GoTo(Helper.PASSWORD_EMAIL_URL);
                explore.WaitForComplete();

                string newAccountId;
                string newPassword;

                loggingInformation.Append(Helper.EXTRACTING_USERNAME_PASSWORD_LOG + Environment.NewLine);

                GetAccountIdAndPassword(explore, out newAccountId, out newPassword);

                loggingInformation.Append(Helper.USERNAME_LOG + newAccountId + Environment.NewLine + Helper.PASSWORD_LOG + newPassword + Environment.NewLine);

                PremiumPeriod period = PremiumPeriod._48Hours;
                if (rbtn48Hours.IsChecked == true)
                {
                    period = PremiumPeriod._48Hours;
                }
                else if (rbtn1Month.IsChecked == true)
                {
                    period = PremiumPeriod._1Month;
                }
                else if (rbtn3Months.IsChecked == true)
                {
                    period = PremiumPeriod._3Months;
                }
                else if (rbtn6Months.IsChecked == true)
                {
                    period = PremiumPeriod._6Months;
                }
                else if (rbtn1Year.IsChecked == true)
                {
                    period = PremiumPeriod._1Year;
                }

                string paysafe_pin1 = txtpaysafe_pin1.Text;
                string paysafe_pin2 = txtpaysafe_pin2.Text;
                string paysafe_pin3 = txtpaysafe_pin3.Text;
                string paysafe_pin4 = txtpaysafe_pin4.Text;

                bool finalStautus = RegisterForPremiumAccountWithReference(uploadedExplorer, period, emailID, paysafe_pin1, paysafe_pin2, paysafe_pin3, paysafe_pin4);
                //bool status = RegisterForPremiumAccount(newAccountId, newPassword, period, emailID, paysafe_pin1, paysafe_pin2, paysafe_pin3, paysafe_pin4);

                explore.Close();
                MessageBox.Show(Helper.REGISTRATION_SUCCESSFUL_LOG);
                this.Close();
                RegistrationSuccessful = true;
                AccountId = newAccountId;
                Password = newPassword;
            }
            catch (Exception ex)
            {
                loggingInformation.Append(Environment.NewLine + "Exception");
                loggingInformation.Append(Environment.NewLine + ex.Message);
                File.WriteAllText(Helper.LOGGING_FILE_PATH, loggingInformation.ToString());
                
                MessageBox.Show(ex.Message);
            }
            finally
            {
                File.WriteAllText(Helper.LOGGING_FILE_PATH, loggingInformation.ToString());
                Mouse.OverrideCursor = null;
                frmWait.Close();
            }
        }

        /// <summary>
        /// Registers for premium account with reference.
        /// </summary>
        /// <param name="uploadedExplorer">The uploaded explorer.</param>
        /// <param name="period">The period.</param>
        /// <param name="emailID">The email ID.</param>
        /// <param name="paysafe_pin1">The paysafe_pin1.</param>
        /// <param name="paysafe_pin2">The paysafe_pin2.</param>
        /// <param name="paysafe_pin3">The paysafe_pin3.</param>
        /// <param name="paysafe_pin4">The paysafe_pin4.</param>
        /// <returns></returns>
        private bool RegisterForPremiumAccountWithReference(IE uploadedExplorer, PremiumPeriod period, string emailID, string paysafe_pin1, string paysafe_pin2, string paysafe_pin3, string paysafe_pin4)
        {
            bool result = false;
            try
            {
                loggingInformation.Append("Redirect to Register page " + Environment.NewLine);

                uploadedExplorer.GoTo("http://Ul.to/ref/6335349");
                uploadedExplorer.WaitForComplete();

                bool isValidPeriod = false;
                switch (period)
                {
                    case PremiumPeriod._48Hours:
                        loggingInformation.Append("Selecting premium registration duration 2 days " + Environment.NewLine);
                        uploadedExplorer.GoTo(Helper._2_DAYS_PREMIUM_DURATION_URL);
                        isValidPeriod = true;
                        break;
                    case PremiumPeriod._1Month:
                        loggingInformation.Append("Selecting premium registration duration 1 month " + Environment.NewLine);
                        uploadedExplorer.GoTo(Helper._1_MONTH_PREMIUM_DURATION_URL);
                        isValidPeriod = true;
                        break;
                    case PremiumPeriod._3Months:
                        loggingInformation.Append("Selecting premium registration duration 3 month " + Environment.NewLine);
                        uploadedExplorer.GoTo(Helper._3_MONTHS_PREMIUM_DURATION_URL);
                        isValidPeriod = true;
                        break;
                    case PremiumPeriod._6Months:
                        loggingInformation.Append("Selecting premium registration duration 6 month " + Environment.NewLine);
                        uploadedExplorer.GoTo(Helper._6_MONTHS_PREMIUM_DURATION_URL);
                        isValidPeriod = true;
                        break;
                    case PremiumPeriod._1Year:
                        loggingInformation.Append("Selecting premium registration duration 1 year " + Environment.NewLine);
                        uploadedExplorer.GoTo(Helper._1_YEAR_PREMIUM_DURATION_URL);
                        isValidPeriod = true;
                        break;
                }

                if (isValidPeriod)
                {
                    uploadedExplorer.WaitForComplete();

                    loggingInformation.Append("Check terms and condition check box " + Environment.NewLine);
                    uploadedExplorer.CheckBox(Find.ById("pinForm:acceptTerms")).Checked = true;
                    uploadedExplorer.WaitForComplete();

                    loggingInformation.Append("Entering paysafe pin 1: " + paysafe_pin1 + Environment.NewLine);
                    uploadedExplorer.TextField(Find.ById("pinForm:rn01")).TypeText(paysafe_pin1);
                    uploadedExplorer.WaitForComplete();

                    loggingInformation.Append("Entering paysafe pin 2: " + paysafe_pin2 + Environment.NewLine);
                    uploadedExplorer.TextField(Find.ById("pinForm:rn02")).TypeText(paysafe_pin2);
                    uploadedExplorer.WaitForComplete();

                    loggingInformation.Append("Entering paysafe pin 3: " + paysafe_pin3 + Environment.NewLine);
                    uploadedExplorer.TextField(Find.ById("pinForm:rn03")).TypeText(paysafe_pin3);
                    uploadedExplorer.WaitForComplete();

                    loggingInformation.Append("Entering paysafe pin 4: " + paysafe_pin4 + Environment.NewLine);
                    uploadedExplorer.TextField(Find.ById("pinForm:rn04")).TypeText(paysafe_pin4);
                    uploadedExplorer.WaitForComplete();

                    loggingInformation.Append("Submitting paysafe pin code for registration " + Environment.NewLine);
                    uploadedExplorer.Button(Find.ById("pinForm:pay")).ClickNoWait();
                    uploadedExplorer.WaitForComplete();
                    loggingInformation.Append("Submitting finished " + Environment.NewLine);

                }
                Thread.Sleep(10000);
                loggingInformation.Append("Logging out of uploaded.net " + Environment.NewLine);
                uploadedExplorer.GoTo("http://uploaded.net/logout");
                uploadedExplorer.WaitForComplete();
                loggingInformation.Append("Log out successful. " + Environment.NewLine);

                uploadedExplorer.Close();
                uploadedExplorer.WaitForComplete();
                result = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return result;
        }

        /// <summary>
        /// Gets the account id and password.
        /// </summary>
        /// <param name="explorer">The explorer.</param>
        /// <param name="newAccountId">The new account id.</param>
        /// <param name="newPassword">The new password.</param>
        private void GetAccountIdAndPassword(IE explorer, out string newAccountId, out string newPassword)
        {
            newAccountId = string.Empty;
            newPassword = string.Empty;
            try
            {
                string htmlContent = explorer.Html;

                string[] allLines = htmlContent.Split('\n');
                foreach (string line in allLines)
                {
                    if (line.Contains("Account-ID:") || line.Contains("Password:"))
                    {
                        string accountId = line.Trim();
                        string[] tempLines = line.Split('>');
                        foreach (string innerLine in tempLines)
                        {
                            if (innerLine.Contains("Account-ID:"))
                            {
                                string temp = innerLine.Replace("<", string.Empty).Replace("BR", string.Empty).Replace("br", string.Empty).Trim();
                                newAccountId = temp.Split(':')[1].Trim();
                            }
                            if (innerLine.Contains("Password:"))
                            {
                                string temp = innerLine.Replace("<", string.Empty).Replace("BR", string.Empty).Replace("br", string.Empty).Trim();
                                newPassword = temp.Split(':')[1].Trim();
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Gets the confirmation mail id.
        /// </summary>
        /// <param name="htmlContent">Content of the HTML.</param>
        /// <returns></returns>
        private string GetConfirmationMailId(string htmlContent)
        {
            string confirmationUrl = string.Empty;

            string[] allLines = htmlContent.Split('\n');
            foreach (string line in allLines)
            { 
                if (line.Contains("http://uploaded.net/welcome"))
                {
                    confirmationUrl = line.Replace("<br>", string.Empty).Replace("</br>", string.Empty).Trim();
                    break;                  
                }
            }
            return confirmationUrl;
        }

        /// <summary>
        /// Creates the upload login.
        /// </summary>
        /// <param name="emailID">The email ID.</param>
        private void CreateUploadLogin(string emailID)
        {
            try
            {
                Dictionary<string, string> RequestCookies = new Dictionary<string, string>();
                string url = "http://uploaded.net/io/register/free";
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.Referer = "http://uploaded.net/register";
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                request.Method = "POST";
                request.ServicePoint.Expect100Continue = false;

                string postData = "mail=" + emailID;
                //string postData = "mail=freelanc1.john@gmail.com";

                var data = Encoding.ASCII.GetBytes(postData);
                request.ContentLength = data.Length;

                request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate,sdch");
                string domain = string.Empty;

                var cookieContainer = new CookieContainer();

                if (RequestCookies != null && RequestCookies.Count > 0)
                {
                    foreach (string key in RequestCookies.Keys)
                    {
                        var cookie = new Cookie(key, RequestCookies[key], "/", domain);
                        cookieContainer.Add(cookie);
                    }
                }
                request.CookieContainer = cookieContainer;

                var requestStream = request.GetRequestStream();
                requestStream.Write(data, 0, data.Length);
                requestStream.Close();

                var myHttpWebResponse = (HttpWebResponse)request.GetResponse();
                
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }

    /// <summary>
    /// PremiumPeriod
    /// </summary>
    enum PremiumPeriod
    {
        /// <summary>
        /// 48 Hours
        /// </summary>
        _48Hours,
        /// <summary>
        /// 1 Month
        /// </summary>
        _1Month,
        /// <summary>
        /// 3 Months
        /// </summary>
        _3Months,
        /// <summary>
        /// 6 Months
        /// </summary>
        _6Months,
        /// <summary>
        /// 1 Year
        /// </summary>
        _1Year
    }
}
