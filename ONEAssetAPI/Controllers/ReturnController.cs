using Microsoft.AspNetCore.Mvc;
using System.Data;
using Newtonsoft.Json;

namespace ONEAssetAPI.Controllers
{
    public class ReturnController : Controller
    {
        private MyClass.DbHelper dbHelper = new ONEAssetAPI.MyClass.DbHelper();
        private MyClass.authenHelper auth = new MyClass.authenHelper();
        private MyClass.AppConfigHelper appconfig = new MyClass.AppConfigHelper();
        private resExecuted oRes = new resExecuted();

        private string strAuthorization;
        private string strU_ID;
        private string strU_NAME;
        private string strU_ROLE;

        public class HReturn
        {
            public string RETURN_HID { get; set; }
            public string DOC_NO { get; set; }
            public string DOC_DATE { get; set; }
            public string RETURN_DATE { get; set; }
            public string CUST_ID { get; set; }
            public string CUST_COMPNAME { get; set; }
            public string CUST_ADDR1 { get; set; }
            public string CUST_ADDR2 { get; set; }
            public string CUST_NAME { get; set; }
            public string CUST_TEL { get; set; }
            public string TAX_NO { get; set; }
            public string NOTE { get; set; }
            public string STATUS { get; set; }
            public string CONFIRM_DATE { get; set; }
            public string CONFIRM_BY { get; set; }
            public string CREATE_BY { get; set; }
            public string UPDATE_BY { get; set; }
            public string DELETED_LIST { get; set; }
            public string CANCEL_REASON { get; set; }
            public string isCONFIRMED { get; set; }
        }
        public class IReturn
        {
            public string? RETURN_IID { get; set; }
            public string? RETURN_HID { get; set; }
            public string? BORROW_IID { get; set; }
            public string? PRODUCT_ID { get; set; }
            public string? QTY_BR { get; set; }
            public string? QTY_RT { get; set; }
            public string? LOC_ID_BR { get; set; }
            public string? LOC_ID_RT { get; set; }
            public string? CREATE_BY { get; set; }
        }
        public sealed class formReturnData
        {
            public string HData { get; set; }
            public string IData { get; set; }
            public string User { get; set; }
        }

