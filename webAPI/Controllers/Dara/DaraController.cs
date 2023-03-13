using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json;
using System.IO;

using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

using Microsoft.AspNetCore.Authentication;


namespace webAPI.Controllers.Dara
{
    [ApiController]
    [Route("api/[controller]")]    
    public class DaraController : ControllerBase
    {
        private MyClass.DbHelper dbHelper = new webAPI.MyClass.DbHelper();
        private MyClass.authenHelper auth = new MyClass.authenHelper();

        [HttpGet("getAllDara")]
        public IActionResult GetAllDara(string U_ID)
        {
            //string query = @"select *,DATEDIFF(yy, B_DATE, GETDATE()) - CASE WHEN (MONTH(B_DATE) > MONTH(GETDATE())) OR (MONTH(B_DATE) = MONTH(GETDATE()) AND DAY(B_DATE) > DAY(GETDATE())) THEN 1 ELSE 0 END as AGE from [dbo].[MT_ACTORS]";
            string query = @"select	a.ACT_ID,isnull(F_NAME_TH,'') as F_NAME_TH,isnull(L_NAME_TH,'') as L_NAME_TH,isnull(N_NAME_TH,'') as N_NAME_TH,
		                            isnull(DISPLAY_NAME,'') as DISPLAY_NAME,isnull(FORMAT (B_DATE, 'dd.MM.yyyy'),'') as B_DATE,isnull(SEX,'') as SEX,
		                            isnull(BE_UNDER,'') as BE_UNDER, isnull(ACTING,'') as ACTING, isnull([IMAGE],'') as [IMAGE], 
		                            case when B_DATE is null or B_DATE ='1900-01-01' then '' else  DATEDIFF(yy, B_DATE, GETDATE()) - CASE WHEN (MONTH(B_DATE) > MONTH(GETDATE())) OR (MONTH(B_DATE) = MONTH(GETDATE()) AND DAY(B_DATE) > DAY(GETDATE())) THEN 1 ELSE 0 END END as AGE,
		                            case when b.ACT_ID is null then 'N' else 'Y' end as isFav
                            from	[dbo].[MT_ACTORS] a
                            left outer join [dbo].[TR_FAVORITE_ACTS] b on a.[ACT_ID] = b.[ACT_ID] and b.U_ID = " + U_ID + 
                            " where IS_DEL is null "+
                            " order by ACT_ID ";
            DataTable resTable = new DataTable();
            resTable = dbHelper.DBGetData(query).Tables[0];
            string strReturn = JsonConvert.SerializeObject(resTable, Newtonsoft.Json.Formatting.Indented);
            return Ok(strReturn);
        }

        [HttpGet("getBeUnder")]
        public IActionResult GetBeUnder()
        {
            string query = @"select BE_UNDER from [dbo].[MT_BE_UNDER] where IS_DEL is null order by be_under";
            DataTable resTable = new DataTable();
            resTable = dbHelper.DBGetData(query).Tables[0];
            string strReturn = JsonConvert.SerializeObject(resTable, Newtonsoft.Json.Formatting.Indented);
            return Ok(strReturn);
        }

        [HttpGet("getTag")]
        public IActionResult GetTag()
        {
            string query = @"select *,null as isSelected from [dbo].[MT_TAG] WHERE IS_DEL is null order by TAG_NAME";
            DataTable resTable = new DataTable();
            resTable = dbHelper.DBGetData(query).Tables[0];
            string strReturn = JsonConvert.SerializeObject(resTable, Newtonsoft.Json.Formatting.Indented);
            return Ok(strReturn);
        }


