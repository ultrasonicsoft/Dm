using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;

namespace DownloadManagerService
{
    public class DBRepository
    {
        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        public DBRepository()
        {
            Initialize();
        }
        private void Initialize()
        {
            //server = "localhost";
            //database = "downloadmanagerdb";
            //uid = "root";
            //password = "root";
            string connectionString = ConfigurationManager.ConnectionStrings["dbConStr"].ConnectionString;
            //connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            //database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            connection = new MySqlConnection(connectionString);
        }
        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
               return false;
            }
        }

        //Close connection
        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                return false;
            }
        }

        private int ExecuteProcedure(string procedure, List<MySqlParameter> parameters)
        {
            int affectedRow=0;
            try
            {
                if (this.OpenConnection() == true)
                {
                    //Create Command
                    MySqlCommand cmd = new MySqlCommand(procedure, connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddRange(parameters.ToArray());
                    affectedRow = cmd.ExecuteNonQuery();
                    //close Connection
                    this.CloseConnection();

                }   
            }
            catch (Exception exception)
            {
                
            }
            return affectedRow;
        }

        public bool isValidLogin(string _userName, string _password)
        {
            bool result = false;
            try
            {
                string query = string.Format("SELECT * FROM users where UserName='{0}' OR Email='{0}'",_userName);

                //Create a list to store the result
                string userName = string.Empty;
                string password = string.Empty;
                string email = string.Empty;

                //Open connection
                if (this.OpenConnection() == true)
                {
                    //Create Command
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    //Create a data reader and Execute the command
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    //Read the data and store them in the list
                    while (dataReader.Read())
                    {
                        userName = dataReader["UserName"].ToString();
                        email = dataReader["Email"].ToString();
                        password = dataReader["Password"].ToString();
                    }

                    //close Data Reader
                    dataReader.Close();

                    //close Connection
                    this.CloseConnection();

                    result = (string.Equals(userName, _userName)  || string.Equals(email, _userName)) && 
                                    string.Equals(password, _password);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception exception)
            {
                
            }
            return result;
        }

        public bool RegisterNewUser(string _userName, string _password, string _email)
        {
            List<MySqlParameter> parameters = new List<MySqlParameter>
            {
                new MySqlParameter("_userName", _userName),
                new MySqlParameter("_password", _password),
                new MySqlParameter("_email", _email)
            };
            return ExecuteProcedure("RegisterNewUser", parameters) > 0;
        }
    }
}