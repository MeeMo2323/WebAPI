using Microsoft.AspNetCore.Mvc;
using System.Data;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.IO;


namespace ONEAssetAPI.Controllers
{
    // [ApiController]
    // [Route("[controller]")]

    public sealed class formRpt
    {
        public string User { get; set; }
    }

    public class UserRpt
    {
        public string U_ID { get; set; }
        public string U_NAME { get; set; }
        public string U_ROLE { get; set; }
    }
    public sealed class formLogin
    { 
        public string User { get; set; }
    }

    public sealed class formProductData
    {
        public string Data { get; set; }
        public string User { get; set; }
        public IFormFile? IMGFile1 { get; set; }
        public IFormFile? IMGFile2 { get; set; }
        public IFormFile? IMGFile3 { get; set; }
    }
    public sealed class formVendorData
    {
        public string Data { get; set; }
        public string User { get; set; }
    }
    public class User
    {
        public string Action { get; set; }
        public string U_ID { get; set; }
        public string U_NAME { get; set; }

        public string U_PASS { get; set; }
    }
    public class Product
    {
        public string PRODUCT_ID { get; set; }
        public string PRODUCT_TYPE_ID { get; set; }
        public string PRODUCT_CAT_ID { get; set; }
        public string PRODUCT_GROUP_ID { get; set; }
        public string PRODUCT_SUBGROUP_ID { get; set; }
        public string PRODUCT_CODE { get; set; }
        public string PRODUCT_DESC { get; set; }
        public string UOM_ID { get; set; }
        public string IMG1 { get; set; }
        public string IMG2 { get; set; }
        public string IMG3 { get; set; }
        public string GEN_ID { get; set; }
        public string SCENE_ID { get; set; }
    }
    public class ProductGroup
    {
        public string PRODUCT_GROUP_ID { get; set; }
        public string PRODUCT_GROUP_ABBR { get; set; }
        public string PRODUCT_GROUP_DESC { get; set; }
    }
    public class ProductSubGroup
    {
        public string PRODUCT_SUBGROUP_ID { get; set; }
        public string PRODUCT_GROUP_ID { get; set; }
        public string PRODUCT_SUBGROUP_DESC { get; set; }
    }
    public class Vendor
    {
        public string VENDOR_ID { get; set; }
        public string VENDOR_DESC { get; set; }
    }
    public class Customer
    {
        public string CUST_ID { get; set; }
        public string CUST_NAME { get; set; }
        public string CUST_TEL { get; set; }
        public string CUST_COMPNAME { get; set; }
        public string CUST_ADDR1 { get; set; }
        public string CUST_ADDR2 { get; set; }
        public string TAX_NO { get; set; }
    }
        
    public class resExecuted
    {
        public string RESULT { get; set; }
        public string MESSAGE { get; set; }
    }
    public class MasterDataController : Controller
    {
        private MyClass.DbHelper dbHelper = new ONEAssetAPI.MyClass.DbHelper();
        private MyClass.authenHelper auth = new MyClass.authenHelper();
        private MyClass.AppConfigHelper appconfig = new MyClass.AppConfigHelper();
        private resExecuted oRes = new resExecuted();

        private string strAuthorization;
        private string strU_ID;
        private string strU_NAME;
        private string strU_ROLE;

        [HttpGet("getAllCustomer")] /* done */
        public IActionResult GetAllCustomer()
        {
            appconfig.ReadAppconfig();
            getRequestData();
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                string strQuery = @"SELECT CUST_ID,CUST_COMPNAME,CUST_ADDR1,CUST_ADDR2,CUST_NAME,CUST_TEL,TAX_NO
                                FROM MT_CUSTOMER
                                WHERE IS_DEL is null
                                order by CUST_NAME ";
                DataTable resTable = new DataTable();
                resTable = dbHelper.DBGetData(strQuery).Tables[0];
                string strReturn = JsonConvert.SerializeObject(new { data = resTable, response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }
            else
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = "Invalid Token";
                return BadRequest(oRes);
            }
            
        }

        [HttpGet("getAllProduct")] /* done */
        public IActionResult GetAllProduct()
        {
            appconfig.ReadAppconfig();
            getRequestData();
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                string strQuery = @"select  a.[PRODUCT_ID], a.[PRODUCT_TYPE_ID], [PRODUCT_CAT_ID], a.[PRODUCT_GROUP_ID],a.[PRODUCT_SUBGROUP_ID],
                                        b.PRODUCT_GROUP_DESC,c.PRODUCT_SUBGROUP_DESC, [PRODUCT_CODE], [PRODUCT_DESC],
		                                a.[UOM_ID], d.UOM_DESC, [IMG1], [IMG2], [IMG3], a.[IS_ACTIVE], a.[IS_DEL], a.[CREATE_DATE], a.[CREATE_BY], 
                                        a.[UPDATE_DATE], a.[UPDATE_BY], f.SCENE_ID, f.SCENE_DESC, h.GEN_ID, h.GENERATION_DESC
                                from MT_PRODUCT a
                                left outer join MT_PRODUCT_GROUP b on a.PRODUCT_GROUP_ID = b.PRODUCT_GROUP_ID
                                left outer join MT_PRODUCT_SUBGROUP c on a.PRODUCT_SUBGROUP_ID = c.PRODUCT_SUBGROUP_ID
                                left outer join MT_UOM d on a.UOM_ID = d.UOM_ID
                                left outer join MT_PRODUCT_SCENE e on a.PRODUCT_ID = e.PRODUCT_ID and e.IS_DEL is null
                                left outer join MT_SCENE f on e.SCENE_ID = f.SCENE_ID
                                left outer join MT_PRODUCT_GEN g on a.PRODUCT_ID = g.PRODUCT_ID and g.IS_DEL is null
                                left outer join MT_GENERATION h on g.GEN_ID = h.GEN_ID 
                                where a.IS_DEL is null and a.is_active <> 'N'
                                order by a.PRODUCT_ID ";
                oRes.RESULT = "1";
                oRes.MESSAGE = "Success ";
                DataTable resTable = new DataTable();
                resTable = dbHelper.DBGetData(strQuery).Tables[0];
                string strReturn = JsonConvert.SerializeObject(new { data = resTable, response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }
            else
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = "Invalid Token";
                return BadRequest(oRes);
            }           
        }

