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
    public interface IDownloadService
    {
        [OperationContract]
        void CreateNewUser(User newUser);

        [OperationContract]
        bool IsRegisteredUser(string email);

        [OperationContract]
        string GetInformationXml();

        [OperationContract]
        string GetFinishXml();

        [OperationContract]
        string GetCategoriesXml();
    }

    [DataContract]
    public class User
    {
        private string userName;
        private string password;
        private string email;
        private int id;
        private bool isRegistered;
        private int registerTypeID;
        private DateTime registerstrationDate;

        [DataMember]
        public string UserName
        {
            get { return userName; }
            set { userName = value; }
        }

        [DataMember]
        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        [DataMember]
        public string Email
        {
            get { return email; }
            set { email = value; }
        }

        [DataMember]
        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        [DataMember]
        public bool IsRegistered
        {
            get { return isRegistered; }
            set { isRegistered = value; }
        }

        [DataMember]
        public int RegisterTypeID
        {
            get { return registerTypeID; }
            set { registerTypeID = value; }
        }


        [DataMember]
        public DateTime RegisterstrationDate
        {
            get { return registerstrationDate; }
            set { registerstrationDate = value; }
        }
        


    }
    
}
