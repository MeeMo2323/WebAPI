using System.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace ONEDaraAPI.MyClass
{
    class DbHelper : IDisposable
    {
        private string dbServerName;
        public string dbDatabaseName;
        public string dbOwner;
        private string dbUserName;
        private string dbPassword;
        private SqlConnection connOpen;
        //public string ProcessingFolder;
        public string BackupFolder;

        public int dbAction;
        public string mode;
        public string dbconnectionStatus;

        public DbHelper()
        {
            AppConfigHelper appconfig = new AppConfigHelper();
            appconfig.ReadAppconfig();

            dbServerName = appconfig.ServerName;
            dbDatabaseName = appconfig.DatabaseName;
            dbUserName = appconfig.UserName;
            dbPassword = passwordDecrypt(appconfig.Password, appconfig.EncryptionKey);
            //ProcessingFolder = appconfig.ProcessingFolder;

        }

        public bool DBConnect()
        {
            string constr = "Server=" + dbServerName + ";Database=" + dbDatabaseName + ";User Id=" + dbUserName + ";Password=" + dbPassword + ";";
            try
            {
                connOpen = new SqlConnection(constr);
                connOpen.Open();
                dbconnectionStatus = "Connected";
                return true;

            }
            catch (Exception ex)
            {

                connOpen = null;
                dbconnectionStatus = ex.Message;
                return false;
            }

        }

        public bool DBClose()
        {
            try
            {
                connOpen.Close();
                connOpen = null;
                dbconnectionStatus = "Disconnected";
                return true;

            }
            catch (Exception ex)
            {

                connOpen = null;
                dbconnectionStatus = ex.Message;
                return false;
            }

        }

        public bool DBExecute(string sqlStr)
        {
            try
            {
                DBConnect();
                SqlCommand command = new SqlCommand(sqlStr, connOpen);
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
                DBClose();
                return true;
            }
            catch (Exception)
            {
                DBClose();
                return true;
            }
        }

        public string DBExecuteResult(string sqlStr)
        {
            string strReturn = "";
            try
            {
                DBConnect();
                SqlCommand command = new SqlCommand(sqlStr, connOpen);
                command.CommandType = CommandType.Text;
                strReturn = System.Convert.ToString(command.ExecuteScalar());
                DBClose();
                return strReturn;
            }
            catch (Exception)
            {
                DBClose();
                return strReturn;
            }
        }


        public DataSet DBGetData(string strSQL)
        {
            DBConnect();
            SqlDataAdapter adapter = new SqlDataAdapter(strSQL, connOpen);
            DataSet ds = new DataSet();
            adapter.Fill(ds, "Result");
            DBClose();
            return ds;
        }

        public DataSet DBGetData(string tableName, string fileName, string orderFieldName)
        {
            SqlDataAdapter adapter = new SqlDataAdapter("SELECT " + fileName + " FROM " + dbDatabaseName + "." + dbOwner + "." + tableName + " Order by " + orderFieldName, connOpen);
            DataSet ds = new DataSet();
            adapter.Fill(ds, "Result");
            return ds;
        }

        public DataSet DBGetData(string tableName, string keyFieldName, string value, string orderFieldName)
        {
            SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM " + dbDatabaseName + "." + dbOwner + "." + tableName + " WHERE " + keyFieldName + " = '" + value + "'", connOpen);
            DataSet ds = new DataSet();
            adapter.Fill(ds, "Result");
            return ds;
        }

        public DataSet DBGetData(string tableName, string fileName, string keyFieldName, string value, string orderFieldName)
        {
            SqlDataAdapter adapter = new SqlDataAdapter("SELECT " + fileName + " FROM " + dbDatabaseName + "." + dbOwner + "." + tableName + " WHERE " + keyFieldName + " = '" + value + "'", connOpen);
            DataSet ds = new DataSet();
            adapter.Fill(ds, "Result");
            return ds;
        }

        public bool DBLogData(string UserName, string Section, string Description)
        {
            try
            {
                SqlCommand command = new SqlCommand("INSERT INTO " + dbDatabaseName + "." + dbOwner + ".DMSconfigUtilityLog VALUES('" + UserName + "',getdate(),'" + Section + "','" + Description + "')", connOpen);
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                return true;
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public static string passwordEncrypt(string inText, string key)
        {
            byte[] bytesBuff = Encoding.Unicode.GetBytes(inText);
            using (Aes aes = Aes.Create())
            {
                Rfc2898DeriveBytes crypto = new Rfc2898DeriveBytes(key, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                aes.Key = crypto.GetBytes(32);
                aes.IV = crypto.GetBytes(16);
                using (MemoryStream mStream = new MemoryStream())
                {
                    using (CryptoStream cStream = new CryptoStream(mStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cStream.Write(bytesBuff, 0, bytesBuff.Length);
                        cStream.Close();
                    }
                    inText = Convert.ToBase64String(mStream.ToArray());
                }
            }
            return inText;
        }

        public static string passwordDecrypt(string cryptTxt, string key)
        {
            cryptTxt = cryptTxt.Replace(" ", "+");
            byte[] bytesBuff = Convert.FromBase64String(cryptTxt);
            using (Aes aes = Aes.Create())
            {
                Rfc2898DeriveBytes crypto = new Rfc2898DeriveBytes(key, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                aes.Key = crypto.GetBytes(32);
                aes.IV = crypto.GetBytes(16);
                using (MemoryStream mStream = new MemoryStream())
                {
                    using (CryptoStream cStream = new CryptoStream(mStream, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cStream.Write(bytesBuff, 0, bytesBuff.Length);
                        cStream.Close();
                    }
                    cryptTxt = Encoding.Unicode.GetString(mStream.ToArray());
                }
            }
            return cryptTxt;
        }

    }
}
