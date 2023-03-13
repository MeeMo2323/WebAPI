using Microsoft.AspNetCore.Mvc;
using System.Data;
using Newtonsoft.Json;

namespace ONEAssetAPI.Controllers
{
    public class HReceive
    {
        public string RECEIVE_HID { get; set; }
        public string DOC_NO { get; set; }
        public string SET_NO { get; set; }
        public string DOC_DATE { get; set; }
        public string RECEIVE_DATE { get; set; }
        public string VENDOR_ID { get; set; }
        public string VENDOR_DESC { get; set; }
        public string NOTE { get; set; }
        public string STATUS { get; set; }
        public string APPROVED_DATE { get; set; }
        public string CREATE_DATE { get; set; }
        public string CREATE_BY { get; set; }
        public string UPDATE_DATE { get; set; }
        public string UPDATE_BY { get; set; }
        public string DELETED_LIST { get; set; }
        public string CANCEL_REASON { get; set; }
        public string isCONFIRMED { get; set; }
    }
    public class IReceive
    {
        public string? RECEIVE_IID { get; set; }
        public string? RECEIVE_HID { get; set; }
        public string? PRODUCT_ID { get; set; }
        public string? PRODUCT_CODE { get; set; }
        public string? QTY { get; set; }
        public string? QTY_IN { get; set; }
        public string? UNIT_PRICE { get; set; }
        public string? FINE_PRICE { get; set; }
        public string? START_DATE { get; set; }
        public string? EXPIRE_DATE { get; set; }
        public string? WH_ID { get; set; }
        public string? REASON_ID { get; set; }
    }
    public sealed class formReceiveData
    {
        public string HData { get; set; }
        public string IData { get; set; }
        public string User { get; set; }
    }
    public sealed class formAttachment
    {
        public string Data { get; set; }
        public string User { get; set; }
        public IFormFile? File1 { get; set; }
    }
    public class Attachement
    {
        public string ATTM_ID { get; set; }
        public string RECEIVE_IID { get; set; }
        public string FILE_NAME { get; set; }
    }
    public class ReceiveController : Controller
    {
        private MyClass.DbHelper dbHelper = new ONEAssetAPI.MyClass.DbHelper();
        private MyClass.authenHelper auth = new MyClass.authenHelper();
        private MyClass.AppConfigHelper appconfig = new MyClass.AppConfigHelper();
        private resExecuted oRes = new resExecuted();

        private string strAuthorization;
        private string strU_ID;
        private string strU_NAME;
        private string strU_ROLE;

