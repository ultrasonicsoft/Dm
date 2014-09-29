using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace DownloadManagerService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class DownloadService : IDownloadService
    {
        public void CreateNewUser(User newUser)
        {
            throw new NotImplementedException();
        }

        public bool IsRegisteredUser(string email)
        {
            return BusinessLogic.IsRegisteredUser(email);
        }

        public string GetInformationXml()
        {
            string path = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath;
            string fileName = path + "\\bin\\" + Constants.InformationXml;
            return File.ReadAllText(fileName);
        }

        public string GetFinishXml()
        {
            string path = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath;
            string fileName = path + "\\bin\\" + Constants.FinishXml;
            return File.ReadAllText(fileName);
        }

        public string GetCategoriesXml()
        {
            string path = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath;
            string fileName = path + "\\bin\\" + Constants.CategoriesXml;
            return File.ReadAllText(fileName);
        }
    }
}
