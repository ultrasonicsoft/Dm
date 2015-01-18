using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ultrasonic.DownloadManager.DownloadManagerService;

namespace Ultrasonic.DownloadManager.Repository
{
    public class ServiceRepository
    {
        private static DownloadDataProviderServiceClient _dataProviderService;
        private static object locker = new object();
        public static DownloadDataProviderServiceClient DataProviderService
        {
            get
            {
                if (_dataProviderService == null)
                {
                    lock (locker)
                    {
                        if (_dataProviderService == null)
                        {
                            _dataProviderService = new DownloadDataProviderServiceClient();
                        }
                    }
                }
                return _dataProviderService;
            }
        }

        public bool IsValidLogin(string _userName, string _password)
        {
            try
            {
                return DataProviderService.IsValidLogin(_userName, _password);
            }
            catch (Exception exception)
            {
                LogHelper.logger.Error(exception);
            }
            return false;
        }

        public bool RegisterNewUser(string _userName, string _password, string _email)
        {
            try
            {
                return DataProviderService.RegisterNewUser(_userName, _password, _email);
            }
            catch (Exception exception)
            {
                LogHelper.logger.Error(exception);
            }
            return false;
        }

        public string GetLatestVersion()
        {
            try
            {
                return DataProviderService.GetLatestVersion();
            }
            catch (Exception exception)
            {
                LogHelper.logger.Error(exception);
            }
            return string.Empty;
        }

        public string GetDownloadUrl()
        {
            try
            {
                return DataProviderService.GetDownloadUrl();
            }
            catch (Exception exception)
            {
                LogHelper.logger.Error(exception);
            }
            return string.Empty;
        }

        public string GetFtpUserName()
        {
            try
            {
                return DataProviderService.GetFtpUserName();
            }
            catch (Exception exception)
            {
                LogHelper.logger.Error(exception);
            }
            return string.Empty;
        }

        public string GetFtpPassword()
        {
            try
            {
                return DataProviderService.GetFtpPassword();

            }
            catch (Exception exception)
            {
                LogHelper.logger.Error(exception);
            }
            return string.Empty;
        }
    }
}