        [HttpPost("createDara")]
        public IActionResult CreateDara([FromForm] formData oData)
        {
            var jObj = Newtonsoft.Json.JsonConvert.DeserializeObject<Dara>(oData.Data);
            string result = "";
            MyClass.AppConfigHelper appconfig = new MyClass.AppConfigHelper();
            appconfig.ReadAppconfig();
            if (oData.IMGFile.Length>0)
            {
                string strSQL = "INSERT INTO MT_ACTORS(F_NAME_TH,L_NAME_TH,N_NAME_TH,DISPLAY_NAME,SEX,B_DATE,BE_UNDER,ACTING,CREATE_DATE,CREATE_BY) values('" + jObj.F_NAME_TH + "','"+ jObj.L_NAME_TH + "','"+ jObj.N_NAME_TH + "','" + jObj.DISPLAY_NAME + "','"+ jObj.SEX + "','" + jObj.B_DATE + "','" + jObj.BE_UNDER+"','"+ jObj.ACTING+ "',getdate(),(select U_NAME from MT_USER where U_ID = " + jObj.U_ID + ")) SELECT SCOPE_IDENTITY()";

                string strResult = dbHelper.DBExecuteResult(strSQL);
                
                if (strResult!="-1")
                {
                    dbHelper.DBExecute("UPDATE MT_ACTORS set IMAGE='" + strResult +"-"+ jObj.IMG_PATH + "' where ACT_ID ="+strResult);
                    
                    result = "1";
                    try
                    {
                        using (FileStream fileStream = System.IO.File.Create(appconfig.IMGDaraUpload + strResult + "-" + oData.IMGFile.FileName))
                        {
                            oData.IMGFile.CopyTo(fileStream);
                            fileStream.Flush();
                            //return oDara.files.FileName;
                        }
                        dbHelper.DBExecute("INSERT INTO TR_LOG(U_ID,ACT_ID,DESCRIPTION,CREATE_DATE) VALUES (" + jObj.U_ID + ","+ strResult + ",'U_ID : " + jObj.U_ID + ",ACT_ID : " + strResult + " Create new dara , F_NAME : " + jObj.F_NAME_TH + " L_NAME : " + jObj.L_NAME_TH + " N_NAME : " + jObj.N_NAME_TH + " DP_NAME : " + jObj.DISPLAY_NAME + " SEX : " + jObj.SEX + " B_DATE : " + jObj.B_DATE + " BE_UNDER : " + jObj.BE_UNDER + " ACTING : " + jObj.ACTING + " IMG : "+ strResult + "-" + oData.IMGFile.FileName + "',getdate())");
                    }
                    catch(Exception ex)
                    {
                        result = ex.Message;

                    }
                  
                }                   
                else
                    result = "0";
            }
            return Ok(result);
        }