        [HttpGet("getAllProductGroup")] /* done */
        public IActionResult GetAllProductGroup()
        {
            appconfig.ReadAppconfig();
            getRequestData();
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                string strQuery = @"select  [PRODUCT_GROUP_ID], [PRODUCT_GROUP_ABBR], [PRODUCT_GROUP_DESC], [IS_ACTIVE],
                                        [IS_DEL], [CREATE_DATE], [CREATE_BY], [UPDATE_DATE], [UPDATE_BY]
                                from[MT_PRODUCT_GROUP]
                                where IS_DEL is null and IS_ACTIVE <> 'N'
                                order by [PRODUCT_GROUP_ID] ";
                oRes.RESULT = "1";
                oRes.MESSAGE = "Success ";
                DataTable resTable = new DataTable();
                resTable = dbHelper.DBGetData(strQuery).Tables[0];
                string strReturn = JsonConvert.SerializeObject(new { data = resTable, response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }
            else
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = "Invalid Token";
                return BadRequest(oRes);
            }          
        }

        [HttpGet("getAllProductSubGroup")] /* done */
        public IActionResult GetAllProductSubGroup(string strPRODUCT_GROUP_ID)
        {
            appconfig.ReadAppconfig();
            getRequestData();
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                string strQuery = @"select  [PRODUCT_GROUP_DESC], [PRODUCT_SUBGROUP_ID], a.[PRODUCT_GROUP_ID], 
                                        [PRODUCT_SUBGROUP_DESC], a.[IS_ACTIVE], a.[IS_DEL], a.[CREATE_DATE], 
                                        a.[CREATE_BY], a.[UPDATE_DATE], a.[UPDATE_BY], a.[DELETE_DATE], a.[DELETE_BY]
                                from	[MT_PRODUCT_SUBGROUP] a
                                left outer join [MT_PRODUCT_GROUP] b on a.PRODUCT_GROUP_ID = b.PRODUCT_GROUP_ID
                                where  a.IS_DEL is null and a.IS_ACTIVE <> 'N' and b.IS_DEL is null and b.IS_ACTIVE <> 'N' " + (strPRODUCT_GROUP_ID != "" && strPRODUCT_GROUP_ID != null ? "and a.PRODUCT_GROUP_ID = " + strPRODUCT_GROUP_ID : "") + @"
                                order by [PRODUCT_SUBGROUP_ID] ";

                oRes.RESULT = "1";
                oRes.MESSAGE = "Success ";
                DataTable resTable = new DataTable();
                resTable = dbHelper.DBGetData(strQuery).Tables[0];
                string strReturn = JsonConvert.SerializeObject(new { data = resTable, response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }
            else
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = "Invalid Token";
                return BadRequest(oRes);
            }
        }

        [HttpGet("getAllUOM")] /* done */
        public IActionResult GetAllUOM()
        {
            appconfig.ReadAppconfig();
            getRequestData();
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                string strQuery = @"select  [UOM_ID], [UOM_DESC], [IS_ACTIVE], [CREATE_DATE], [CREATE_BY], 
                                        [UPDATE_DATE], [UPDATE_BY]
                                from	[MT_UOM] 
                                where   IS_DEL is null and IS_ACTIVE <> 'N'
                                order by [UOM_ID] ";
                oRes.RESULT = "1";
                oRes.MESSAGE = "Success ";
                DataTable resTable = new DataTable();
                resTable = dbHelper.DBGetData(strQuery).Tables[0];
                string strReturn = JsonConvert.SerializeObject(new { data = resTable, response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }
            else
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = "Invalid Token";
                return BadRequest(oRes);
            }
        }

       /* [HttpGet("getAllLocation")]
        public IActionResult GetAllLocation()
        {
            string strQuery = @"select  [LOC_ID], [LOCATION], [IS_ACTIVE], [CREATE_DATE],
                                        [CREATE_BY], [UPDATE_DATE], [UPDATE_BY]
                                from	[MT_LOCATION] 
                                where   IS_DEL is null and IS_ACTIVE <> 'N'
                                order by [LOC_ID] ";
            DataTable resTable = new DataTable();
            resTable = dbHelper.DBGetData(strQuery).Tables[0];
            string strReturn = JsonConvert.SerializeObject(resTable, Newtonsoft.Json.Formatting.Indented);
            return Ok(strReturn);
        }*/

        [HttpGet("getAllScene")] /* done */
        public IActionResult GetAllScene()
        {
            appconfig.ReadAppconfig();
            getRequestData();
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                string strQuery = @"select  SCENE_ID, SCENE_DESC, IS_ACTIVE, CREATE_DATE, 
                                        CREATE_BY, UPDATE_DATE, UPDATE_BY
                                from	MT_SCENE 
                                where   IS_DEL is null and IS_ACTIVE <> 'N'
                                order by SCENE_ID ";
                oRes.RESULT = "1";
                oRes.MESSAGE = "Success ";
                DataTable resTable = new DataTable();
                resTable = dbHelper.DBGetData(strQuery).Tables[0];
                string strReturn = JsonConvert.SerializeObject(new { data = resTable, response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }
            else
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = "Invalid Token";
                return BadRequest(oRes);
            }
        }

        [HttpGet("getAllGeneration")]/*done*/
        public IActionResult GetAllGeneration()
        {
            appconfig.ReadAppconfig();
            getRequestData();
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                string strQuery = @"select  [GEN_ID], [GENERATION_DESC], [IS_ACTIVE], [CREATE_DATE] ,
                                        [CREATE_BY], [UPDATE_DATE], [UPDATE_BY]
                                from	[MT_GENERATION] 
                                where   IS_DEL is null and IS_ACTIVE <> 'N'
                                order by [GEN_ID] ";
                oRes.RESULT = "1";
                oRes.MESSAGE = "Success ";
                DataTable resTable = new DataTable();
                resTable = dbHelper.DBGetData(strQuery).Tables[0];
                string strReturn = JsonConvert.SerializeObject(new { data = resTable, response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }
            else
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = "Invalid Token";
                return BadRequest(oRes);
            }
           
        }

        [HttpGet("getAllVendor")] /* done */
        public IActionResult GetAllVendor()
        {
            appconfig.ReadAppconfig();
            getRequestData();
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                string strQuery = @"select  VENDOR_ID, VENDOR_DESC
                                from MT_VENDOR
                                where IS_DEL is null and IS_ACTIVE <> 'N'
                                order by VENDOR_ID";
                oRes.RESULT = "1";
                oRes.MESSAGE = "Success ";
                DataTable resTable = new DataTable();
                resTable = dbHelper.DBGetData(strQuery).Tables[0];
                string strReturn = JsonConvert.SerializeObject(new { data = resTable, response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }
            else
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = "Invalid Token";
                return BadRequest(oRes);
            }
                       
        }

