using Microsoft.AspNetCore.Mvc;
using System.Data;
using Newtonsoft.Json;

namespace ONEAssetAPI.Controllers
{
    public class HBorrow
    {
        public string BORROW_HID { get; set; }
        public string DOC_NO { get; set; }
        public string DOC_DATE { get; set; }
        public string CUST_ID { get; set; }
        public string CUST_COMPNAME { get; set; }
        public string CUST_ADDR1 { get; set; }
        public string CUST_ADDR2 { get; set; }
        public string CUST_NAME { get; set; }
        public string CUST_TEL { get; set; }
        public string TAX_NO { get; set; }
        public string BORROW_TYPE { get; set; }
        public string BORROW_LOC { get; set; }
        public string BORROW_LOC_DESC { get; set; }
        public string BORROW_VENDOR { get; set; }
        public string BORROW_FROM { get; set; }
        public string BORROW_TO { get; set; }
        public string BORROW_REASON { get; set; }
        public string NOTE { get; set; }
        public string STATUS { get; set; }
        public string APPROVED_DATE { get; set; }
        public string CREATE_DATE { get; set; }
        public string CREATE_BY { get; set; }
        public string UPDATE_DATE { get; set; }
        public string UPDATE_BY { get; set; }
        public string DELETED_LIST { get; set; }
        public string CANCEL_REASON { get; set; }
        public string isAPPROVED { get; set; }
    }
    public class IBorrow
    {
        public string? BORROW_IID { get; set; }
        public string? BORROW_HID { get; set; }
        public string? PRODUCT_ID { get; set; }
        public string? UNIT_PRICE { get; set; }
        public string? QTY { get; set; }        
        public string? LINE_AMOUNT { get; set; }
        public string? LOC_ID_FROM { get; set; }
        public string? LOC_ID_TO { get; set; }
    }
    public sealed class formBorrowData
    {
        public string HData { get; set; }
        public string IData { get; set; }
        public string User { get; set; }
    }
    public class BorrowController : Controller
    {
        private MyClass.DbHelper dbHelper = new ONEAssetAPI.MyClass.DbHelper();
        private MyClass.authenHelper auth = new MyClass.authenHelper();
        private MyClass.AppConfigHelper appconfig = new MyClass.AppConfigHelper();
        private resExecuted oRes = new resExecuted();

        private string strAuthorization;
        private string strU_ID;
        private string strU_NAME;
        private string strU_ROLE;       

