using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ultrasonic.DownloadManager
{
    internal static class Helper
    {
        // Private Server information
        internal const long BUFFER_SIZE = 4096;
        internal const string SERVER_URL = "http://62.219.225.208/";
        //internal const string FILELIST_XML_PATH = @"http://62.219.225.208/uploaded/filelist.xml";
        //internal const string FINISH_XML_PATH = @"http://62.219.225.208/uploaded/finish.xml";
        //internal const string CATEGORIES_XML_PATH = @"http://62.219.225.208/uploaded/categories.xml";
        //internal const string INFORMATION_XML_PATH = @"http://62.219.225.208/uploaded/informations.xml";
        internal const string FTP_USER_NODE_TEXT = "ftpuser";
        internal const string FTP_PASSWORD_NODE_TEXT = "ftppass";
        internal const char DOWNLOAD_URI_SEPARATOR_CHAR = '^';
        internal const string DEFAULT_PASSWORD = "uploaded.net";
        internal const string UPLOADED_ID_VALUE = "7549995";
        internal const string UPLOADED_PASSWORD_VALUE = "admin";
        internal const string UPLOADED_ID_TEXT = "id";
        internal const string UPLOADED_PASSWORD_TEXT = "pw";

        // uploaded.net information
        internal const string UPLOADED_BASE_ADDRESS = @"http://uploaded.net/io/login";
        internal const string FILENAME_TEXT = "filename";
        internal const string POST_BACK_METHOD = "POST";
        internal const string CONFIRMATION_EMAIL_URL = "http://10minutemail.com/10MinuteMail/index.html?dataModelSelection=message%3Aemails%5B0%5D&actionMethod=index.xhtml%3AmailQueue.select";
        internal const string PASSWORD_EMAIL_URL = "http://10minutemail.com/10MinuteMail/index.html?dataModelSelection=message%3Aemails%5B1%5D&actionMethod=index.xhtml%3AmailQueue.select";
        internal const string _10_MINUTE_MAIL_URL = "http://10minutemail.com/10MinuteMail/index.html";
        internal const string _10_MINUTE_EMAIL_TEXTBOX_ID = "addyForm:addressSelect";
        
        // registration urls
        internal const string _2_DAYS_PREMIUM_DURATION_URL = "http://uploaded.net/register/pay/PRM-D2/paysafe";
        internal const string _1_MONTH_PREMIUM_DURATION_URL = "http://uploaded.net/register/pay/PRM-M1/paysafe";
        internal const string _3_MONTHS_PREMIUM_DURATION_URL = "http://uploaded.net/register/pay/PRM-M3/paysafe";
        internal const string _6_MONTHS_PREMIUM_DURATION_URL = "http://uploaded.net/register/pay/PRM-M6/paysafe";
        internal const string _1_YEAR_PREMIUM_DURATION_URL = "http://uploaded.net/register/pay/PRM-Y1/paysafe";

        // 7 zip installation path
        internal const string _7ZIP_FILE_PATH = @"C:\Program Files\7-Zip\7z.exe";
        internal const string OUTPUT_ZIP_FOLDER = @"f:\zipOutput";
        
        // logging path
        internal const string LOGGING_FILE_PATH = @"C:\Users\BChavan\Desktop\cdm.log";

        //logging messages
        internal const string PLEASE_WAIT_LOG = "Please wait... This will take few minutes...";
        internal const string CREATING_10MINUTE_ID_LOG = "Creating  10minutemail new email id";
        internal const string EMAIL_ID_IS_LOG = "emailID is : ";
        internal const string CREATING_UPLOADED_ACCOUNT_LOG = "Creating Uploaded.net new account with 10minute email id";
        internal const string ACCOUNT_CREATED_LOG = "Account created";
        internal const string READING_WELCOME_MAIL_LOG = "Reading welcome email from uploaded.net";
        internal const string EXTRACTING_CONFIRMATION_URL_LOG = "Extracting confirmation url";
        internal const string CONFIRMATION_URL_LOG = "Confirmation url: ";
        internal const string HITTING_CONFIRMATION_URL_LOG = "Hitting Confirmation url ";
        internal const string READING_USERNAME_PASSWORD_LOG = "Reading username password email ";
        internal const string EXTRACTING_USERNAME_PASSWORD_LOG = "Extracting username password email ";
        internal const string USERNAME_LOG = "username: ";
        internal const string PASSWORD_LOG = "password: ";
        internal const string REGISTRATION_SUCCESSFUL_LOG = "Registration successful";

    }
}
