using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using WatiN.Core;
using WatiN.Core.Native.Windows;
using Window = System.Windows.Window;

namespace Ultrasonic.DownloadManager
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Register : Window
    {
        //private StringBuilder loggingInformation = new StringBuilder();
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
            //RegisterOldWay();
            RegiterNewWay();
        }

        private void RegiterNewWay()
        {
            Wait frmWait = new Wait();
            try
            {
                MessageBox.Show(Helper.PLEASE_WAIT_LOG);
                Mouse.OverrideCursor = Cursors.Wait;

                //Settings.Instance.MakeNewIeInstanceVisible = false;

                IE explore = new IE();
                explore.ClearCache();
                explore.ClearCookies();
                explore.WaitForComplete();
                explore.ShowWindow(NativeMethods.WindowShowStyle.Maximize);


                LogHelper.logger.Info("Redirect to Register page ");

                explore.GoTo("http://Ul.to/ref/6335349");
                explore.WaitForComplete();

                var period = PremiumPeriod._48Hours;
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

                bool isValidPeriod = false;
                switch (period)
                {
                    case PremiumPeriod._48Hours:
                        LogHelper.logger.Info("Selecting premium registration duration 2 days ");
                        explore.GoTo(Helper._2_DAYS_PREMIUM_DURATION_URL);
                        isValidPeriod = true;
                        break;
                    case PremiumPeriod._1Month:
                        LogHelper.logger.Info("Selecting premium registration duration 1 month ");
                        explore.GoTo(Helper._1_MONTH_PREMIUM_DURATION_URL);
                        isValidPeriod = true;
                        break;
                    case PremiumPeriod._3Months:
                        LogHelper.logger.Info("Selecting premium registration duration 3 month ");
                        explore.GoTo(Helper._3_MONTHS_PREMIUM_DURATION_URL);
                        isValidPeriod = true;
                        break;
                    case PremiumPeriod._6Months:
                        LogHelper.logger.Info("Selecting premium registration duration 6 month ");
                        explore.GoTo(Helper._6_MONTHS_PREMIUM_DURATION_URL);
                        isValidPeriod = true;
                        break;
                    case PremiumPeriod._1Year:
                        LogHelper.logger.Info("Selecting premium registration duration 1 year");
                        explore.GoTo(Helper._1_YEAR_PREMIUM_DURATION_URL);
                        isValidPeriod = true;
                        break;
                }

                string email = ConfigurationManager.AppSettings["email"];
                explore.TextField(Find.ById("free-mail")).Value = email;
                string firstName = ConfigurationManager.AppSettings["firstName"];
                explore.TextField(Find.ById("free-firstname")).Value = email;
                string lastName = ConfigurationManager.AppSettings["lastName"];
                explore.TextField(Find.ById("free-lastname")).Value = email;


                //LogHelper.logger.Info("Creating 10minutemail new email id");

                //explore.GoTo(Helper._10_MINUTE_MAIL_URL);
                //explore.WaitForComplete();
                //string emailID = explore.TextField(Find.ById(Helper._10_MINUTE_EMAIL_TEXTBOX_ID)).Value;

                //LogHelper.logger.Info("emailID is : " + emailID);
                //LogHelper.logger.Info("Creating Uploaded.net new account with 10minute email id");

                //CreateUploadLogin(emailID);

                //LogHelper.logger.Info("Account created");
                //LogHelper.logger.Info("Reading welcome email from uploaded.net");

                //bool isConfirmationMailArrived = false;

                //string confirmationUrl = string.Empty;

                //while (!isConfirmationMailArrived)
                //{
                //    Thread.Sleep(20000);
                //    explore.GoTo(Helper.CONFIRMATION_EMAIL_URL);
                //    explore.WaitForComplete();

                //    LogHelper.logger.Info("Extracting confirmation url");

                //    confirmationUrl = GetConfirmationMailId(explore.Html);
                //    if (!string.IsNullOrEmpty(confirmationUrl))
                //    {
                //        isConfirmationMailArrived = true;
                //    }
                //}
                //LogHelper.logger.Info("Confirmation url: ");
                //LogHelper.logger.Info("Hitting Confirmation url ");

                //IE uploadedExplorer = new IE(confirmationUrl);

                //LogHelper.logger.Info("Reading username password email ");

                //Thread.Sleep(20000);
                //explore.GoTo(Helper.PASSWORD_EMAIL_URL);
                //explore.WaitForComplete();

                //string newAccountId;
                //string newPassword;

                //LogHelper.logger.Info("Extracting username password email ");

                //GetAccountIdAndPassword(explore, out newAccountId, out newPassword);

                //LogHelper.logger.Info("username: " + newAccountId + "password: ");

                //var period = PremiumPeriod._48Hours;
                //if (rbtn48Hours.IsChecked == true)
                //{
                //    period = PremiumPeriod._48Hours;
                //}
                //else if (rbtn1Month.IsChecked == true)
                //{
                //    period = PremiumPeriod._1Month;
                //}
                //else if (rbtn3Months.IsChecked == true)
                //{
                //    period = PremiumPeriod._3Months;
                //}
                //else if (rbtn6Months.IsChecked == true)
                //{
                //    period = PremiumPeriod._6Months;
                //}
                //else if (rbtn1Year.IsChecked == true)
                //{
                //    period = PremiumPeriod._1Year;
                //}

                //var paysafePin1 = txtpaysafe_pin1.Text;
                //var paysafePin2 = txtpaysafe_pin2.Text;
                //var paysafePin3 = txtpaysafe_pin3.Text;
                //var paysafePin4 = txtpaysafe_pin4.Text;

                //bool finalStautus = RegisterForPremiumAccountWithReference(uploadedExplorer, period, emailID, paysafePin1, paysafePin2, paysafePin3, paysafePin4);
                ////bool status = RegisterForPremiumAccount(newAccountId, newPassword, period, emailID, paysafe_pin1, paysafe_pin2, paysafe_pin3, paysafe_pin4);

                ////explore.Close();
                //MessageBox.Show(Helper.REGISTRATION_SUCCESSFUL_LOG);
                //this.Close();
                //RegistrationSuccessful = true;
                //AccountId = newAccountId;
                //Password = newPassword;
            }
            catch (Exception ex)
            {
                LogHelper.logger.Error(ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Mouse.OverrideCursor = null;
                frmWait.Close();
            }
        }
        private void RegisterOldWay()
        {
            Wait frmWait = new Wait();
            try
            {
                MessageBox.Show(Helper.PLEASE_WAIT_LOG);
                Mouse.OverrideCursor = Cursors.Wait;

                //Settings.Instance.MakeNewIeInstanceVisible = false;

                IE explore = new IE();
                explore.ClearCache();
                explore.ClearCookies();
                explore.WaitForComplete();
                explore.ShowWindow(NativeMethods.WindowShowStyle.Maximize);



                LogHelper.logger.Info("Creating 10minutemail new email id");

                explore.GoTo(Helper._10_MINUTE_MAIL_URL);
                explore.WaitForComplete();
                string emailID = explore.TextField(Find.ById(Helper._10_MINUTE_EMAIL_TEXTBOX_ID)).Value;

                LogHelper.logger.Info("emailID is : " + emailID);
                LogHelper.logger.Info("Creating Uploaded.net new account with 10minute email id");

                CreateUploadLogin(emailID);

                LogHelper.logger.Info("Account created");
                LogHelper.logger.Info("Reading welcome email from uploaded.net");

                bool isConfirmationMailArrived = false;

                string confirmationUrl = string.Empty;

                while (!isConfirmationMailArrived)
                {
                    Thread.Sleep(20000);
                    explore.GoTo(Helper.CONFIRMATION_EMAIL_URL);
                    explore.WaitForComplete();

                    LogHelper.logger.Info("Extracting confirmation url");

                    confirmationUrl = GetConfirmationMailId(explore.Html);
                    if (!string.IsNullOrEmpty(confirmationUrl))
                    {
                        isConfirmationMailArrived = true;
                    }
                }
                LogHelper.logger.Info("Confirmation url: ");
                LogHelper.logger.Info("Hitting Confirmation url ");

                IE uploadedExplorer = new IE(confirmationUrl);

                LogHelper.logger.Info("Reading username password email ");

                Thread.Sleep(20000);
                explore.GoTo(Helper.PASSWORD_EMAIL_URL);
                explore.WaitForComplete();

                string newAccountId;
                string newPassword;

                LogHelper.logger.Info("Extracting username password email ");

                GetAccountIdAndPassword(explore, out newAccountId, out newPassword);

                LogHelper.logger.Info("username: " + newAccountId + "password: ");

                var period = PremiumPeriod._48Hours;
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

                var paysafePin1 = txtpaysafe_pin1.Text;
                var paysafePin2 = txtpaysafe_pin2.Text;
                var paysafePin3 = txtpaysafe_pin3.Text;
                var paysafePin4 = txtpaysafe_pin4.Text;

                bool finalStautus = RegisterForPremiumAccountWithReference(uploadedExplorer, period, emailID, paysafePin1, paysafePin2, paysafePin3, paysafePin4);
                //bool status = RegisterForPremiumAccount(newAccountId, newPassword, period, emailID, paysafe_pin1, paysafe_pin2, paysafe_pin3, paysafe_pin4);

                //explore.Close();
                MessageBox.Show(Helper.REGISTRATION_SUCCESSFUL_LOG);
                this.Close();
                RegistrationSuccessful = true;
                AccountId = newAccountId;
                Password = newPassword;
            }
            catch (Exception ex)
            {
                LogHelper.logger.Error(ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
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
                LogHelper.logger.Info("Redirect to Register page ");

                uploadedExplorer.GoTo("http://Ul.to/ref/6335349");
                uploadedExplorer.WaitForComplete();

                bool isValidPeriod = false;
                switch (period)
                {
                    case PremiumPeriod._48Hours:
                        LogHelper.logger.Info("Selecting premium registration duration 2 days ");
                        uploadedExplorer.GoTo(Helper._2_DAYS_PREMIUM_DURATION_URL);
                        isValidPeriod = true;
                        break;
                    case PremiumPeriod._1Month:
                        LogHelper.logger.Info("Selecting premium registration duration 1 month ");
                        uploadedExplorer.GoTo(Helper._1_MONTH_PREMIUM_DURATION_URL);
                        isValidPeriod = true;
                        break;
                    case PremiumPeriod._3Months:
                        LogHelper.logger.Info("Selecting premium registration duration 3 month ");
                        uploadedExplorer.GoTo(Helper._3_MONTHS_PREMIUM_DURATION_URL);
                        isValidPeriod = true;
                        break;
                    case PremiumPeriod._6Months:
                        LogHelper.logger.Info("Selecting premium registration duration 6 month ");
                        uploadedExplorer.GoTo(Helper._6_MONTHS_PREMIUM_DURATION_URL);
                        isValidPeriod = true;
                        break;
                    case PremiumPeriod._1Year:
                        LogHelper.logger.Info("Selecting premium registration duration 1 year");
                        uploadedExplorer.GoTo(Helper._1_YEAR_PREMIUM_DURATION_URL);
                        isValidPeriod = true;
                        break;
                }

                if (isValidPeriod)
                {
                    Thread.Sleep(3000);
                    uploadedExplorer.WaitForComplete();

                    LogHelper.logger.Info("Check terms and condition check box ");
                    var frames = uploadedExplorer.Frames;
                    if (frames != null && frames.Count>0 && frames[0].CheckBoxes.Count>0)
                    {
                        frames[0].CheckBoxes[0].Checked = true;
                    }
                    if (frames != null && frames.Count > 0)
                    {
                        var pinTextField = frames[0].TextField(Find.ById("pinForm:rn01"));
                        if (pinTextField != null)
                        {
                            LogHelper.logger.Info("Entering paysafe pin 1: " + paysafe_pin1);
                            frames[0].TextField(Find.ById("pinForm:rn01")).Value = paysafe_pin1;
                            uploadedExplorer.WaitForComplete();
                        }
                        pinTextField = frames[0].TextField(Find.ById("pinForm:rn02"));
                        if (pinTextField != null)
                        {
                            LogHelper.logger.Info("Entering paysafe pin 2: " + paysafe_pin2);
                            frames[0].TextField(Find.ById("pinForm:rn02")).Value = paysafe_pin2;
                            uploadedExplorer.WaitForComplete();
                        }
                        pinTextField = frames[0].TextField(Find.ById("pinForm:rn03"));
                        if (pinTextField != null)
                        {
                            LogHelper.logger.Info("Entering paysafe pin 3: " + paysafe_pin3);
                            frames[0].TextField(Find.ById("pinForm:rn03")).Value = paysafe_pin3;
                            uploadedExplorer.WaitForComplete();
                        }
                        pinTextField = frames[0].TextField(Find.ById("pinForm:rn04"));
                        if (pinTextField != null)
                        {
                            LogHelper.logger.Info("Entering paysafe pin 4: " + paysafe_pin4);
                            frames[0].TextField(Find.ById("pinForm:rn04")).Value = paysafe_pin4;
                            uploadedExplorer.WaitForComplete();
                        }
                    }
                    LogHelper.logger.Info("Submitting paysafe pin code for registration ");

                    var submitButton = frames[0].Button(Find.ByName("pinForm:pay"));
                    if (submitButton != null)
                    {
                        submitButton.Click();
                        uploadedExplorer.WaitForComplete();
                    }
                    //uploadedExplorer.Button(Find.ById("pinForm:pay")).ClickNoWait();
                    LogHelper.logger.Info("Submitting finished ");

                }
                Thread.Sleep(10000);
                LogHelper.logger.Info("Logging out of uploaded.net");
                uploadedExplorer.GoTo("http://uploaded.net/logout");
                uploadedExplorer.WaitForComplete();
                LogHelper.logger.Info("Log out successful.");

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
                string url = "https://uploaded.net/io/register/free";
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.Referer = "https://uploaded.net/register";
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                request.Method = "POST";
                request.ServicePoint.Expect100Continue = false;

                string postData = "mail=" + emailID + "&firstname=abc&lastname=xyz";
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
