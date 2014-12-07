using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace DownloadManagerService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IDownloadDataProviderService
    {
        [OperationContract]
        string GetCategories();

        [OperationContract]
        string GetInformation();

        [OperationContract]
        string GetDownloadInformation();

        [OperationContract]
        string GetFileList();

        [OperationContract]
        bool IsValidLogin(string _userName, string _password);

        [OperationContract]
        bool RegisterNewUser(string _userName, string _password, string _email);
    }
  
}