        [HttpGet("getBorrowList")]/*done*/
        public IActionResult GetBorrowList()
        {
            appconfig.ReadAppconfig();           
            getRequestData();
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                string strQuery = @"select	TR_BORROW_H.BORROW_HID,TR_BORROW_H.DOC_NO,TR_BORROW_H.BORROW_TYPE,FORMAT(TR_BORROW_H.DOC_DATE, 'dd/MM/yyyy') as DOC_DATE,
                                        TR_BORROW_H.CUST_ID, TR_BORROW_H.CUST_COMPNAME, TR_BORROW_H.CUST_ADDR1, TR_BORROW_H.CUST_ADDR2,
                                        TR_BORROW_H.CUST_NAME,TR_BORROW_H.CUST_TEL, TR_BORROW_H.TAX_NO,
		                                TR_BORROW_H.BORROW_FROM, TR_BORROW_H.BORROW_TO, TR_BORROW_H.BORROW_VENDOR,TR_BORROW_H.BORROW_LOC,TR_BORROW_H.BORROW_LOC_DESC,
                                        TR_BORROW_H.BORROW_TYPE, CASE WHEN TR_BORROW_H.BORROW_TYPE ='B' then 'ยืม' when TR_BORROW_H.BORROW_TYPE ='R' then 'เช่า' else '' end as BORROW_TYPE_DESC,
                                        TR_BORROW_H.STATUS, TR_BORROW_H.BORROW_REASON, TR_BORROW_H.NOTE, TR_BORROW_H.CREATE_BY,
                                        CONVERT(varchar, BORROW_FROM, 103) as RPT_BORROW_FROM,CONVERT(varchar, BORROW_TO, 103) as RPT_BORROW_TO,
                                        datediff(day,BORROW_FROM,BORROW_TO) as BORROW_DATE_COUNT
                                from	TR_BORROW_H
                                order by TR_BORROW_H.BORROW_HID desc  ";

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

        [HttpGet("getBorrowDetails")]/*done*/
        public IActionResult GetBorrowDetails(string BORROW_HID)
        {
            appconfig.ReadAppconfig();
            getRequestData();
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                string strQuery = @"select  *,MT_LOCATION2.LOC_CODE as LOC_CODE2
                                from TR_BORROW_H
                                inner join TR_BORROW_I on TR_BORROW_H.BORROW_HID = TR_BORROW_I.BORROW_HID
                                left outer join MT_PRODUCT on TR_BORROW_I.PRODUCT_ID = MT_PRODUCT.PRODUCT_ID
                                left outer join MT_UOM on MT_PRODUCT.UOM_ID = MT_UOM.UOM_ID
                                left outer join MT_LOCATION on TR_BORROW_I.LOC_ID_FROM = MT_LOCATION.LOC_ID
                                left outer join MT_LOCATION MT_LOCATION2 on TR_BORROW_I.LOC_ID_TO = MT_LOCATION2.LOC_ID
                                WHERE TR_BORROW_I.IS_DEL is null and TR_BORROW_H.BORROW_HID = " + BORROW_HID + @"
                                order by TR_BORROW_I.BORROW_IID ";

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

        [HttpGet("getRemainStock")]/*done*/
        public IActionResult GetRemainStock()
        {
            appconfig.ReadAppconfig();
            getRequestData();
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                string strQuery = @"select  MT_PRODUCT.PRODUCT_ID, MT_PRODUCT.PRODUCT_TYPE_ID, PRODUCT_CAT_ID, 
		                                MT_PRODUCT.PRODUCT_GROUP_ID,MT_PRODUCT.PRODUCT_SUBGROUP_ID,
                                        MT_PRODUCT_GROUP.PRODUCT_GROUP_DESC,MT_PRODUCT_SUBGROUP.PRODUCT_SUBGROUP_DESC, 
		                                PRODUCT_CODE, PRODUCT_DESC, TR_STOCK.QTY,TR_STOCK.LOC_ID, MT_LOCATION.LOC_CODE , MT_LOCATION.LOCATION, MT_PRODUCT.UOM_ID, MT_UOM.UOM_DESC, 
		                                IMG1, IMG2, IMG3, MT_PRODUCT.IS_ACTIVE, MT_PRODUCT.IS_DEL, 
		                                MT_SCENE.SCENE_ID, MT_SCENE.SCENE_DESC, MT_GENERATION.GEN_ID, MT_GENERATION.GENERATION_DESC
                                from MT_PRODUCT 
                                inner join TR_STOCK on MT_PRODUCT.PRODUCT_ID = TR_STOCK.PRODUCT_ID
                                left outer join MT_PRODUCT_GROUP on MT_PRODUCT.PRODUCT_GROUP_ID = MT_PRODUCT_GROUP.PRODUCT_GROUP_ID
                                left outer join MT_PRODUCT_SUBGROUP on MT_PRODUCT.PRODUCT_SUBGROUP_ID = MT_PRODUCT_SUBGROUP.PRODUCT_SUBGROUP_ID
                                left outer join MT_UOM on MT_PRODUCT.UOM_ID = MT_UOM.UOM_ID
                                left outer join MT_PRODUCT_SCENE on MT_PRODUCT.PRODUCT_ID = MT_PRODUCT_SCENE.PRODUCT_ID and MT_PRODUCT_SCENE.IS_DEL is null
                                left outer join MT_SCENE on MT_PRODUCT_SCENE.SCENE_ID = MT_SCENE.SCENE_ID
                                left outer join MT_PRODUCT_GEN on MT_PRODUCT.PRODUCT_ID = MT_PRODUCT_GEN.PRODUCT_ID and MT_PRODUCT_GEN.IS_DEL is null
                                left outer join MT_GENERATION on MT_PRODUCT_GEN.GEN_ID = MT_GENERATION.GEN_ID 
                                left outer join MT_LOCATION on TR_STOCK.LOC_ID = MT_LOCATION.LOC_ID
                                where MT_PRODUCT.IS_DEL is null and MT_PRODUCT.is_active <> 'N' and TR_STOCK.QTY > 0
                                order by MT_PRODUCT.PRODUCT_ID ";

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

        [HttpPost("createBorrow")]/*done*/
        public IActionResult CreateBorrow([FromForm] formBorrowData oData)
        {
            appconfig.ReadAppconfig();
            getRequestData();
            var objHBorrow = Newtonsoft.Json.JsonConvert.DeserializeObject<HBorrow>(oData.HData);
            var objIBorrow = Newtonsoft.Json.JsonConvert.DeserializeObject<List<IBorrow>>(oData.IData);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(oData.User);
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                string strSQL = @"INSERT INTO TR_BORROW_H(
                                                        DOC_NO, DOC_DATE, CUST_ID, CUST_COMPNAME, CUST_ADDR1, 
                                                        CUST_ADDR2, CUST_NAME, CUST_TEL, TAX_NO, BORROW_TYPE,
                                                        BORROW_FROM,BORROW_TO,
                                                        BORROW_LOC , BORROW_LOC_DESC, BORROW_VENDOR, BORROW_REASON, NOTE, STATUS, 
                                                        CREATE_DATE, CREATE_BY) " +
                           " Values (dbo.genDOC_NO('BR'), getdate(), " + objHBorrow.CUST_ID + ", '" + objHBorrow.CUST_COMPNAME + "', '" + objHBorrow.CUST_ADDR1 + "'," +
                                     "'" + objHBorrow.CUST_ADDR2 + "','" + objHBorrow.CUST_NAME + "','" + objHBorrow.CUST_TEL + "','" + objHBorrow.TAX_NO + "','" + objHBorrow.BORROW_TYPE + "','" + objHBorrow.BORROW_FROM + "','" + objHBorrow.BORROW_TO + "'," +
                                     "'" + objHBorrow.BORROW_LOC + "','" + objHBorrow.BORROW_LOC_DESC + "','" + objHBorrow.BORROW_VENDOR + "','" + objHBorrow.BORROW_REASON + "','" + objHBorrow.NOTE + "','PENDING',getdate(),'" + objUser.U_NAME + "' ) SELECT SCOPE_IDENTITY()";

                string strResult = dbHelper.DBExecuteResult(strSQL);
                if (strResult != "-1")
                {
                    for (int i = 0; i < objIBorrow.Count; i++)
                    {
                        strSQL = "INSERT INTO TR_BORROW_I(BORROW_HID, PRODUCT_ID, QTY, LOC_ID_FROM , LOC_ID_TO , CREATE_DATE, CREATE_BY) " +
                               " VALUES(" + strResult + "," + objIBorrow[i].PRODUCT_ID + "," + objIBorrow[i].QTY + "," + objIBorrow[i].LOC_ID_FROM + "," + objIBorrow[i].LOC_ID_TO + ",getdate(),'" + objUser.U_NAME + "')";
                        dbHelper.DBExecute(strSQL);
                    }
                    oRes.RESULT = "1";
                    oRes.MESSAGE = "บันทึกข้อมูลสำเร็จ";
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

        [HttpGet("getAllLocation")]/*done*/
        public IActionResult GetAllLocation()
        {
            appconfig.ReadAppconfig();
            getRequestData();
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                string strQuery = @"SELECT LOC_ID,LOC_CODE,LOCATION,ISNULL(IS_DEFAULT,'') as IS_DEFAULT
                                FROM MT_LOCATION
                                WHERE IS_DEL IS NULL
                                ORDER BY LOC_ID ";
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

        [HttpPost("updateBorrow")]/*done*/
        public IActionResult UpdateBorrow([FromForm] formBorrowData oData)
        {
            appconfig.ReadAppconfig();
            getRequestData();

            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                var objHBorrow = Newtonsoft.Json.JsonConvert.DeserializeObject<HBorrow>(oData.HData);
                var objIBorrow = Newtonsoft.Json.JsonConvert.DeserializeObject<List<IBorrow>>(oData.IData);
                var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(oData.User);
                string strSQL = "";
                if (objHBorrow.STATUS == "PENDING")
                {
                    strSQL = @"UPDATE TR_BORROW_H set CUST_ID=" + objHBorrow.CUST_ID + ", CUST_COMPNAME ='" + objHBorrow.CUST_COMPNAME + "',CUST_ADDR1 = '" + objHBorrow.CUST_ADDR1 + "'," +
                                     "CUST_ADDR2 = '" + objHBorrow.CUST_ADDR2 + "',CUST_NAME='" + objHBorrow.CUST_NAME + "' , CUST_TEL='" + objHBorrow.CUST_TEL + "',TAX_NO = '" + objHBorrow.TAX_NO + "'," +
                                     "BORROW_TYPE='" + objHBorrow.BORROW_TYPE + "',BORROW_LOC='" + objHBorrow.BORROW_LOC + "', BORROW_LOC_DESC = '" + (objHBorrow.BORROW_LOC == "IN" ? "" : objHBorrow.BORROW_LOC_DESC) + "', BORROW_FROM='" + objHBorrow.BORROW_FROM + "'," +
                                     "BORROW_TO='" + objHBorrow.BORROW_TO + "', BORROW_VENDOR='" + objHBorrow.BORROW_VENDOR + "', BORROW_REASON='" + objHBorrow.BORROW_REASON + "'," +
                                     "NOTE='" + objHBorrow.NOTE + "', UPDATE_DATE=getdate(), UPDATE_BY='" + objUser.U_NAME + "'" +
                             " WHERE BORROW_HID = " + objHBorrow.BORROW_HID;

                    dbHelper.DBExecute(strSQL);

                    if (objHBorrow.DELETED_LIST != null && objHBorrow.DELETED_LIST != "")
                    {
                        strSQL = @"UPDATE TR_BORROW_I SET IS_DEL='Y', DELETE_DATE=getdate(), DELETE_BY='" + objUser.U_NAME + "' WHERE BORROW_IID in (" + objHBorrow.DELETED_LIST.Substring(1) + ")";
                        dbHelper.DBExecute(strSQL);
                    }

                    for (int i = 0; i < objIBorrow.Count; i++)
                    {
                        if (objIBorrow[i].BORROW_IID == null || objIBorrow[i].BORROW_IID == "")
                        {
                            strSQL = "INSERT INTO TR_BORROW_I (BORROW_HID, PRODUCT_ID, QTY, LOC_ID_FROM, LOC_ID_TO, UNIT_PRICE,  CREATE_DATE, CREATE_BY) " +
                                     " VALUES(" + objHBorrow.BORROW_HID + "," + objIBorrow[i].PRODUCT_ID + "," + objIBorrow[i].QTY + "," + objIBorrow[i].LOC_ID_FROM + "," + (objHBorrow.BORROW_LOC == "EX" ? "null" : objIBorrow[i].LOC_ID_TO) + "," + (objIBorrow[i].UNIT_PRICE == null || objIBorrow[i].UNIT_PRICE == "" ? "null" : objIBorrow[i].UNIT_PRICE) + ",getdate(),'" + objUser.U_NAME + "')";
                            dbHelper.DBExecute(strSQL);
                        }
                        else
                        {
                            strSQL = "UPDATE TR_BORROW_I set PRODUCT_ID = " + objIBorrow[i].PRODUCT_ID + ", QTY = " + objIBorrow[i].QTY +
                                            (objIBorrow[i].LOC_ID_FROM != null ? ",LOC_ID_FROM=" + objIBorrow[i].LOC_ID_FROM : "") +
                                            (objIBorrow[i].LOC_ID_TO != null ? ",LOC_ID_TO=" + objIBorrow[i].LOC_ID_TO : "") +
                                            (objIBorrow[i].UNIT_PRICE != null ? ",UNIT_PRICE=" + objIBorrow[i].UNIT_PRICE : "") +
                                            @", UPDATE_DATE = getdate(), UPDATE_BY ='" + objUser.U_NAME + "' " +
                                     "WHERE BORROW_IID=" + objIBorrow[i].BORROW_IID;
                            dbHelper.DBExecute(strSQL);
                        }
                    }

                    if (objHBorrow.isAPPROVED == "1")
                    {

                        for (int i = 0; i < objIBorrow.Count; i++)
                        {
                            strSQL = "select case when QTY >=" + objIBorrow[i].QTY + " then 'Y' else 'N' end from TR_STOCK where LOC_ID = " + objIBorrow[i].LOC_ID_FROM + " and PRODUCT_ID = " + objIBorrow[i].PRODUCT_ID;
                            string result = dbHelper.DBExecuteResult(strSQL);
                            if (result == "N")
                            {
                                oRes.RESULT = "E";
                                oRes.MESSAGE = "รายการที่ " + (i + 1) + " จำนวนคงเหลือในระบบไม่เพียงพอ";
                                return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(oRes));
                            }

                        }
                        strSQL = @"UPDATE TR_BORROW_H set STATUS='APPROVED', APPROVE_DATE=getdate(), APPROVE_BY='" + objUser.U_NAME + "' WHERE BORROW_HID = " + objHBorrow.BORROW_HID;
                        dbHelper.DBExecute(strSQL);
                        // loop to decrease QTY from stock
                        for (int i = 0; i < objIBorrow.Count; i++)
                        {
                            strSQL = @"UPDATE TR_STOCK set QTY = QTY - " + objIBorrow[i].QTY + @",UPDATE_DATE = getdate() where LOC_ID = " + objIBorrow[i].LOC_ID_FROM + " AND PRODUCT_ID = " + objIBorrow[i].PRODUCT_ID;
                            dbHelper.DBExecute(strSQL);
                        }
                    }
                }
                else if (objHBorrow.STATUS == "APPROVED")
                {
                    if (objHBorrow.isAPPROVED == "0")
                    {
                        strSQL = @"UPDATE TR_BORROW_H set STATUS='PENDING',UPDATE_DATE=getdate() WHERE BORROW_HID = " + objHBorrow.BORROW_HID;
                        dbHelper.DBExecute(strSQL);

                        for (int i = 0; i < objIBorrow.Count; i++)
                        {

                            strSQL = @"IF EXISTS(SELECT PRODUCT_ID FROM TR_STOCK WHERE LOC_ID =" + objIBorrow[i].LOC_ID_FROM + " AND PRODUCT_ID =" + objIBorrow[i].PRODUCT_ID + @")            
                                     BEGIN
                                         UPDATE TR_STOCK set QTY = QTY + " + objIBorrow[i].QTY + @",UPDATE_DATE = getdate() where LOC_ID =" + objIBorrow[i].LOC_ID_FROM + " AND PRODUCT_ID = " + objIBorrow[i].PRODUCT_ID + @"
                                     END
                                     ELSE
                                     BEGIN
                                         INSERT INTO TR_STOCK(PRODUCT_ID, QTY , LOC_ID, CREATE_DATE) values(" + objIBorrow[i].PRODUCT_ID + @"," + objIBorrow[i].QTY + "," + objIBorrow[i].LOC_ID_FROM + @", getdate())
                                     END ";
                            dbHelper.DBExecute(strSQL);
                        }
                    }
                }
                oRes.RESULT = "1";
                oRes.MESSAGE = "บันทึกข้อมูลสำเร็จ";
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

        [HttpPost("cancelBorrow")]/*done*/
        public IActionResult CancelBorrow([FromForm] formReceiveData oData)
        {
            appconfig.ReadAppconfig();
            getRequestData();
            resExecuted oRes = new resExecuted();
            var objHBorrow = Newtonsoft.Json.JsonConvert.DeserializeObject<HBorrow>(oData.HData);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(oData.User);
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                string strSQL = @"UPDATE TR_BORROW_H set STATUS='CANCELED', CANCEL_DATE=getdate(), CANCEL_BY='" + objUser.U_NAME + "',CANCEL_REASON='" + objHBorrow.CANCEL_REASON + "'  WHERE BORROW_HID = " + objHBorrow.BORROW_HID;
                dbHelper.DBExecute(strSQL);

                oRes.RESULT = "1";
                oRes.MESSAGE = "ยกเลิกเอกสารเลขที่ " + objHBorrow.DOC_NO + " สำเร็จ";
                string strReturn = JsonConvert.SerializeObject(new { response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }else{
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