        [HttpGet("getAllReason")] /* done */
        public IActionResult GettAllReason()
        {
            appconfig.ReadAppconfig();
            getRequestData();
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                string strQuery = @"select * from MT_REASON
                                where IS_DEL is null and IS_ACTIVE <> 'N'
                                order by REASON_DESC";
                oRes.RESULT = "1";
                oRes.MESSAGE = "Success ";
                DataTable resTable = new DataTable();
                resTable = dbHelper.DBGetData(strQuery).Tables[0];
                string strReturn = JsonConvert.SerializeObject(new { data = resTable, response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }
            else
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = "Invalid Token";
                return BadRequest(oRes);
            }           
        }

        [HttpPost("createProduct")] /* done */
        public IActionResult CreateProduct([FromForm] formProductData oData)
        {
            appconfig.ReadAppconfig();
            getRequestData();
            var objProduct = Newtonsoft.Json.JsonConvert.DeserializeObject<Product>(oData.Data);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(oData.User);
                     
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                string strSQL = "INSERT INTO MT_PRODUCT(PRODUCT_TYPE_ID, PRODUCT_CAT_ID, PRODUCT_GROUP_ID," +
                                                     " PRODUCT_SUBGROUP_ID, PRODUCT_CODE, PRODUCT_DESC, UOM_ID ," +
                                                     " IS_ACTIVE, CREATE_DATE, CREATE_BY) " +
                               "values('" + objProduct.PRODUCT_TYPE_ID + "','" + objProduct.PRODUCT_CAT_ID + "','" + objProduct.PRODUCT_GROUP_ID + "'," +
                                       "'" + objProduct.PRODUCT_SUBGROUP_ID + "','" + objProduct.PRODUCT_CODE + "','" + objProduct.PRODUCT_DESC + "'," +
                                       "'" + objProduct.UOM_ID + "','Y',getdate(),'" + objUser.U_NAME + "') SELECT SCOPE_IDENTITY()";

                string strResult = dbHelper.DBExecuteResult(strSQL);

                if (strResult != "-1")
                {
                    if (objProduct.GEN_ID != "")
                    {
                        strSQL = "INSERT INTO MT_PRODUCT_GEN (GEN_ID, PRODUCT_ID, IS_ACTIVE, CREATE_DATE, CREATE_BY) " +
                                                   " VALUES(" + objProduct.GEN_ID + "," + strResult + ",'Y',getdate(),'" + objUser.U_NAME + "')";
                        dbHelper.DBExecute(strSQL);
                    }

                    if (objProduct.SCENE_ID != "")
                    {
                        strSQL = "INSERT INTO MT_PRODUCT_SCENE(SCENE_ID, PRODUCT_ID, IS_ACTIVE, CREATE_DATE, CREATE_BY) " +
                                                   " VALUES(" + objProduct.SCENE_ID + "," + strResult + ",'Y',getdate(),'" + objUser.U_NAME + "')";
                        dbHelper.DBExecute(strSQL);
                    }
                    oRes.RESULT = "1";
                    oRes.MESSAGE = "บันทึกข้อมูลสำเร็จ";
                    if (oData.IMGFile1 != null && oData.IMGFile1.Length > 0)
                    {
                        dbHelper.DBExecute("UPDATE MT_PRODUCT set IMG1='" + strResult + "-1-" + objProduct.IMG1 + "' where PRODUCT_ID =" + strResult);
                       
                        try
                        {
                            using (FileStream fileStream = System.IO.File.Create(appconfig.IMGAssetUpload + strResult + "-1-" + objProduct.IMG1))
                            {
                                oData.IMGFile1.CopyTo(fileStream);
                                fileStream.Flush();
                                oRes.RESULT = "1";
                                oRes.MESSAGE = "บันทึกข้อมูลสำเร็จ";
                            }
                            //dbHelper.DBExecute("INSERT INTO TR_LOG(U_ID,ACT_ID,DESCRIPTION,CREATE_DATE) VALUES (" + jObj.U_ID + "," + strResult + ",'U_ID : " + jObj.U_ID + ",ACT_ID : " + strResult + " Create new dara , F_NAME : " + jObj.F_NAME_TH + " L_NAME : " + jObj.L_NAME_TH + " N_NAME : " + jObj.N_NAME_TH + " DP_NAME : " + jObj.DISPLAY_NAME + " SEX : " + jObj.SEX + " B_DATE : " + jObj.B_DATE + " BE_UNDER : " + jObj.BE_UNDER + " ACTING : " + jObj.ACTING + " IMG : " + strResult + "-" + oData.IMGFile.FileName + "',getdate())");
                        }
                        catch (Exception ex)
                        {
                            oRes.RESULT = "0";
                            oRes.MESSAGE = ex.Message; 
                        }
                    }
                    if (oData.IMGFile2 != null && oData.IMGFile2.Length > 0)
                    {
                        dbHelper.DBExecute("UPDATE MT_PRODUCT set IMG2='" + strResult + "-2-" + objProduct.IMG2 + "' where PRODUCT_ID =" + strResult);
                        try
                        {
                            using (FileStream fileStream = System.IO.File.Create(appconfig.IMGAssetUpload + strResult + "-2-" + objProduct.IMG2))
                            {
                                oData.IMGFile2.CopyTo(fileStream);
                                fileStream.Flush();
                                oRes.RESULT = "1";
                                oRes.MESSAGE = "บันทึกข้อมูลสำเร็จ";
                            }
                            //dbHelper.DBExecute("INSERT INTO TR_LOG(U_ID,ACT_ID,DESCRIPTION,CREATE_DATE) VALUES (" + jObj.U_ID + "," + strResult + ",'U_ID : " + jObj.U_ID + ",ACT_ID : " + strResult + " Create new dara , F_NAME : " + jObj.F_NAME_TH + " L_NAME : " + jObj.L_NAME_TH + " N_NAME : " + jObj.N_NAME_TH + " DP_NAME : " + jObj.DISPLAY_NAME + " SEX : " + jObj.SEX + " B_DATE : " + jObj.B_DATE + " BE_UNDER : " + jObj.BE_UNDER + " ACTING : " + jObj.ACTING + " IMG : " + strResult + "-" + oData.IMGFile.FileName + "',getdate())");
                        }
                        catch (Exception ex)
                        {
                            oRes.RESULT = "0";
                            oRes.MESSAGE = ex.Message;
                        }
                    }
                    if (oData.IMGFile3 != null && oData.IMGFile3.Length > 0)
                    {
                        dbHelper.DBExecute("UPDATE MT_PRODUCT set IMG3='" + strResult + "-3-" + objProduct.IMG3 + "' where PRODUCT_ID =" + strResult);
                        try
                        {
                            using (FileStream fileStream = System.IO.File.Create(appconfig.IMGAssetUpload + strResult + "-3-" + objProduct.IMG3))
                            {
                                oData.IMGFile2.CopyTo(fileStream);
                                fileStream.Flush();
                                oRes.RESULT = "1";
                                oRes.MESSAGE = "บันทึกข้อมูลสำเร็จ";
                            }
                            //dbHelper.DBExecute("INSERT INTO TR_LOG(U_ID,ACT_ID,DESCRIPTION,CREATE_DATE) VALUES (" + jObj.U_ID + "," + strResult + ",'U_ID : " + jObj.U_ID + ",ACT_ID : " + strResult + " Create new dara , F_NAME : " + jObj.F_NAME_TH + " L_NAME : " + jObj.L_NAME_TH + " N_NAME : " + jObj.N_NAME_TH + " DP_NAME : " + jObj.DISPLAY_NAME + " SEX : " + jObj.SEX + " B_DATE : " + jObj.B_DATE + " BE_UNDER : " + jObj.BE_UNDER + " ACTING : " + jObj.ACTING + " IMG : " + strResult + "-" + oData.IMGFile.FileName + "',getdate())");
                        }
                        catch (Exception ex)
                        {
                            oRes.RESULT = "0";
                            oRes.MESSAGE = ex.Message;
                        }
                    }
                }
                else
                {
                    oRes.RESULT = "0";
                    oRes.MESSAGE = "เกิดความผิดพลาดระหว่างการบันทึกข้อมูล";
                }
                string strReturn = JsonConvert.SerializeObject(new { response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }
            else
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = "Invalid Token";
                return BadRequest(oRes);
            }            
        }