        [HttpPost("updateDara")]
        public IActionResult UpdateDara([FromForm] formData oData)
        {
            var jObj = Newtonsoft.Json.JsonConvert.DeserializeObject<Dara>(oData.Data);
            string result = "";
            MyClass.AppConfigHelper appconfig = new MyClass.AppConfigHelper();
            appconfig.ReadAppconfig();

            try
            {
                string strSQL = "UPDATE MT_ACTORS set F_NAME_TH = '" + jObj.F_NAME_TH + "',L_NAME_TH ='" + jObj.L_NAME_TH + "'," +
                                                         "N_NAME_TH ='" + jObj.N_NAME_TH + "',DISPLAY_NAME = '" + jObj.DISPLAY_NAME + "',SEX = '" + jObj.SEX + "'," +
                                                         "B_DATE="+ (jObj.B_DATE==""?"null":"'"+ jObj.B_DATE + "'") + ", BE_UNDER='" + jObj.BE_UNDER + "',ACTING='" + jObj.ACTING + "',update_by =(select U_NAME from MT_USER where U_ID = " + jObj.U_ID + ") ,update_date = getdate() " +
                                "WHERE ACT_ID="+jObj.ACT_ID;
                dbHelper.DBExecute(strSQL);
                dbHelper.DBExecute("INSERT INTO TR_LOG(U_ID,ACT_ID,DESCRIPTION,CREATE_DATE) VALUES (" + jObj.U_ID + "," + jObj.ACT_ID + ",'U_ID : " + jObj.U_ID + ",ACT_ID : " + jObj.ACT_ID + " Update dara => F_NAME : " + jObj.F_NAME_TH + ", L_NAME : " + jObj.L_NAME_TH + ", N_NAME : " + jObj.N_NAME_TH + ", DP_NAME : " + jObj.DISPLAY_NAME + ", SEX : " + jObj.SEX + ", B_DATE : " + jObj.B_DATE + ", BE_UNDER : " + jObj.BE_UNDER + ", ACTING : " + jObj.ACTING + "',getdate())");
                result = "1";       
            }
            catch(Exception e)
            {
                result = "0";
            }

          
            if (oData.IMGFile != null && oData.IMGFile.Length > 0)
            {
                try
                {                    
                    dbHelper.DBExecuteResult("UPDATE MT_ACTORS set IMAGE='" + jObj.ACT_ID + "-" + jObj.IMG_PATH + "' where ACT_ID =" + jObj.ACT_ID);
                    result = "1";
                    //dbHelper.DBExecute("INSERT INTO MT_LOG(logX) values ('b-OK : " + appconfig.IMGDaraUpload + jObj.ACT_ID + "-" + oData.IMGFile.FileName + "')");
                    using (FileStream fileStream = System.IO.File.Create(appconfig.IMGDaraUpload + jObj.ACT_ID + "-" + oData.IMGFile.FileName))
                    {
                        //dbHelper.DBExecute("INSERT INTO MT_LOG(logX) values ('c-OK')");
                        oData.IMGFile.CopyTo(fileStream);
                        fileStream.Flush();
                        //return oDara.files.FileName;
                    }
                    dbHelper.DBExecute("INSERT INTO TR_LOG(U_ID,ACT_ID,DESCRIPTION,CREATE_DATE) VALUES (" + jObj.U_ID + "," + jObj.ACT_ID + ",'U_ID : " + jObj.U_ID + ",ACT_ID : " + jObj.ACT_ID + " Update dara => F_NAME : " + jObj.F_NAME_TH + ", L_NAME : " + jObj.L_NAME_TH + ", N_NAME : " + jObj.N_NAME_TH + ", DP_NAME : " + jObj.DISPLAY_NAME + ", SEX : " + jObj.SEX + ", BE_UNDER : " + jObj.BE_UNDER + ", ACTING : " + jObj.ACTING + ", IMG : " + jObj.ACT_ID + "-" + oData.IMGFile.FileName + "',getdate())");
                    result = "1";
                }
                catch (Exception ex)
                {
                    result = "0";
                }
            }
            return Ok(result);
        }

        [HttpPost("deleteDara")]
        public IActionResult DeleteDara([FromForm] formData oData)
        {
            var jObj = Newtonsoft.Json.JsonConvert.DeserializeObject<U_ID_ACT_ID>(oData.Data);
            string result = "";
            MyClass.AppConfigHelper appconfig = new MyClass.AppConfigHelper();
            appconfig.ReadAppconfig();

            try
            {
                string strSQL = " UPDATE MT_ACTORS set IS_DEL ='Y',DELETE_DATE=getdate(),DELETE_BY = (select U_NAME from MT_USER where U_ID = " + jObj.U_ID + ") WHERE ACT_ID=" + jObj.ACT_ID;
                dbHelper.DBExecute(strSQL);
                dbHelper.DBExecute("INSERT INTO TR_LOG(U_ID,ACT_ID,DESCRIPTION,CREATE_DATE) VALUES (" + jObj.U_ID + "," + jObj.ACT_ID + ",'U_ID : " + jObj.U_ID + ",ACT_ID : " + jObj.ACT_ID + " Delete dara',getdate())");
                result = "1";
            }
            catch (Exception e)
            {
                result = "0";
            }

            return Ok(result);
        }

        [HttpPost("updateFav")]
        public IActionResult UpdateFav([FromForm] formData oData)
        {
            var jObj = Newtonsoft.Json.JsonConvert.DeserializeObject<U_ID_ACT_ID>(oData.Data);
            string result = "";
            MyClass.AppConfigHelper appconfig = new MyClass.AppConfigHelper();
            appconfig.ReadAppconfig();

            try
            {
                string strSQL = " IF EXISTS (select * from TR_FAVORITE_ACTS where U_ID = "+ jObj.U_ID + " and ACT_ID=" + jObj.ACT_ID + ") " +
                               " BEGIN " +
                               "   delete from TR_FAVORITE_ACTS where U_ID = " + jObj.U_ID + " and ACT_ID = " + jObj.ACT_ID + 
                               " END " +
                               " ELSE " +
                               " BEGIN " +
                               "   insert into TR_FAVORITE_ACTS(U_ID, ACT_ID, CREATE_DATE, CREATE_BY) values(" + jObj.U_ID + ", " + jObj.ACT_ID + ", getDate(), (select U_NAME from MT_USER where U_ID = " + jObj.U_ID + ")) " +
                               " END";
                dbHelper.DBExecute(strSQL);
                result = "1";
            }
            catch (Exception e)
            {
                result = "0";
            }

            return Ok(result);
        }