        [HttpPost("createReceive")] /* done */
        public IActionResult CreateReceive([FromForm] formReceiveData oData)
        {
            appconfig.ReadAppconfig();
            getRequestData();
            var objHreceive = Newtonsoft.Json.JsonConvert.DeserializeObject<HReceive>(oData.HData);
            var objIreceive = Newtonsoft.Json.JsonConvert.DeserializeObject<List<IReceive>>(oData.IData);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(oData.User);

            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                string strSQL = "select COUNT(RECEIVE_HID) FROM TR_RECEIVE_H WHERE VENDOR_ID = " + objHreceive.VENDOR_ID + " and SET_NO = " + objHreceive.SET_NO + " and STATUS <> 'CANCELED' ";
                string strCountResult = dbHelper.DBExecuteResult(strSQL);
                if (strCountResult != "0")
                {
                    oRes.RESULT = "0";
                    oRes.MESSAGE = "เลขที่ชุดซ้ำในระบบ";
                   // return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(oRes));
                }
                else
                {
                    strSQL = @"INSERT INTO TR_RECEIVE_H (DOC_NO,SET_NO, DOC_DATE, RECEIVE_DATE, VENDOR_ID, VENDOR_DESC, NOTE, STATUS, CREATE_DATE, CREATE_BY) " +
                               " Values (dbo.genDOC_NO('RE'),'" + objHreceive.SET_NO + "',getdate(),'" + objHreceive.RECEIVE_DATE + "','" + objHreceive.VENDOR_ID + "','" + objHreceive.VENDOR_DESC + "','" + objHreceive.NOTE + "','PENDING',getdate(),'" + objUser.U_NAME + "' ) SELECT SCOPE_IDENTITY()";

                    string strResult = dbHelper.DBExecuteResult(strSQL);
                    if (strResult != "-1")
                    {
                        for (int i = 0; i < objIreceive.Count; i++)
                        {
                            strSQL = "INSERT INTO TR_RECEIVE_I (RECEIVE_HID, PRODUCT_ID, PRODUCT_CODE, QTY, UNIT_PRICE, WH_ID, CREATE_DATE, CREATE_BY) " +
                                   " VALUES(" + strResult + "," + objIreceive[i].PRODUCT_ID + ",'" + objIreceive[i].PRODUCT_CODE + "'," + objIreceive[i].QTY + "," + objIreceive[i].UNIT_PRICE + "," + objIreceive[i].WH_ID + ",getdate(),'" + objUser.U_NAME + "')";
                            dbHelper.DBExecute(strSQL);
                        }
                        oRes.RESULT = "1";
                        oRes.MESSAGE = "บันทึกข้อมูลสำเร็จ";
                    }
                    else
                    {
                        oRes.RESULT = "0";
                        oRes.MESSAGE = "เกิดความผิพลาดระหว่างการบันทึกข้อมูล";
                    }
                   // return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(oRes));
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

        [HttpPost("addReceiveAttached")] /* done */
        public IActionResult AddReceiveAttached([FromForm] formAttachment oData)
        {
            appconfig.ReadAppconfig();
            getRequestData();
            var objAttachement = Newtonsoft.Json.JsonConvert.DeserializeObject<Attachement>(oData.Data);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(oData.User);
           
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                string strSQL = "INSERT INTO TR_ATTACHMENT(RECEIVE_IID,CREATE_DATE,CREATE_BY) " +
                               "values(" + objAttachement.RECEIVE_IID + ",getdate(),'" + objUser.U_NAME + "') SELECT SCOPE_IDENTITY()";

                string strResult = dbHelper.DBExecuteResult(strSQL);
                if (strResult != "-1")
                {
                    strSQL = "UPDATE TR_ATTACHMENT SET FILE_NAME = '" + strResult + "-" + objAttachement.FILE_NAME + "' WHERE ATTM_ID = " + strResult;
                    dbHelper.DBExecute(strSQL);
                    if (oData.File1 != null && oData.File1.Length > 0)
                    {
                        try
                        {
                            using (FileStream fileStream = System.IO.File.Create(appconfig.AttachementUpload + strResult + "-" + objAttachement.FILE_NAME))
                            {
                                oData.File1.CopyTo(fileStream);
                                fileStream.Flush();
                                oRes.RESULT = "1";
                                oRes.MESSAGE = "บันทึกข้อมูลสำเร็จ";
                            }
                        }catch (Exception ex){
                            oRes.RESULT = "0";
                            oRes.MESSAGE = ex.Message;
                        }
                    }
                }else{
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

        [HttpGet("getAttachement")] /* done */
        public IActionResult GetAttachement(string RECEIVE_IID)
        {
            appconfig.ReadAppconfig();
            getRequestData();
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                string strQuery = @"select FILE_NAME as FILE_NAME
                                from TR_ATTACHMENT
                                where TR_ATTACHMENT.IS_DEL is null and RECEIVE_IID =" + RECEIVE_IID;

                DataTable resTable = new DataTable();
                resTable = dbHelper.DBGetData(strQuery).Tables[0];
                string strReturn = JsonConvert.SerializeObject(new { data = resTable,response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }
            else
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = "Invalid Token";
                return BadRequest(oRes);
            }
          
        }

        [HttpPost("deleteAttachement")] /* done */
        public IActionResult DeleteAttachement([FromForm] formAttachment oData)
        {
            appconfig.ReadAppconfig();
            getRequestData();
            var objAttachement = Newtonsoft.Json.JsonConvert.DeserializeObject<Attachement>(oData.Data);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(oData.User);
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                try
                {
                    string strSQL = " UPDATE TR_ATTACHMENT set IS_DEL ='Y',DELETE_DATE=getdate(),DELETE_BY = '" + objUser.U_NAME + "' WHERE ATTM_ID=" + objAttachement.ATTM_ID;
                    dbHelper.DBExecute(strSQL);
                    oRes.RESULT = "1";
                    oRes.MESSAGE = "ลบไฟล์แนบสำเร็จ";

                    try
                    {
                        // Moving the file file.txt to location C:\gfg.txt
                        System.IO.File.Move(appconfig.AttachementUpload + objAttachement.FILE_NAME, appconfig.AttachementUpload + @"deletedItems\\" + objAttachement.FILE_NAME);
                    }
                    catch (IOException ex)
                    {
                        oRes.RESULT = "0";
                        oRes.MESSAGE = ex.Message;
                    }

                }
                catch (Exception ex)
                {
                    oRes.RESULT = "0";
                    oRes.MESSAGE = ex.Message;
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

        [HttpGet("getRptReceiveFines")] /* done */
        public IActionResult GetRptReceiveFines(string VENDOR_ID)
        {
            appconfig.ReadAppconfig();
            getRequestData();
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                string strQuery = @"select  TR_RECEIVE_H.VENDOR_ID,MT_VENDOR.VENDOR_DESC,TR_RECEIVE_H.SET_NO,MT_PRODUCT.PRODUCT_DESC,MT_PRODUCT.IMG1, TR_RECEIVE_I.FINE_PRICE
                                from    TR_RECEIVE_H 
                                inner join TR_RECEIVE_I on TR_RECEIVE_H.RECEIVE_HID = TR_RECEIVE_I.RECEIVE_HID and TR_RECEIVE_I.IS_DEL is null
                                left outer join MT_PRODUCT on TR_RECEIVE_I.PRODUCT_ID = MT_PRODUCT.PRODUCT_ID 
                                left outer join MT_VENDOR on TR_RECEIVE_H.VENDOR_ID = MT_VENDOR.VENDOR_ID
                                where TR_RECEIVE_H.STATUS ='CONFIRMED' and isnull(FINE_PRICE,0) >0 AND TR_RECEIVE_H.VENDOR_ID = " + VENDOR_ID + @" 
                                order by TR_RECEIVE_I.RECEIVE_IID ";
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

        [HttpPost("updateReceive")] /* done */
        public IActionResult UpdateReceive([FromForm] formReceiveData oData)
        {
            appconfig.ReadAppconfig();
            getRequestData();
            var objHreceive = Newtonsoft.Json.JsonConvert.DeserializeObject<HReceive>(oData.HData);
            var objIreceive = Newtonsoft.Json.JsonConvert.DeserializeObject<List<IReceive>>(oData.IData);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(oData.User);

            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                string strSQL = "select COUNT(RECEIVE_HID) FROM TR_RECEIVE_H WHERE VENDOR_ID = " + objHreceive.VENDOR_ID + " and SET_NO = " + objHreceive.SET_NO + " and STATUS <> 'CANCELED' and RECEIVE_HID <> " + objHreceive.RECEIVE_HID;
                string strCountResult = dbHelper.DBExecuteResult(strSQL);
                if (strCountResult != "0")
                {
                    oRes.RESULT = "0";
                    oRes.MESSAGE = "เลขที่ชุดซ้ำในระบบ";
                   // return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(oRes));
                } else {
                    if (objHreceive.STATUS == "PENDING")
                    {
                        strSQL = @"UPDATE TR_RECEIVE_H set SET_NO ='" + objHreceive.SET_NO + "', RECEIVE_DATE ='" + objHreceive.RECEIVE_DATE + "', VENDOR_ID=" + objHreceive.VENDOR_ID + ", VENDOR_DESC='" + objHreceive.VENDOR_DESC + "', NOTE='" + objHreceive.NOTE + "', UPDATE_DATE=getdate(), UPDATE_BY='" + objUser.U_NAME + "' WHERE RECEIVE_HID = " + objHreceive.RECEIVE_HID;
                        dbHelper.DBExecute(strSQL);

                        if (objHreceive.DELETED_LIST != null && objHreceive.DELETED_LIST != "")
                        {
                            strSQL = @"UPDATE TR_RECEIVE_I SET IS_DEL='Y', DELETE_DATE=getdate(), DELETE_BY='" + objUser.U_NAME + "' WHERE RECEIVE_IID in (" + objHreceive.DELETED_LIST.Substring(1) + ")";
                            dbHelper.DBExecute(strSQL);
                        }

                        for (int i = 0; i < objIreceive.Count; i++)
                        {
                            if (objIreceive[i].RECEIVE_IID == "")
                            {
                                strSQL = "INSERT INTO TR_RECEIVE_I (RECEIVE_HID, PRODUCT_ID, PRODUCT_CODE, QTY, UNIT_PRICE, WH_ID, CREATE_DATE, CREATE_BY) " +
                                         " VALUES(" + objIreceive[i].RECEIVE_HID + "," + objIreceive[i].PRODUCT_ID + ",'" + objIreceive[i].PRODUCT_CODE + "'," + objIreceive[i].QTY + "," + objIreceive[i].UNIT_PRICE + "," + objIreceive[i].WH_ID + ",getdate(),'" + objUser.U_NAME + "')";
                                dbHelper.DBExecute(strSQL);
                            }else{
                                strSQL = "UPDATE TR_RECEIVE_I set PRODUCT_ID = " + objIreceive[i].PRODUCT_ID + ", PRODUCT_CODE = '" + objIreceive[i].PRODUCT_CODE + "', QTY = " + objIreceive[i].QTY +
                                                                  (objIreceive[i].QTY_IN != "" && objIreceive[i].QTY_IN != null ? ",QTY_IN = " + objIreceive[i].QTY_IN : "") +
                                                                  (objIreceive[i].REASON_ID != "" && objIreceive[i].REASON_ID != null ? ",REASON_ID = " + objIreceive[i].REASON_ID : "") +
                                                                  (objIreceive[i].FINE_PRICE != "" && objIreceive[i].FINE_PRICE != null ? ",FINE_PRICE = " + objIreceive[i].FINE_PRICE : "") +
                                                                  ",UNIT_PRICE=" + objIreceive[i].UNIT_PRICE + ", UPDATE_DATE = getdate(), UPDATE_BY ='" + objUser.U_NAME + "' " +
                                         "WHERE RECEIVE_IID=" + objIreceive[i].RECEIVE_IID;
                                dbHelper.DBExecute(strSQL);
                            }
                        }

                        if (objHreceive.isCONFIRMED == "1")
                        {
                            strSQL = @"UPDATE TR_RECEIVE_H set STATUS='CONFIRMED', CONFIRM_DATE=getdate(), CONFIRM_BY='" + objUser.U_NAME + "' WHERE RECEIVE_HID = " + objHreceive.RECEIVE_HID;
                            dbHelper.DBExecute(strSQL);
                            /* loop to insert into stock*/
                            for (int i = 0; i < objIreceive.Count; i++)
                            {
                                if (Convert.ToInt32(objIreceive[i].QTY_IN) > 0)
                                {                                 
                                    strSQL = @"IF EXISTS(SELECT PRODUCT_ID FROM TR_STOCK WHERE LOC_ID = (select top 1 LOC_ID from MT_LOCATION where IS_DEL IS NULL AND IS_DEFAULT = 'Y') AND PRODUCT_ID =" + objIreceive[i].PRODUCT_ID + @")            
                                            BEGIN
                                                UPDATE TR_STOCK set QTY = QTY + " + objIreceive[i].QTY_IN + @",UPDATE_DATE = getdate() where LOC_ID = (select top 1 LOC_ID from MT_LOCATION where IS_DEL IS NULL AND IS_DEFAULT = 'Y') AND PRODUCT_ID = " + objIreceive[i].PRODUCT_ID + @"
                                            END
                                            ELSE
                                            BEGIN
                                                INSERT INTO TR_STOCK(PRODUCT_ID, QTY,LOC_ID, CREATE_DATE) values(" + objIreceive[i].PRODUCT_ID + @"," + objIreceive[i].QTY_IN + @",(select top 1 LOC_ID from MT_LOCATION where IS_DEL IS NULL AND IS_DEFAULT = 'Y'), getdate())
                                            END ";
                                    dbHelper.DBExecute(strSQL);
                                }
                            }
                        }
                    }
                    else if (objHreceive.STATUS == "CONFIRMED")
                    {
                        if (objHreceive.isCONFIRMED == "0")
                        {
                            strSQL = @"UPDATE TR_RECEIVE_H set STATUS='PENDING' WHERE RECEIVE_HID = " + objHreceive.RECEIVE_HID;
                            dbHelper.DBExecute(strSQL);

                            for (int i = 0; i < objIreceive.Count; i++)
                            {
                                if (Convert.ToInt32(objIreceive[i].QTY_IN) > 0)
                                {
                                    strSQL = @"UPDATE TR_STOCK set QTY = QTY - " + objIreceive[i].QTY_IN + @",UPDATE_DATE = getdate() where PRODUCT_ID = " + objIreceive[i].PRODUCT_ID;
                                    dbHelper.DBExecute(strSQL);
                                }
                            }
                        }
                    }
                    oRes.RESULT = "1";
                    oRes.MESSAGE = "บันทึกข้อมูลสำเร็จ";
                   // return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(oRes));
                }
                string strReturn = JsonConvert.SerializeObject(new { response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }else{
                oRes.RESULT = "E";
                oRes.MESSAGE = "Invalid Token";
                return BadRequest(oRes);
            }

        }

        [HttpPost("cancelReceive")] /* done */
        public IActionResult CancelReceive([FromForm] formReceiveData oData)
        {
            appconfig.ReadAppconfig();
            getRequestData();           
            var objHreceive = Newtonsoft.Json.JsonConvert.DeserializeObject<HReceive>(oData.HData);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(oData.User);
          
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                string strSQL = @"UPDATE TR_RECEIVE_H set STATUS='CANCELED', CANCEL_DATE=getdate(), CANCEL_BY='" + objUser.U_NAME + "',CANCEL_REASON='" + objHreceive.CANCEL_REASON + "'  WHERE RECEIVE_HID = " + objHreceive.RECEIVE_HID;
                dbHelper.DBExecute(strSQL);
                oRes.RESULT = "1";
                oRes.MESSAGE = "ยกเลิกเอกสารเลขที่ "+ objHreceive.DOC_NO + " สำเร็จ";
                string strReturn = JsonConvert.SerializeObject(new {response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }
            else
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = "Invalid Token";
                return BadRequest(oRes);
            }          

        }

        [HttpGet("getReceiveList")] /* done */
        public IActionResult GetReceiveList()
        {
            appconfig.ReadAppconfig();
            getRequestData();
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                string strQuery = @"select	RECEIVE_HID, DOC_NO, SET_NO, FORMAT(DOC_DATE, 'dd/MM/yyyy') as DOC_DATE, RECEIVE_DATE, TR_RECEIVE_H.VENDOR_ID, MT_VENDOR.VENDOR_DESC, NOTE, STATUS, 
		                                FORMAT(TR_RECEIVE_H.CREATE_DATE, 'dd/MM/yyyy') as CREATE_DATE, TR_RECEIVE_H.CREATE_BY, TR_RECEIVE_H.UPDATE_DATE, TR_RECEIVE_H.UPDATE_BY
                                FROM TR_RECEIVE_H
                                LEFT OUTER JOIN MT_VENDOR on TR_RECEIVE_H.VENDOR_ID = MT_VENDOR.VENDOR_ID
                                order by RECEIVE_HID desc  ";

                oRes.RESULT = "1";
                oRes.MESSAGE = "Success ";
                DataTable resTable = new DataTable();
                resTable = dbHelper.DBGetData(strQuery).Tables[0];
                string strReturn = JsonConvert.SerializeObject(new { data = resTable, response = oRes, token= auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue),appconfig.ActsTokenPeriod)}, Newtonsoft.Json.Formatting.Indented);
                return Ok(strReturn);
            }
            else
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = "Invalid Token";
                return BadRequest(oRes);
            }           
        }

        [HttpGet("getReceiveDetails")] /* done */
        public IActionResult GetReceiveDetails(string RECEIVE_HID)
        {
            appconfig.ReadAppconfig();
            getRequestData();
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                string strQuery = @"select  TR_RECEIVE_I.RECEIVE_IID, RECEIVE_HID, TR_RECEIVE_I.PRODUCT_ID, TR_RECEIVE_I.PRODUCT_CODE, QTY,QTY_IN,
                                        (select QTY FROM TR_STOCK WHERE TR_STOCK.PRODUCT_ID = TR_RECEIVE_I.PRODUCT_ID) as QTY_STOCK,UNIT_PRICE, FINE_PRICE,
		                                MT_PRODUCT.PRODUCT_DESC,UOM_DESC, TR_RECEIVE_I.REASON_ID , MT_REASON.REASON_DESC,TR_ATTACHMENT.ATTM_ID,ISNULL(TR_ATTACHMENT.FILE_NAME,'') as FILE_NAME,
                                        ISNULL(MT_PRODUCT.IMG1,'no-image-available.png') as IMG1,ISNULL(MT_PRODUCT.IMG2,'no-image-available.png') as IMG2,ISNULL(MT_PRODUCT.IMG3,'no-image-available.png') as IMG3
                                from TR_RECEIVE_I
                                left outer join MT_PRODUCT on TR_RECEIVE_I.PRODUCT_ID = MT_PRODUCT.PRODUCT_ID
                                left outer join MT_UOM on MT_PRODUCT.UOM_ID = MT_UOM.UOM_ID
                                left outer join MT_REASON on TR_RECEIVE_I.REASON_ID = MT_REASON.REASON_ID
                                left outer join TR_ATTACHMENT on TR_RECEIVE_I.RECEIVE_IID = TR_ATTACHMENT.RECEIVE_IID and TR_ATTACHMENT.IS_DEL is null
                                where TR_RECEIVE_I.IS_DEL is null and RECEIVE_HID = " + RECEIVE_HID + @"
                                order by RECEIVE_IID ";
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

        [HttpGet("getRptReceive")] /* done */
        public IActionResult GetRptReceive(string VENDOR_ID)
        {
            appconfig.ReadAppconfig();
            getRequestData();
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                string strQuery = @"select  TR_RECEIVE_H.VENDOR_ID, MT_VENDOR.VENDOR_DESC, TR_RECEIVE_H.SET_NO,TR_RECEIVE_I.PRODUCT_CODE,MT_PRODUCT.PRODUCT_DESC,QTY,QTY_IN,
		                                UNIT_PRICE,FINE_PRICE, MT_REASON.REASON_DESC
                                from    TR_RECEIVE_I
                                left outer join TR_RECEIVE_H on TR_RECEIVE_H.RECEIVE_HID = TR_RECEIVE_I.RECEIVE_HID
                                left outer join MT_PRODUCT on TR_RECEIVE_I.PRODUCT_ID = MT_PRODUCT.PRODUCT_ID
                                left outer join MT_REASON on TR_RECEIVE_I.REASON_ID = MT_REASON.REASON_ID
                                left outer join MT_VENDOR on TR_RECEIVE_H.VENDOR_ID = MT_VENDOR.VENDOR_ID
                                where  TR_RECEIVE_H.STATUS = 'CONFIRMED' AND TR_RECEIVE_I.IS_DEL is null
                                AND TR_RECEIVE_H.VENDOR_ID = " + VENDOR_ID + @"
                                order by SET_NO,TR_RECEIVE_I.RECEIVE_IID";

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

        [HttpPost("addProductIMG")]/* done */
        public IActionResult AddProductIMG([FromForm] formProductData oData)
        {
            appconfig.ReadAppconfig();
            getRequestData();
            if (auth.checkValidToken(strAuthorization, appconfig.ActsSecretKey))
            {
                var objProduct = Newtonsoft.Json.JsonConvert.DeserializeObject<Product>(oData.Data);
                var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(oData.User);
                
                if (oData.IMGFile1 != null && oData.IMGFile1.Length > 0)
                {
                    dbHelper.DBExecute("UPDATE MT_PRODUCT set IMG1='" + objProduct.PRODUCT_ID + "-1-" + oData.IMGFile1.FileName + "',update_date = getdate(),update_by ='" + objUser.U_NAME + "' where PRODUCT_ID =" + objProduct.PRODUCT_ID);
                   
                    try
                    {
                        using (FileStream fileStream = System.IO.File.Create(appconfig.IMGAssetUpload + objProduct.PRODUCT_ID + "-1-" + oData.IMGFile1.FileName))
                        {
                            oData.IMGFile1.CopyTo(fileStream);
                            fileStream.Flush();
                            oRes.RESULT = "1";
                            oRes.MESSAGE = "บันทึกรูปสินค้า(1)สำเร็จ";
                        }
                    }
                    catch (Exception ex)
                    { 
                        oRes.RESULT = "0";
                        oRes.MESSAGE = ex.Message;
                    }
                }
                else if (oData.IMGFile2 != null && oData.IMGFile2.Length > 0)
                {
                    dbHelper.DBExecute("UPDATE MT_PRODUCT set IMG2='" + objProduct.PRODUCT_ID + "-2-" + oData.IMGFile2.FileName + "',update_date = getdate(),update_by ='" + objUser.U_NAME + "' where PRODUCT_ID =" + objProduct.PRODUCT_ID);
                    
                    try
                    {
                        using (FileStream fileStream = System.IO.File.Create(appconfig.IMGAssetUpload + objProduct.PRODUCT_ID + "-2-" + oData.IMGFile2.FileName))
                        {
                            oData.IMGFile2.CopyTo(fileStream);
                            fileStream.Flush();
                            oRes.RESULT = "1";
                            oRes.MESSAGE = "บันทึกรูปสินค้า(2)สำเร็จ";
                        }
                    }
                    catch (Exception ex)
                    {
                        oRes.RESULT = "0";
                        oRes.MESSAGE = ex.Message;
                    }
                }
                else if (oData.IMGFile3 != null && oData.IMGFile3.Length > 0)
                {
                    dbHelper.DBExecute("UPDATE MT_PRODUCT set IMG3='" + objProduct.PRODUCT_ID + "-3-" + oData.IMGFile3.FileName + "',update_date = getdate(),update_by ='" + objUser.U_NAME + "' where PRODUCT_ID =" + objProduct.PRODUCT_ID);
                    
                    try
                    {
                        using (FileStream fileStream = System.IO.File.Create(appconfig.IMGAssetUpload + objProduct.PRODUCT_ID + "-3-" + oData.IMGFile3.FileName))
                        {
                            oData.IMGFile3.CopyTo(fileStream);
                            fileStream.Flush();
                            oRes.RESULT = "1";
                            oRes.MESSAGE = "บันทึกรูปสินค้า(3)สำเร็จ";
                        }
                    }
                    catch (Exception ex)
                    {
                        oRes.RESULT = "0";
                        oRes.MESSAGE = ex.Message;
                    }
                }
                string strReturn = JsonConvert.SerializeObject(new {response = oRes, token = auth.GenerateToken(strU_ID, strU_ROLE, strU_NAME, Convert.ToInt32(appconfig.ActsTokenValue), appconfig.ActsTokenPeriod) }, Newtonsoft.Json.Formatting.Indented);
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

        /* End Receive */

    }
}