        [HttpGet("getReturnList")] /* done */
        public IActionResult GetReturnList()
        {
            appconfig.ReadAppconfig();
            getRequestData();
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                string strQuery = @"select	TR_RETURN_H.RETURN_HID,TR_RETURN_H.DOC_NO,FORMAT(TR_RETURN_H.DOC_DATE, 'dd/MM/yyyy') as DOC_DATE,
		                                FORMAT(TR_RETURN_H.RETURN_DATE, 'dd/MM/yyyy') as RETURN_DATE,
		                                TR_RETURN_H.CUST_ID,TR_RETURN_H.CUST_NAME, TR_RETURN_H.CUST_COMPNAME, 
		                                TR_RETURN_H.CUST_TEL,TR_RETURN_H.CUST_ADDR1, TR_RETURN_H.CUST_ADDR2,TR_RETURN_H.TAX_NO,
                                        TR_RETURN_H.STATUS,TR_RETURN_H.NOTE
                                from TR_RETURN_H
                                ORDER BY TR_RETURN_H.RETURN_HID DESC";
                oRes.RESULT = "1";
                oRes.MESSAGE = "Success ";
                DataTable resTable = new DataTable();
                resTable = dbHelper.DBGetData(strQuery).Tables[0];
                string strReturn = JsonConvert.SerializeObject(new { data = resTable, response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }else{
                oRes.RESULT = "E";
                oRes.MESSAGE = "Invalid Token";
                return BadRequest(oRes);
            }           
        }

        [HttpGet("getReturnDetails")]/* done */
        public IActionResult GetReturnDetails(string RETURN_HID)
        {
            appconfig.ReadAppconfig();
            getRequestData();
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                string strQuery = @"select  TR_RETURN_I.RETURN_IID,TR_RETURN_I.RETURN_HID, TR_BORROW_I.BORROW_IID,TR_BORROW_I.PRODUCT_ID,
                                        TR_BORROW_H.DOC_NO as BORROW_DOCNO,TR_BORROW_H.CUST_NAME,TR_BORROW_H.CUST_COMPNAME,
		                                MT_PRODUCT.PRODUCT_CODE,MT_PRODUCT.PRODUCT_DESC,TR_RETURN_I.QTY_BR,TR_RETURN_I.QTY_RT,MT_UOM.UOM_DESC
                                from TR_RETURN_H
                                inner join TR_RETURN_I on TR_RETURN_H.RETURN_HID = TR_RETURN_I.RETURN_HID and TR_RETURN_I.IS_DEL IS NULL
                                inner join TR_BORROW_I on TR_RETURN_I.BORROW_IID = TR_BORROW_I.BORROW_IID
                                inner join TR_BORROW_H on TR_BORROW_H.BORROW_HID = TR_BORROW_I.BORROW_HID
                                left outer join MT_PRODUCT on TR_RETURN_I.PRODUCT_ID = MT_PRODUCT.PRODUCT_ID
                                left outer join MT_UOM on MT_PRODUCT.UOM_ID = MT_UOM.UOM_ID
                                WHERE TR_RETURN_H.RETURN_HID = " + RETURN_HID +
                                "order by TR_RETURN_I.RETURN_IID ";
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

        [HttpGet("getProductToReturn")]/* done */
        public IActionResult GetProductToReturn()
        {
            appconfig.ReadAppconfig();
            getRequestData();
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                string strQuery = @"select	TR_BORROW_H.DOC_NO as BORROW_DOCNO, TR_BORROW_H.CUST_NAME, TR_BORROW_H.CUST_COMPNAME, TR_BORROW_H.BORROW_TYPE as BORROW_TYPE,  
		                                case when TR_BORROW_H.BORROW_TYPE ='B' then 'ยืม' when  TR_BORROW_H.BORROW_TYPE ='R' then 'เช่า' else '' end + 
		                                case when TR_BORROW_H.BORROW_LOC = 'IN' then '-ในสถานที่' when TR_BORROW_H.BORROW_LOC = 'EX' THEN 'นอกสถานที่' else '' end as BORROW_TYPE_DESC,
		                                case when TR_BORROW_H.BORROW_LOC = 'IN' then MT_LOCATION.LOCATION when TR_BORROW_H.BORROW_LOC = 'EX' THEN TR_BORROW_H.BORROW_LOC_DESC else '' end as LOCATION,
		                                TR_BORROW_I.PRODUCT_ID,MT_PRODUCT.PRODUCT_CODE,MT_PRODUCT.PRODUCT_DESC,TR_BORROW_I.BORROW_IID,TR_BORROW_I.LOC_ID_TO as LOC_ID, QTY as QTY_BR,
		                                isnull(QTY_RT,0) as QTY_RT, QTY- isnull(QTY_RT,0) as QTY_REMAIN, MT_UOM.UOM_DESC
                                from TR_BORROW_H
                                inner join TR_BORROW_I on TR_BORROW_H.BORROW_HID = TR_BORROW_I.BORROW_HID
                                left outer join MT_PRODUCT on TR_BORROW_I.PRODUCT_ID = MT_PRODUCT.PRODUCT_ID and TR_BORROW_I.IS_DEL is null
                                left outer join MT_UOM on MT_PRODUCT.UOM_ID = MT_UOM.UOM_ID
                                left outer join MT_LOCATION on TR_BORROW_I.LOC_ID_TO = MT_LOCATION.LOC_ID
                                where TR_BORROW_H.STATUS = 'APPROVED' and QTY <> isnull(QTY_RT,0)";

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

        [HttpPost("createReturn")]/* done */
        public IActionResult CreateReturn([FromForm] formReturnData oData)
        {
            appconfig.ReadAppconfig();
            getRequestData();
            resExecuted oRes = new resExecuted();
            var objHReturn = Newtonsoft.Json.JsonConvert.DeserializeObject<HReturn>(oData.HData);
            var objIReturn = Newtonsoft.Json.JsonConvert.DeserializeObject<List<IReturn>>(oData.IData);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(oData.User);

            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                string strSQL = @"INSERT INTO TR_RETURN_H(  DOC_NO, DOC_DATE, RETURN_DATE, CUST_ID, CUST_COMPNAME, CUST_ADDR1, 
                                                        CUST_ADDR2, CUST_NAME, CUST_TEL, TAX_NO,NOTE, STATUS, 
                                                        CREATE_DATE, CREATE_BY) " +
                           " Values (dbo.genDOC_NO('RT'), getdate(), '" + objHReturn.RETURN_DATE + "', '" + objHReturn.CUST_ID + "','" + objHReturn.CUST_COMPNAME + "', '" + objHReturn.CUST_ADDR1 + "'," +
                                     "'" + objHReturn.CUST_ADDR2 + "','" + objHReturn.CUST_NAME + "','" + objHReturn.CUST_TEL + "','" + objHReturn.TAX_NO + "','" + objHReturn.NOTE + "','PENDING',getdate(),'" + objUser.U_NAME + "' ) SELECT SCOPE_IDENTITY()";

                string strResult = dbHelper.DBExecuteResult(strSQL);
                if (strResult != "-1")
                {
                    for (int i = 0; i < objIReturn.Count; i++)
                    {
                        strSQL = "INSERT INTO TR_RETURN_I(RETURN_HID,BORROW_IID, PRODUCT_ID, QTY_BR, QTY_RT,  CREATE_DATE, CREATE_BY) " +
                               " VALUES(" + strResult + "," + objIReturn[i].BORROW_IID + "," + objIReturn[i].PRODUCT_ID + "," + objIReturn[i].QTY_BR + "," + objIReturn[i].QTY_RT + ",getdate(),'" + objUser.U_NAME + "')";
                        dbHelper.DBExecute(strSQL);
                    }
                    oRes.RESULT = "1";
                    oRes.MESSAGE = "บันทึกข้อมูลสำเร็จ";
                }else{
                    oRes.RESULT = "0";
                    oRes.MESSAGE = "เกิดความผิดพลาดระหว่างการบันทึกข้อมูล";
                }
                string strReturn = JsonConvert.SerializeObject(new { response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            } else{
                oRes.RESULT = "E";
                oRes.MESSAGE = "Invalid Token";
                return BadRequest(oRes);
            }
        }

        [HttpPost("updateReturn")] /* done */
        public IActionResult UpdateReturn([FromForm] formReturnData oData)
        {
            appconfig.ReadAppconfig();
            getRequestData();
            resExecuted oRes = new resExecuted();
            var objHReturn = Newtonsoft.Json.JsonConvert.DeserializeObject<HReturn>(oData.HData);
            var objIReturn = Newtonsoft.Json.JsonConvert.DeserializeObject<List<IReturn>>(oData.IData);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(oData.User);
            string strSQL = "";

            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                if (objHReturn.STATUS == "PENDING")
                {
                    strSQL = @"UPDATE TR_RETURN_H set CUST_ID=" + objHReturn.CUST_ID + ", CUST_COMPNAME ='" + objHReturn.CUST_COMPNAME + "',CUST_ADDR1 = '" + objHReturn.CUST_ADDR1 + "'," +
                                     "CUST_ADDR2 = '" + objHReturn.CUST_ADDR2 + "',CUST_NAME='" + objHReturn.CUST_NAME + "' , CUST_TEL='" + objHReturn.CUST_TEL + "',TAX_NO = '" + objHReturn.TAX_NO + "'," +
                                     "RETURN_DATE='" + objHReturn.RETURN_DATE + "', " +
                                     "NOTE='" + objHReturn.NOTE + "', UPDATE_DATE=getdate(), UPDATE_BY='" + objUser.U_NAME + "'" +
                             " WHERE RETURN_HID = " + objHReturn.RETURN_HID;

                    dbHelper.DBExecute(strSQL);

                    if (objHReturn.DELETED_LIST != null && objHReturn.DELETED_LIST != "")
                    {
                        strSQL = @"UPDATE TR_RETURN_I SET IS_DEL='Y', DELETE_DATE=getdate(), DELETE_BY='" + objUser.U_NAME + "' WHERE RETURN_IID in (" + objHReturn.DELETED_LIST.Substring(1) + ")";
                        dbHelper.DBExecute(strSQL);
                    }

                    for (int i = 0; i < objIReturn.Count; i++)
                    {
                        if (objIReturn[i].RETURN_IID == null || objIReturn[i].RETURN_IID == "")
                        {
                            strSQL = "INSERT INTO TR_RETURN_I (RETURN_HID, BORROW_IID, PRODUCT_ID, QTY_BR, QTY_RT, CREATE_DATE, CREATE_BY) " +
                                     " VALUES(" + objHReturn.RETURN_HID + "," + objIReturn[i].BORROW_IID + "," + objIReturn[i].PRODUCT_ID + "," + objIReturn[i].QTY_BR + "," + objIReturn[i].QTY_RT + ",getdate(),'" + objUser.U_NAME + "')";
                            dbHelper.DBExecute(strSQL);
                        }
                        else
                        {
                            strSQL = "UPDATE TR_RETURN_I set BORROW_IID = '" + objIReturn[i].BORROW_IID + "', PRODUCT_ID = " + objIReturn[i].PRODUCT_ID + ", QTY_BR = " + objIReturn[i].QTY_BR + ", QTY_RT = " + objIReturn[i].QTY_RT +
                                            @", UPDATE_DATE = getdate(), UPDATE_BY ='" + objUser.U_NAME + "' " +
                                     "WHERE RETURN_IID=" + objIReturn[i].RETURN_IID;
                            dbHelper.DBExecute(strSQL);
                        }
                    }

                    if (objHReturn.isCONFIRMED == "1")
                    {

                        for (int i = 0; i < objIReturn.Count; i++)
                        {
                            strSQL = "select isnull(QTY_RT,0) from TR_BORROW_H inner join TR_BORROW_I on TR_BORROW_H.BORROW_HID = TR_BORROW_I.BORROW_HID " +
                                     "where TR_BORROW_H.STATUS = 'APPROVED' and TR_BORROW_I.BORROW_IID =" + objIReturn[i].BORROW_IID;
                            string result = dbHelper.DBExecuteResult(strSQL);
                            if (Convert.ToDecimal(result) >= Convert.ToDecimal(objIReturn[i].QTY_RT))
                            {
                                oRes.RESULT = "0";
                                oRes.MESSAGE = "รายการที่ " + (i + 1) + " จำนวนคืนมากกว่าจำนวนที่ยืม";
                                return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(oRes));
                            }
                        }
                        strSQL = @"UPDATE TR_RETURN_H set STATUS='CONFIRMED', CONFIRM_DATE=getdate(), CONFIRM_BY='" + objUser.U_NAME + "' WHERE RETURN_HID = " + objHReturn.RETURN_HID;
                        dbHelper.DBExecute(strSQL);
                        // loop to add QTY to stock
                        for (int i = 0; i < objIReturn.Count; i++)
                        {
                            strSQL = @" IF EXISTS(SELECT PRODUCT_ID FROM TR_STOCK WHERE LOC_ID = (select LOC_ID_FROM from TR_BORROW_I where BORROW_IID = " + objIReturn[i].BORROW_IID + ") AND PRODUCT_ID = " + objIReturn[i].PRODUCT_ID + @")
                                    BEGIN
                                        UPDATE TR_STOCK set QTY = QTY + " + objIReturn[i].QTY_RT + @",UPDATE_DATE = getdate() where PRODUCT_ID = " + objIReturn[i].PRODUCT_ID + " AND LOC_ID = (select LOC_ID_FROM from TR_BORROW_I where BORROW_IID = " + objIReturn[i].BORROW_IID + ") " +
                                      @"END 
                                    ELSE
                                    BEGIN
                                         INSERT INTO TR_STOCK(PRODUCT_ID, QTY , LOC_ID, CREATE_DATE) values(" + objIReturn[i].PRODUCT_ID + @", " + objIReturn[i].QTY_RT + ", (select LOC_ID_FROM from TR_BORROW_I where BORROW_IID = " + objIReturn[i].BORROW_IID + @"), getdate())
                                    END " +
                                     @" UPDATE TR_BORROW_I set QTY_RT = isnull(QTY_RT,0) + " + objIReturn[i].QTY_RT + @" where BORROW_IID = " + objIReturn[i].BORROW_IID;
                            dbHelper.DBExecute(strSQL);
                        }
                    }
                }
                else if (objHReturn.STATUS == "CONFIRMED")
                {
                    if (objHReturn.isCONFIRMED == "0")
                    {

                        for (int i = 0; i < objIReturn.Count; i++)
                        {
                            strSQL = "select QTY from tr_stock where LOC_ID = (select LOC_ID_FROM from TR_BORROW_I where BORROW_IID = " + objIReturn[i].BORROW_IID + ") AND PRODUCT_ID = " + objIReturn[i].PRODUCT_ID;
                            string result = dbHelper.DBExecuteResult(strSQL);
                            if (Convert.ToDecimal(result) < Convert.ToDecimal(objIReturn[i].QTY_RT))
                            {
                                oRes.RESULT = "0";
                                oRes.MESSAGE = "ไม่สามารถปลดยืนยันได้เนื่งอจาก รายการที่ " + (i + 1) + " จำนวนคงเหลือใน stock ไม่พอ";
                                return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(oRes));
                            }
                        }

                        strSQL = @"UPDATE TR_RETURN_H set STATUS='PENDING',UPDATE_DATE=getdate() WHERE RETURN_HID = " + objHReturn.RETURN_HID;
                        dbHelper.DBExecute(strSQL);

                        for (int i = 0; i < objIReturn.Count; i++)
                        {

                            strSQL = @" UPDATE TR_STOCK set QTY = QTY - " + objIReturn[i].QTY_RT + @",UPDATE_DATE = getdate() where PRODUCT_ID = " + objIReturn[i].PRODUCT_ID + " AND LOC_ID = (select LOC_ID_FROM from TR_BORROW_I where BORROW_IID = " + objIReturn[i].BORROW_IID + ") " +
                                     @" UPDATE TR_BORROW_I set QTY_RT = isnull(QTY_RT,0) - " + objIReturn[i].QTY_RT + @" where BORROW_IID = " + objIReturn[i].BORROW_IID;
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

        [HttpPost("cancelReturn")]/* done */
        public IActionResult CancelReturn([FromForm] formReturnData oData)
        {
            appconfig.ReadAppconfig();
            getRequestData();

            resExecuted oRes = new resExecuted();
            var objHReturn = Newtonsoft.Json.JsonConvert.DeserializeObject<HReturn>(oData.HData);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(oData.User);           
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                try{
                    string strSQL = @"UPDATE TR_RETURN_H set STATUS='CANCELED', CANCEL_DATE=getdate(), CANCEL_BY='" + objUser.U_NAME + "',CANCEL_REASON='" + objHReturn.CANCEL_REASON + "'  WHERE RETURN_HID = " + objHReturn.RETURN_HID;
                    dbHelper.DBExecute(strSQL);
                    oRes.RESULT = "1";
                    oRes.MESSAGE = "ยกเลิกเอกสารเลขที่ " + objHReturn.DOC_NO+ " สำเร็จ";
                }catch (Exception ex){
                    oRes.RESULT = "0";
                    oRes.MESSAGE = "เกิความผิดพลาดระหว่างการบันทึกข้อมูล " + ex.Message;
                }
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