        [HttpPost("deleteProduct")] /* done */
        public IActionResult deleteProduct([FromForm] formProductData oData)
        {
            appconfig.ReadAppconfig();
            getRequestData();
            var objProduct = Newtonsoft.Json.JsonConvert.DeserializeObject<Product>(oData.Data);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(oData.User);

            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                try
                {
                    string strSQL = " UPDATE MT_PRODUCT set IS_DEL ='Y',DELETE_DATE=getdate(),DELETE_BY = '" + objUser.U_NAME + "' WHERE PRODUCT_ID=" + objProduct.PRODUCT_ID;
                    dbHelper.DBExecute(strSQL);
                    //dbHelper.DBExecute("INSERT INTO TR_LOG(U_ID,ACT_ID,DESCRIPTION,CREATE_DATE) VALUES (" + jObj.U_ID + "," + jObj.ACT_ID + ",'U_ID : " + jObj.U_ID + ",ACT_ID : " + jObj.ACT_ID + " Delete dara',getdate())");
                    oRes.RESULT = "1";
                    oRes.MESSAGE = "บันทึกข้อมูลสำเร็จ";
                }
                catch (Exception e)
                {
                    oRes.RESULT = "0";
                    oRes.MESSAGE = e.Message;
                }
                string strReturn = JsonConvert.SerializeObject(new { response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }
            else
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = "Invalid Token";
                return BadRequest(oRes);
            }
           
        }

