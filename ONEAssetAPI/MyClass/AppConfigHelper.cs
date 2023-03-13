using System.Security.Cryptography;
using System.Text;
namespace ONEAssetAPI.MyClass
{
    public class AppConfigHelper
    {
        public string ServerName;
        public string DatabaseName;
        public string UserName;
        public string Password;
        public string IMGAssetUpload;
        public string AttachementUpload;
        public string ActsSecretKey;
        public string ApplicationSecret;
        public string ActsTokenPeriod;
        public string ActsTokenValue;

       //public string EncryptionKey = "ONEEDEV";


        public void ReadAppconfig()
        {

            ServerName = System.Configuration.ConfigurationManager.AppSettings.Get("ServerName");
            DatabaseName = System.Configuration.ConfigurationManager.AppSettings.Get("DatabaseName");
            UserName = System.Configuration.ConfigurationManager.AppSettings.Get("UserName");
            Password = System.Configuration.ConfigurationManager.AppSettings.Get("Password");
            IMGAssetUpload = System.Configuration.ConfigurationManager.AppSettings.Get("IMGAssetUpload");
            AttachementUpload = System.Configuration.ConfigurationManager.AppSettings.Get("AttachementUpload");
            ActsSecretKey = System.Configuration.ConfigurationManager.AppSettings.Get("ActsSecretKey");
            ApplicationSecret = System.Configuration.ConfigurationManager.AppSettings.Get("ApplicationSecret");
            ActsTokenPeriod = System.Configuration.ConfigurationManager.AppSettings.Get("ActsTokenPeriod");
            ActsTokenValue = System.Configuration.ConfigurationManager.AppSettings.Get("ActsTokenValue");
        }

        public bool AddUpdateAppSettings(string key, string value)
        {

            try
            {
                var configFile = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(System.Configuration.ConfigurationSaveMode.Modified);
                System.Configuration.ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
                return true;
            }
            catch (System.Configuration.ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings");
                return false;
            }
        }

        public string passwordEncrypt(string inText, string key)
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

        public string passwordDecrypt(string cryptTxt, string key)
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
