using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;


namespace webAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ValidateAuthenController : Controller
    {
        // GET: ValidateAuthenController
        [Route("getValidAuthen")]
        [HttpGet]
        public JsonResult getValidAuthen(string strUsername, string strPassword , string strLoginType)
        {
            if(strLoginType=="Local")
            {
                string query = @"select * from MT_USER where IS_DEL is null and U_NAME = '" + strUsername + "' and U_PASS = '" + strPassword + "'";
                DataTable table = new DataTable();
                string sqlDataSource = "Server=10.10.0.23;Database=WebPortal;User Id=sa;Password=Mv5148cBX@;";
                SqlDataReader myReader;
                using (SqlConnection myCon = new SqlConnection(sqlDataSource))
                {
                    myCon.Open();
                    using (SqlCommand myCommand = new SqlCommand(query, myCon))
                    {
                        myReader = myCommand.ExecuteReader();
                        table.Load(myReader); ;

                        myReader.Close();
                        myCon.Close();
                    }
                }

                if (table.Rows.Count > 0)
                    return new JsonResult("1");
                else
                    return new JsonResult("0");
            }
            else
            {
                if(strLoginType== "" && strLoginType != "GMMChannelHolding" && strLoginType != "ONE31")
                    return new JsonResult("0");
                else
                {
                    string strLDAPServer = "";
                    if (strLoginType == "GMMChannelHolding")
                        strLDAPServer = "10.5.0.11/DC=gmmchannelholding,DC=com";
                    else if (strLoginType == "ONE31")
                        strLDAPServer = "10.31.1.2/DC=one31,DC=net";

                    System.DirectoryServices.DirectoryEntry Entry = new System.DirectoryServices.DirectoryEntry("LDAP://"+ strLDAPServer, strUsername, strPassword);
                    System.DirectoryServices.DirectorySearcher Searcher = new System.DirectoryServices.DirectorySearcher(Entry);
                    
                    try
                    {
                        System.DirectoryServices.SearchResult Results = Searcher.FindOne();
                        return new JsonResult("1");
                    }
                    catch
                    {
                        return new JsonResult("0");
                    }

                }


            }
                


        }

        //[HttpGet]
        //public string Get(string strIn)
        //{
        //    return "Result with parameter " + strIn;
        //}

        [HttpPost]
        public bool Login(string username, string password)
        {
            if (username != null && password != null && username.Equals("acc1") && password.Equals("123"))
            {
                HttpContext.Session.SetString("username", username);
                return true;
            }
            else
            {
                ViewBag.error = "Invalid Account";
                return false;
            }
        }

        //[Route("getDataX")]
        //[HttpGet]
        public JsonResult GetDataX(string strUsername)
        {
            string query = @"select * from [dbo].[MT_USER]";
            DataTable table = new DataTable();
            string sqlDataSource = "Server=10.10.0.23;Database=WebPortal;User Id=sa;Password=Mv5148cBX@;";
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader); ;

                    myReader.Close();
                    myCon.Close();
                }
            }

            return new JsonResult(table);

        }

        [Route("getEncryptText")]
        [HttpGet]
        public IActionResult GetEncryptText(string strText)
        {
            webAPI.MyClass.AppConfigHelper appconfig = new webAPI.MyClass.AppConfigHelper();
            appconfig.ReadAppconfig();
            string strReturn = webAPI.MyClass.AppConfigHelper.passwordEncrypt(strText, appconfig.ApplicationSecret);
            return Ok(strReturn);
        }

        [Route("getDecryptText")]
        [HttpGet]
        public IActionResult GetDecryptText(string strText)
        {
            webAPI.MyClass.AppConfigHelper appconfig = new webAPI.MyClass.AppConfigHelper();
            appconfig.ReadAppconfig();
            string strReturn = webAPI.MyClass.AppConfigHelper.passwordDecrypt(strText, appconfig.ApplicationSecret);
            return Ok(strReturn);
        }


    }
}
