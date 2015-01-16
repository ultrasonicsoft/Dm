using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Web.Hosting;

namespace DownloadManagerService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "DownloadDataProviderService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class DownloadDataProviderService : IDownloadDataProviderService
    {
        DBRepository _dbRepository;

        public DownloadDataProviderService()
        {
            if(_dbRepository ==null)
            _dbRepository = new DBRepository();
        }
        public string GetCategories()
        {
            string fileName = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "App_Data", "categories.xml");
            return File.ReadAllText(fileName);
        }

        public string GetInformation()
        {
            string fileName = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "App_Data", "informations.xml");
            return File.ReadAllText(fileName);
        }

        public string GetDownloadInformation()
        {
            string fileName = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "App_Data", "finish.xml");
            return File.ReadAllText(fileName);
        }

        public string GetFileList()
        {
            string fileName = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "App_Data", "filelist.xml");
            return File.ReadAllText(fileName);
        }

        public bool IsValidLogin(string _userName, string _password)
        {
            return _dbRepository.isValidLogin(_userName, _password);
        }

        public bool RegisterNewUser(string _userName, string _password, string _email)
        {
            return _dbRepository.RegisterNewUser(_userName, _password, _email);
        }


        public string GetLatestVersion()
        {
            string fileName = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "App_Data", "LatestVersion.txt");
            if (File.Exists(fileName))
                return File.ReadAllText(fileName);
            return string.Empty;
        }

        public string GetDownloadUrl()
        {
            string fileName = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "App_Data", "DownloadUrl.txt");
            if (File.Exists(fileName))
                return File.ReadAllText(fileName);
            return string.Empty;
        }


        public string GetFtpUserName()
        {
            string fileName = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "App_Data", "FtpUserName.txt");
            if (File.Exists(fileName))
                return File.ReadAllText(fileName);
            return string.Empty;
        }

        public string GetFtpPassword()
        {
            string fileName = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "App_Data", "FtpPassword.txt");
            if (File.Exists(fileName))
                return File.ReadAllText(fileName);
            return string.Empty;
        }
    }
}