        [HttpPost("updateProduct")] /* done */
        public IActionResult UpdateProduct([FromForm] formProductData oData)
        {
            appconfig.ReadAppconfig();
            getRequestData();
            var objProduct = Newtonsoft.Json.JsonConvert.DeserializeObject<Product>(oData.Data);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(oData.User);   
            
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                try
                {
                    string strSQL = "UPDATE MT_PRODUCT set PRODUCT_TYPE_ID = '" + objProduct.PRODUCT_TYPE_ID + "',PRODUCT_CAT_ID ='" + objProduct.PRODUCT_CAT_ID + "'," +
                                                             "PRODUCT_GROUP_ID ='" + objProduct.PRODUCT_GROUP_ID + "',PRODUCT_SUBGROUP_ID = '" + objProduct.PRODUCT_SUBGROUP_ID + "',PRODUCT_DESC = '" + objProduct.PRODUCT_DESC + "'," +
                                                             "UOM_ID=" + objProduct.UOM_ID + ",update_date = getdate(),update_by ='" + objUser.U_NAME + "' " +
                                    "WHERE PRODUCT_ID=" + objProduct.PRODUCT_ID;
                    dbHelper.DBExecute(strSQL);
                    //dbHelper.DBExecute("INSERT INTO TR_LOG(U_ID,ACT_ID,DESCRIPTION,CREATE_DATE) VALUES (" + jObj.U_ID + "," + jObj.ACT_ID + ",'U_ID : " + jObj.U_ID + ",ACT_ID : " + jObj.ACT_ID + " Update dara => F_NAME : " + jObj.F_NAME_TH + ", L_NAME : " + jObj.L_NAME_TH + ", N_NAME : " + jObj.N_NAME_TH + ", DP_NAME : " + jObj.DISPLAY_NAME + ", SEX : " + jObj.SEX + ", B_DATE : " + jObj.B_DATE + ", BE_UNDER : " + jObj.BE_UNDER + ", ACTING : " + jObj.ACTING + "',getdate())");
                    oRes.RESULT = "1";
                    oRes.MESSAGE = "บันทึกข้อมูลสำเร็จ";

                    if (objProduct.GEN_ID != "")
                    {
                        strSQL = @" if exists(SELECT top 1 GEN_ID from MT_PRODUCT_GEN where PRODUCT_ID  =" + objProduct.PRODUCT_ID + @" and IS_DEL is null)            
                                 BEGIN
                                    update MT_PRODUCT_GEN set GEN_ID = " + objProduct.GEN_ID + ", UPDATE_DATE=getdate(), UPDATE_BY='" + objUser.U_NAME + "' where PRODUCT_ID = " + objProduct.PRODUCT_ID + @" and IS_DEL is null 
                                 End
                                 else
                                 begin
                                    INSERT INTO MT_PRODUCT_GEN(GEN_ID, PRODUCT_ID, IS_ACTIVE, CREATE_DATE, CREATE_BY)
                                    VALUES(" + objProduct.GEN_ID + @"," + objProduct.PRODUCT_ID + @", 'Y', getdate(), '" + objUser.U_NAME + @"')
                                 end ";
                        dbHelper.DBExecute(strSQL);
                    }
                    else
                    {
                        strSQL = @"  update MT_PRODUCT_GEN set IS_DEL = 'Y', DELETE_DATE=getdate(), DELETE_BY='" + objUser.U_NAME + "' where PRODUCT_ID = " + objProduct.PRODUCT_ID + " and IS_DEL is null";
                        dbHelper.DBExecute(strSQL);
                    }

                    if (objProduct.SCENE_ID != "")
                    {
                        strSQL = @" if exists(SELECT top 1 SCENE_ID from MT_PRODUCT_SCENE where PRODUCT_ID  =" + objProduct.PRODUCT_ID + @" and IS_DEL is null)             
                                 BEGIN
                                    update MT_PRODUCT_SCENE set SCENE_ID = " + objProduct.SCENE_ID + ", UPDATE_DATE=getdate(), UPDATE_BY='" + objUser.U_NAME + "' where PRODUCT_ID = " + objProduct.PRODUCT_ID + @" and IS_DEL is null 
                                 End
                                 else
                                 begin
                                    INSERT INTO MT_PRODUCT_SCENE(SCENE_ID, PRODUCT_ID, IS_ACTIVE, CREATE_DATE, CREATE_BY)
                                    VALUES(" + objProduct.SCENE_ID + @"," + objProduct.PRODUCT_ID + @", 'Y', getdate(), '" + objUser.U_NAME + @"')
                                 end ";
                        dbHelper.DBExecute(strSQL);
                    }
                    else
                    {
                        strSQL = @"  update MT_PRODUCT_SCENE set IS_DEL = 'Y', DELETE_DATE=getdate(), DELETE_BY='" + objUser.U_NAME + "' where  PRODUCT_ID = " + objProduct.PRODUCT_ID + " and IS_DEL is null";
                        dbHelper.DBExecute(strSQL);
                    }
                }
                catch (Exception e)
                {
                    oRes.RESULT = "0";
                    oRes.MESSAGE = e.Message;
                }

                if (oData.IMGFile1 != null && oData.IMGFile1.Length > 0)
                {
                    dbHelper.DBExecute("UPDATE MT_PRODUCT set IMG1='" + objProduct.PRODUCT_ID + "-1-" + objProduct.IMG1 + "' where PRODUCT_ID =" + objProduct.PRODUCT_ID);

                    try
                    {
                        using (FileStream fileStream = System.IO.File.Create(appconfig.IMGAssetUpload + objProduct.PRODUCT_ID + "-1-" + objProduct.IMG1))
                        {
                            oData.IMGFile1.CopyTo(fileStream);
                            fileStream.Flush();
                            oRes.RESULT = "1";
                            oRes.MESSAGE = "บันทึกข้อมูลสำเร็จ";
                        }
                    }
                    catch (Exception ex)
                    {
                        oRes.RESULT = "0";
                        oRes.MESSAGE = ex.Message;
                    }
                }
                if (oData.IMGFile2 != null && oData.IMGFile2.Length > 0)
                {
                    dbHelper.DBExecute("UPDATE MT_PRODUCT set IMG2='" + objProduct.PRODUCT_ID + "-2-" + objProduct.IMG2 + "' where PRODUCT_ID =" + objProduct.PRODUCT_ID);

                    try
                    {
                        using (FileStream fileStream = System.IO.File.Create(appconfig.IMGAssetUpload + objProduct.PRODUCT_ID + "-2-" + objProduct.IMG2))
                        {
                            oData.IMGFile2.CopyTo(fileStream);
                            fileStream.Flush();
                            oRes.RESULT = "1";
                            oRes.MESSAGE = "บันทึกข้อมูลสำเร็จ";
                        }
                    }
                    catch (Exception ex)
                    {
                        oRes.RESULT = "0";
                        oRes.MESSAGE = ex.Message;
                    }
                }
                if (oData.IMGFile3 != null && oData.IMGFile1.Length > 0)
                {
                    dbHelper.DBExecute("UPDATE MT_PRODUCT set IMG3='" + objProduct.PRODUCT_ID + "-3-" + objProduct.IMG1 + "' where PRODUCT_ID =" + objProduct.PRODUCT_ID);
                    try
                    {
                        using (FileStream fileStream = System.IO.File.Create(appconfig.IMGAssetUpload + objProduct.PRODUCT_ID + "-3-" + objProduct.IMG3))
                        {
                            oData.IMGFile3.CopyTo(fileStream);
                            fileStream.Flush();
                            oRes.RESULT = "1";
                            oRes.MESSAGE = "บันทึกข้อมูลสำเร็จ";
                        }
                    }
                    catch (Exception ex)
                    {
                        oRes.RESULT = "0";
                        oRes.MESSAGE = ex.Message;
                    }
                }

                string strReturn = JsonConvert.SerializeObject(new { response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);

            }
            else
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = "Invalid Token";
                return BadRequest(oRes);
            }


        }
             

        [HttpPost("createProductGroup")]  /* done */
        public IActionResult CreateProductGroup([FromForm] formProductData oData)
        {
            appconfig.ReadAppconfig();
            getRequestData();
            var objProductGroup = Newtonsoft.Json.JsonConvert.DeserializeObject<ProductGroup>(oData.Data);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(oData.User);
                      
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                try
                {
                    string strSQL = "INSERT INTO MT_PRODUCT_GROUP(PRODUCT_GROUP_ABBR, PRODUCT_GROUP_DESC, IS_ACTIVE, CREATE_DATE, CREATE_BY) " +
                                   "values('" + objProductGroup.PRODUCT_GROUP_ABBR + "','" + objProductGroup.PRODUCT_GROUP_DESC + "','Y', getdate(),'" + objUser.U_NAME + "') ";

                    dbHelper.DBExecute(strSQL);
                    oRes.RESULT = "1";
                    oRes.MESSAGE = "บันทึกข้อมูลสำเร็จ";                   
                }
                catch (Exception e)
                {
                    oRes.RESULT = "0";
                    oRes.MESSAGE = e.Message;
                }
                string strReturn = JsonConvert.SerializeObject(new { response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }
            else
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = "Invalid Token";
                return BadRequest(oRes);
            }
        }

