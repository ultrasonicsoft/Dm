using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;

namespace DownloadManagerService
{
    internal class BusinessLogic
    {
        public static bool IsRegisteredUser(string email)
        {
            bool isRegisteredUser = false;
            try
            {
                MySqlParameter emailParameter = new MySqlParameter
                {
                    DbType = DbType.AnsiString,
                    ParameterName = "?_email",
                    Value = email,
                    Direction = ParameterDirection.Input
                };
                List<MySqlParameter> parameterCollection = new List<MySqlParameter>() { emailParameter };
                var result = DatabaseHelper.ExecuteStoredProcedure(Constants.IsRegisteredUser, parameterCollection);
                if (result.Tables[0].Rows.Count > 0)
                {
                    int value = int.Parse(result.Tables[0].Rows[0][0].ToString());
                    isRegisteredUser = value > 0;
                }
            }
            catch (Exception exception)
            {
                throw;
            }
            return isRegisteredUser;
        }

    }
}