using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;

namespace DownloadManagerService
{
    // Reset Password \bin\mysql\mysql5.5.24\bin\mysqladmin -u root password NEWPASSWORD
    public static class DatabaseHelper
    {
        private static MySqlConnection connection;

        static DatabaseHelper()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DownloadManagerDB"].ConnectionString;
            connection = new MySqlConnection(connectionString);
            LogHelper.logger.Error("Connection string : " + connectionString);
        }

        

        internal static DataSet ExecuteStoredProcedure(string procedureName, List<MySqlParameter> parametersCollection)
        {
            DataSet dsOutput = new DataSet();
            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.CommandText = procedureName;
                cmd.CommandType = CommandType.StoredProcedure;
                foreach (var parameter in parametersCollection)
                {
                    cmd.Parameters.Add(parameter);
                }
                cmd.Connection = connection;
                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                adapter.Fill(dsOutput);
            }
            catch (Exception exception)
            {
                throw;
            }
            finally
            {
                connection.Close();
            }

            return dsOutput;
        }
    }
}