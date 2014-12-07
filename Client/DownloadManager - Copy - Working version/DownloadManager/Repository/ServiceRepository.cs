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
            return DataProviderService.IsValidLogin(_userName, _password);
        }

        public bool RegisterNewUser(string _userName, string _password, string _email)
        {
            return DataProviderService.RegisterNewUser(_userName, _password, _email);
        }
    }
}