        [HttpPost("updateProductGroup")]  /* done */
        public IActionResult UpdateProductGroup([FromForm] formProductData oData)
        {
            appconfig.ReadAppconfig();
            getRequestData();
            var objProductGroup = Newtonsoft.Json.JsonConvert.DeserializeObject<ProductGroup>(oData.Data);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(oData.User);

            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                try
                {
                    string strSQL = "UPDATE MT_PRODUCT_GROUP set PRODUCT_GROUP_ABBR = '" + objProductGroup.PRODUCT_GROUP_ABBR + "',PRODUCT_GROUP_DESC ='" + objProductGroup.PRODUCT_GROUP_DESC + "'," +
                                                             "update_date = getdate(),update_by ='" + objUser.U_NAME + "' " +
                                    "WHERE PRODUCT_GROUP_ID=" + objProductGroup.PRODUCT_GROUP_ID;
                    dbHelper.DBExecute(strSQL);
                    oRes.RESULT = "1";
                    oRes.MESSAGE = "บันทึกข้อมูลสำเร็จ";
                }
                catch (Exception e)
                {
                    oRes.RESULT = "0";
                    oRes.MESSAGE = e.Message;
                }
                string strReturn = JsonConvert.SerializeObject(new { response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }
            else
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = "Invalid Token";
                return BadRequest(oRes);
            }
        }