        [HttpPost("logIn")]
        public IActionResult LogIn(User oUser)
        {
            var accessToken = HttpContext.GetTokenAsync("access_token");
            //var jObj = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(oUser.Data);
            string result = "";
            MyClass.AppConfigHelper appconfig = new MyClass.AppConfigHelper();
            appconfig.ReadAppconfig();

            string query = @"select U_ID,U_ROLE,U_NAME from [MT_USER] where U_NAME='"+ oUser.U_NAME + "' and U_PASS='"+ oUser.U_PASS + "' and IS_DEL is null";
            DataTable resTable = new DataTable();
            resTable = dbHelper.DBGetData(query).Tables[0];

            if(resTable.Rows.Count>0){
                dbHelper.DBExecute("UPDATE MT_USER set LAST_LOGIN = getdate() where U_ID="+ resTable.Rows[0]["U_ID"].ToString());
                result = auth.GenerateToken(resTable.Rows[0]["U_ID"].ToString(), resTable.Rows[0]["U_ROLE"].ToString(), resTable.Rows[0]["U_NAME"].ToString(), 1, "h");                
                string strReturn = JsonConvert.SerializeObject(new { U_ID = resTable.Rows[0]["U_ID"].ToString(), U_ROLE = resTable.Rows[0]["U_ROLE"].ToString(), U_NAME = resTable.Rows[0]["U_NAME"].ToString(), token = result }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }
            else
            {
                return Ok(JsonConvert.SerializeObject(new { U_ID = -1}, Newtonsoft.Json.Formatting.Indented));
            }
                

        }

        [HttpGet("getToken")]
        public IActionResult GetToken()
        {
            string strReturn = JsonConvert.SerializeObject(GenerateToken(""), Newtonsoft.Json.Formatting.Indented);
            return Ok(strReturn);
        }

        [HttpPost]
        public string GenerateToken(string strX)
        {
            // oUser.U_ID = "1";
            MyClass.AppConfigHelper appconfig = new MyClass.AppConfigHelper();
            appconfig.ReadAppconfig();
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(appconfig.DaraSecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", "1") }),
                //Expires = DateTime.UtcNow.AddDays(7),
                Expires = DateTime.UtcNow.AddSeconds(60),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [HttpGet("validateToken")]
        public int? ValidateToken(string token)
        {
            MyClass.AppConfigHelper appconfig = new MyClass.AppConfigHelper();
            appconfig.ReadAppconfig();
            if (token == null)
                return -1;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(appconfig.DaraSecretKey);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
                // return user id from JWT token if validation successful
                return userId;
            }
            catch
            {
                // return null if validation fails
                return -1;
            }
        }
    }

    public class Dara
    {
        public string U_ID { get; set; }
        public string ACT_ID { get; set; }
        public string F_NAME_TH { get; set; }
        public string L_NAME_TH { get; set; }
        public string N_NAME_TH { get; set; }
        public string DISPLAY_NAME { get; set; }
        public string SEX { get; set; }
        public string B_DATE { get; set; }
        public string BE_UNDER { get; set; }
        public string IMG_PATH { get; set; }
        public string ACTING { get; set; }
        public string isFav { get; set; }
    }

    public class User
    {
        public string U_NAME { get; set; }
        public string U_PASS { get; set; }
    }

    public class U_ID_ACT_ID
    {
        public string U_ID { get; set; }
        public string ACT_ID { get; set; }
    }

    public sealed class formData
    {
        public string Data { get; set; }
        public IFormFile? IMGFile { get; set; }
    }

}