        [HttpPost("deleteProductGroup")] /* done */
        public IActionResult DeleteProductGroup([FromForm] formProductData oData)
        {
            appconfig.ReadAppconfig();
            getRequestData();
            var objProductGroup = Newtonsoft.Json.JsonConvert.DeserializeObject<ProductGroup>(oData.Data);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(oData.User);
           
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                try
                {
                    string strSQL = " UPDATE MT_PRODUCT_GROUP set IS_DEL ='Y',DELETE_DATE=getdate(),DELETE_BY = '" + objUser.U_NAME + "' WHERE PRODUCT_GROUP_ID=" + objProductGroup.PRODUCT_GROUP_ID;
                    dbHelper.DBExecute(strSQL);
                    oRes.RESULT = "1";
                    oRes.MESSAGE = "ลบข้อมูลสำเร็จ";
                }
                catch (Exception e)
                {
                    oRes.RESULT = "0";
                    oRes.MESSAGE = e.Message;
                }
                string strReturn = JsonConvert.SerializeObject(new { response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }else{
                oRes.RESULT = "E";
                oRes.MESSAGE = "Invalid Token";
                return BadRequest(oRes);
            }
           
        }

        [HttpPost("createProductSubGroup")] /* done */
        public IActionResult CreateProductSubGroup([FromForm] formProductData oData)
        {
            appconfig.ReadAppconfig();
            getRequestData();
            var objProductSubGroup = Newtonsoft.Json.JsonConvert.DeserializeObject<ProductSubGroup>(oData.Data);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(oData.User);
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                try
                {
                    string strSQL = "INSERT INTO MT_PRODUCT_SUBGROUP(PRODUCT_SUBGROUP_DESC, PRODUCT_GROUP_ID, IS_ACTIVE, CREATE_DATE, CREATE_BY) " +
                                   "values('" + objProductSubGroup.PRODUCT_SUBGROUP_DESC + "','" + objProductSubGroup.PRODUCT_GROUP_ID + "','Y', getdate(),'" + objUser.U_NAME + "') ";

                    dbHelper.DBExecute(strSQL);
                    oRes.RESULT = "1";
                    oRes.MESSAGE = "บันทึกข้อมูลสำเร็จ";
                }
                catch (Exception e)
                {
                    oRes.RESULT = "0";
                    oRes.MESSAGE = e.Message;
                }
                string strReturn = JsonConvert.SerializeObject(new { response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }
            else
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = "Invalid Token";
                return BadRequest(oRes);
            }
           
        }

        [HttpPost("updateProductSubGroup")] /* done */
        public IActionResult UpdateProductSubGroup([FromForm] formProductData oData)
        {
            appconfig.ReadAppconfig();
            getRequestData();
            var objProductSubGroup = Newtonsoft.Json.JsonConvert.DeserializeObject<ProductSubGroup>(oData.Data);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(oData.User);
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                try
                {
                    string strSQL = "UPDATE MT_PRODUCT_SUBGROUP set PRODUCT_SUBGROUP_DESC = '" + objProductSubGroup.PRODUCT_SUBGROUP_DESC + "', PRODUCT_GROUP_ID = " + objProductSubGroup.PRODUCT_GROUP_ID +
                                                                   " update_date = getdate(),update_by ='" + objUser.U_NAME + "' " +
                                    "WHERE PRODUCT_GROUP_ID=" + objProductSubGroup.PRODUCT_SUBGROUP_ID;
                    dbHelper.DBExecute(strSQL);
                    oRes.RESULT = "1";
                    oRes.MESSAGE = "บันทึกข้อมูลสำเร็จ";
                }
                catch (Exception e)
                {
                    oRes.RESULT = "0";
                    oRes.MESSAGE = e.Message;
                }

                string strReturn = JsonConvert.SerializeObject(new { response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }
            else
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = "Invalid Token";
                return BadRequest(oRes);
            }

           
        }

        [HttpPost("deleteProductSubGroup")] /* done */
        public IActionResult DeleteProductSubGroup([FromForm] formProductData oData)
        {
            appconfig.ReadAppconfig();
            getRequestData();
            var objProductSubGroup = Newtonsoft.Json.JsonConvert.DeserializeObject<ProductSubGroup>(oData.Data);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(oData.User);
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                try
                {
                    string strSQL = " UPDATE MT_PRODUCT_SUBGROUP set IS_DEL ='Y',DELETE_DATE=getdate(),DELETE_BY = '" + objUser.U_NAME + "' WHERE PRODUCT_SUBGROUP_ID=" + objProductSubGroup.PRODUCT_SUBGROUP_ID;
                    dbHelper.DBExecute(strSQL);
                    oRes.RESULT = "1";
                    oRes.MESSAGE = "ลบข้อมูลสำเร็จ";
                }
                catch (Exception e)
                {
                    oRes.RESULT = "0";
                    oRes.MESSAGE = e.Message;
                }
                string strReturn = JsonConvert.SerializeObject(new { response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }
            else
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = "Invalid Token";
                return BadRequest(oRes);
            }            
        }

        [HttpPost("createVendor")] /* done */
        public IActionResult CreateVendor([FromForm] formVendorData oData)
        {
            appconfig.ReadAppconfig();
            getRequestData();
            var objVendor = Newtonsoft.Json.JsonConvert.DeserializeObject<Vendor>(oData.Data);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(oData.User);
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                try
                {
                    string strSQL = "INSERT INTO MT_VENDOR(VENDOR_DESC, IS_ACTIVE, CREATE_DATE, CREATE_BY) " +
                                   "values('" + objVendor.VENDOR_DESC + "','Y', getdate(),'" + objUser.U_NAME + "') ";

                    dbHelper.DBExecute(strSQL);
                    oRes.RESULT = "1";
                    oRes.MESSAGE = "บันทึกข้อมูลสำเร็จ";
                }
                catch (Exception e)
                {
                    oRes.RESULT = "0";
                    oRes.MESSAGE = e.Message;
                }

                string strReturn = JsonConvert.SerializeObject(new { response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }
            else
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = "Invalid Token";
                return BadRequest(oRes);
            }           
        }

        [HttpPost("updateVendor")] /* done */
        public IActionResult UpdateVendor([FromForm] formVendorData oData)
        {
            appconfig.ReadAppconfig();
            getRequestData();
            var objVendor = Newtonsoft.Json.JsonConvert.DeserializeObject<Vendor>(oData.Data);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(oData.User);
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                try
                {
                    string strSQL = "UPDATE MT_VENDOR set VENDOR_DESC = '" + objVendor.VENDOR_DESC + "'," +
                                                             "update_date = getdate(),update_by ='" + objUser.U_NAME + "' " +
                                    "WHERE VENDOR_ID=" + objVendor.VENDOR_ID;
                    dbHelper.DBExecute(strSQL);
                    oRes.RESULT = "1";
                    oRes.MESSAGE = "บันทึกข้อมูลสำเร็จ";
                }
                catch (Exception e)
                {
                    oRes.RESULT = "0";
                    oRes.MESSAGE = e.Message;
                }

                string strReturn = JsonConvert.SerializeObject(new { response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }
            else
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = "Invalid Token";
                return BadRequest(oRes);
            }

            
        }

        [HttpPost("deleteVendor")] /* done */
        public IActionResult DeleteVendor([FromForm] formVendorData oData)
        {
            appconfig.ReadAppconfig();
            getRequestData();
            var objVendor = Newtonsoft.Json.JsonConvert.DeserializeObject<Vendor>(oData.Data);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(oData.User);
           
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                try
                {
                    string strSQL = " UPDATE MT_VENDOR set IS_DEL ='Y',DELETE_DATE=getdate(),DELETE_BY = '" + objUser.U_NAME + "' WHERE VENDOR_ID=" + objVendor.VENDOR_ID;
                    dbHelper.DBExecute(strSQL);
                    oRes.RESULT = "1";
                    oRes.MESSAGE = "ลบข้อมูลสำเร็จ";
                }
                catch (Exception e)
                {
                    oRes.RESULT = "0";
                    oRes.MESSAGE = e.Message;
                }
                string strReturn = JsonConvert.SerializeObject(new { response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }
            else
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = "Invalid Token";
                return BadRequest(oRes);
            }
                      
        }
                
        /* RECEIVE */
        [HttpGet("getTempProductCodeSeq")]
        public IActionResult GetTempProductCodeSeq(string strSearch)
        {
            string strQuery = @"select top 1 *
                                from 
                                (
                                select Right('000000'+convert(varchar,(convert(integer,right(PRODUCT_CODE,6))+1)),6) as tempProductCode,PRODUCT_ID
                                from MT_PRODUCT where PRODUCT_CODE like '%"+ strSearch + @"%'
                                union all 
                                select '000001',-1
                                ) as res order by PRODUCT_ID desc ";
            DataTable resTable = new DataTable();
            resTable = dbHelper.DBGetData(strQuery).Tables[0];
            string strReturn = JsonConvert.SerializeObject(resTable, Newtonsoft.Json.Formatting.Indented);
            return Ok(strReturn);
        }

        /* log in*/
        [HttpPost("logIn")]
        public IActionResult LogIn2([FromForm] formLogin oData)
        {
            var jObj = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(oData.User);
            string strToken = "";
            MyClass.AppConfigHelper appconfig = new MyClass.AppConfigHelper();
            appconfig.ReadAppconfig();
            string query = @"select a.U_ID,MT_ROLE.U_ROLE,a.U_NAME,a.U_EMAIL
                             from [WebPortal].[dbo].[MT_USER] a
                             inner join MT_USER_ROLE on a.U_ID = MT_USER_ROLE.U_ID
                             inner join MT_ROLE on MT_USER_ROLE.ROLE_ID = MT_ROLE.ROLE_ID 
                             where a.U_NAME='" + jObj.U_NAME + "' and U_PASSHASH='" + appconfig.passwordEncrypt(jObj.U_PASS, appconfig.ApplicationSecret) + "' and a.IS_DEL is null";

            DataTable resTable = new DataTable();
            resTable = dbHelper.DBGetData(query).Tables[0];

            if (resTable.Rows.Count > 0)
            {
                dbHelper.DBExecute("UPDATE MT_USER_ROLE set LAST_LOGIN = getdate() where U_ID=" + resTable.Rows[0]["U_ID"].ToString());
                strToken = auth.GenerateToken(resTable.Rows[0]["U_ID"].ToString(), resTable.Rows[0]["U_ROLE"].ToString(), resTable.Rows[0]["U_NAME"].ToString(), Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod);
                string strReturn = JsonConvert.SerializeObject(new { U_ID = resTable.Rows[0]["U_ID"].ToString(), U_ROLE = resTable.Rows[0]["U_ROLE"].ToString(), U_NAME = resTable.Rows[0]["U_NAME"].ToString(), U_EMAIL = resTable.Rows[0]["U_EMAIL"].ToString(), token = strToken  }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }
            else
            {
                return Ok(JsonConvert.SerializeObject(new { U_ID = -1 }, Newtonsoft.Json.Formatting.Indented));
            }
        }

        [HttpPost("createCustomer")] /* done */
        public IActionResult CreateCustomer([FromForm] formProductData oData)
        {
            appconfig.ReadAppconfig();
            getRequestData();
            var objCustomer = Newtonsoft.Json.JsonConvert.DeserializeObject<Customer>(oData.Data);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(oData.User);

            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                try
                {
                    string strSQL = "INSERT INTO MT_CUSTOMER(CUST_COMPNAME,CUST_ADDR1,CUST_ADDR2,CUST_NAME,CUST_TEL,TAX_NO, CREATE_DATE, CREATE_BY) " +
                                   "values('" + objCustomer.CUST_COMPNAME + "','" + objCustomer.CUST_ADDR1 + "','" + objCustomer.CUST_ADDR2 + "','" + objCustomer.CUST_NAME + "','" + objCustomer.CUST_TEL + "','" + objCustomer.TAX_NO + "',getdate(),'" + objUser.U_NAME + "') ";

                    dbHelper.DBExecute(strSQL);
                    oRes.RESULT = "1";
                    oRes.MESSAGE = "บันทึกข้อมูลสำเร็จ";
                }
                catch (Exception e)
                {
                    oRes.RESULT = "0";
                    oRes.MESSAGE = e.Message;
                }
                string strReturn = JsonConvert.SerializeObject(new { response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }
            else
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = "Invalid Token";
                return BadRequest(oRes);
            }


        }

        [HttpPost("updateCustomer")] /* done */
        public IActionResult UpdateCustomer([FromForm] formProductData oData)
        {
            appconfig.ReadAppconfig();
            getRequestData();
            var objCustomer = Newtonsoft.Json.JsonConvert.DeserializeObject<Customer>(oData.Data);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(oData.User);

            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                try
                {
                    string strSQL = "UPDATE MT_CUSTOMER set CUST_NAME = '" + objCustomer.CUST_NAME + "',CUST_COMPNAME ='" + objCustomer.CUST_COMPNAME + "'," +
                                            "CUST_ADDR1='" + objCustomer.CUST_ADDR1 + "',CUST_ADDR2 = '" + objCustomer.CUST_ADDR2 + "'," +
                                            "CUST_TEL='" + objCustomer.CUST_TEL + "',TAX_NO='" + objCustomer.TAX_NO + "'," +
                                            "update_date = getdate(), update_by ='" + objUser.U_NAME + "' " +
                                    "WHERE CUST_ID=" + objCustomer.CUST_ID;
                    dbHelper.DBExecute(strSQL);
                    oRes.RESULT = "1";
                    oRes.MESSAGE = "บันทึกข้อมูลสำเร็จ";
                }
                catch (Exception e)
                {
                    oRes.RESULT = "0";
                    oRes.MESSAGE = e.Message;
                }
                string strReturn = JsonConvert.SerializeObject(new { response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }
            else
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = "Invalid Token";
                return BadRequest(oRes);
            }            
        }

        [HttpPost("deleteCustomer")] /* done */
        public IActionResult DeleteCustomer([FromForm] formProductData oData)
        {
            appconfig.ReadAppconfig();
            getRequestData();
            var objCustomer = Newtonsoft.Json.JsonConvert.DeserializeObject<Customer>(oData.Data);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(oData.User);

            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                try
                {
                    string strSQL = " UPDATE MT_CUSTOMER set IS_DEL ='Y',DELETE_DATE=getdate(),DELETE_BY = '" + objUser.U_NAME + "' WHERE CUST_ID=" + objCustomer.CUST_ID;
                    dbHelper.DBExecute(strSQL);
                    oRes.RESULT = "1";
                    oRes.MESSAGE = "บันทึกข้อมูลสำเร็จ";
                }
                catch (Exception e)
                {
                    oRes.RESULT = "0";
                    oRes.MESSAGE = e.Message;
                }
                string strReturn = JsonConvert.SerializeObject(new { response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }
            else
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = "Invalid Token";
                return BadRequest(oRes);
            }
           
        }

        [HttpGet("getRptStockRemain")] /* done */
        public IActionResult GetRptStockRemain()
        {
            appconfig.ReadAppconfig();
            getRequestData();

            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                string strQuery = @" select MT_PRODUCT_GROUP.PRODUCT_GROUP_DESC, MT_PRODUCT_SUBGROUP.PRODUCT_SUBGROUP_DESC,
		                                 MT_LOCATION.LOC_CODE,MT_LOCATION.LOCATION, QTY_STOCK+QTY_BR as QTY,QTY_STOCK,QTY_BR
                                  from 
                                  (
	                                  select  MT_PRODUCT.PRODUCT_GROUP_ID,MT_PRODUCT.PRODUCT_SUBGROUP_ID,LOC_ID, sum(oStock.QTY_STOCK) as QTY_STOCK,sum(oStock.QTY_BR) as QTY_BR 
	                                  from 
	                                  (
		                                  select TR_BORROW_H.BORROW_TYPE as STOCK_TYPE, TR_BORROW_I.PRODUCT_ID,TR_BORROW_I.LOC_ID_TO as LOC_ID, 0 as QTY_STOCK,(QTY - ISNULL(QTY_RT,0)) as QTY_BR
		                                  from TR_BORROW_H
		                                  inner join TR_BORROW_I on TR_BORROW_H.BORROW_HID = TR_BORROW_I.BORROW_HID
		                                  where TR_BORROW_H.STATUS = 'APPROVED'
		                                  UNION ALL
		                                  select 'S',PRODUCT_ID,LOC_ID, QTY as QTY_STOCK, 0 as QTYBR from TR_STOCK
	                                   ) as oStock
	                                   left outer join MT_PRODUCT on oStock.PRODUCT_ID = MT_PRODUCT.PRODUCT_ID
	                                   group by MT_PRODUCT.PRODUCT_GROUP_ID,MT_PRODUCT.PRODUCT_SUBGROUP_ID,LOC_ID
                                   ) as resStock
                                   left outer join MT_PRODUCT_GROUP on resStock.PRODUCT_GROUP_ID = MT_PRODUCT_GROUP.PRODUCT_GROUP_ID
                                   left outer join MT_PRODUCT_SUBGROUP on resStock.PRODUCT_SUBGROUP_ID = MT_PRODUCT_SUBGROUP.PRODUCT_SUBGROUP_ID
                                   left outer join MT_LOCATION on resStock.LOC_ID = MT_LOCATION.LOC_ID
                                   order by  MT_PRODUCT_GROUP.PRODUCT_GROUP_DESC, MT_PRODUCT_SUBGROUP.PRODUCT_SUBGROUP_DESC,MT_LOCATION.LOC_CODE";

                oRes.RESULT = "1";
                oRes.MESSAGE = "Success ";
                DataTable resTable = new DataTable();
                resTable = dbHelper.DBGetData(strQuery).Tables[0];
                string strReturn = JsonConvert.SerializeObject(new { data = resTable, response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }
            else
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = "Invalid Token";
                return BadRequest(oRes);
            }

        }
        private void getRequestData()
        {
            strAuthorization = Request.Headers["Authorization"];
            strU_ID = Request.Headers["U_ID"];
            strU_NAME = Request.Headers["U_NAME"];
            strU_ROLE = Request.Headers["U_ROLE"];
        }
    }
}
