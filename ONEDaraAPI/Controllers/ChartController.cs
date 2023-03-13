using Microsoft.AspNetCore.Mvc;
using System.Data;
using Newtonsoft.Json;

namespace ONEDaraAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChartController : Controller
    {
        private MyClass.DbHelper dbHelper = new ONEDaraAPI.MyClass.DbHelper();
        private MyClass.authenHelper auth = new MyClass.authenHelper();

        [HttpGet("getAllChart")]
        public IActionResult GetAllChart(string strU_ID)
        {
            string query = @"select CHART_ID,CHART_NAME,CREATE_BY, isnull(FORMAT(TR_CHART.CREATE_DATE, 'dd/MM/yyyy'),' ') as CREATE_DATE
                             from TR_CHART 
                             where is_del is null and 
                             (
                                CHART_ID in 
                                (
                                    select distinct CHART_ID 
                                    from TR_CHART_OPTIONS
                                    inner join TR_SHARE_CHART on TR_CHART_OPTIONS.OPTION_ID = TR_SHARE_CHART.OPTION_ID
                                    where (TR_SHARE_CHART.CAN_VIEW = 'Y' or TR_SHARE_CHART.CAN_EDIT = 'Y' ) and TR_SHARE_CHART.U_ID = "+ strU_ID + @"
                                ) or (select U_ID from[WebPortal].[dbo].[MT_USER] where U_NAME = TR_CHART.CREATE_BY) = " + strU_ID + @"
                             )
                             order by CHART_ID desc ";
            DataTable resTable = new DataTable();
            resTable = dbHelper.DBGetData(query).Tables[0];
            string strReturn = JsonConvert.SerializeObject(resTable, Newtonsoft.Json.Formatting.Indented);
            return Ok(strReturn);
        }

        [HttpGet("getAllOption")]
        public IActionResult GetAllOption(string strCHART_ID,string strU_ID)
        {
            string query = @"select TR_CHART.CHART_ID, TR_CHART.CHART_NAME, TR_CHART.CREATE_BY, TR_CHART_OPTIONS.OPTION_ID,
                                    isnull((select case when CAN_EDIT='Y' THEN 'E' 
											else 
											case when CAN_VIEW ='Y' THEN 'V' 
											ELSE null end end from TR_SHARE_CHART where TR_CHART_OPTIONS.OPTION_ID = TR_SHARE_CHART.OPTION_ID and U_ID = " + strU_ID + @"
									),'N') as MODE,
                                    (select U_ID from[WebPortal].[dbo].[MT_USER] where U_NAME = TR_CHART_OPTIONS.CREATE_BY) as OPTION_U_ID
                             from   TR_CHART
                             inner join TR_CHART_OPTIONS on TR_CHART.CHART_ID = TR_CHART_OPTIONS.CHART_ID
                             where TR_CHART_OPTIONS.IS_DEL is null and TR_CHART.CHART_ID = " + strCHART_ID + @"
                             order by TR_CHART_OPTIONS.OPTION_ID ";
            DataTable resTable = new DataTable();
            resTable = dbHelper.DBGetData(query).Tables[0];
            string strReturn = JsonConvert.SerializeObject(resTable, Newtonsoft.Json.Formatting.Indented);
            return Ok(strReturn);
        }

        [HttpGet("getAllChartOption")]
        public IActionResult GetAllChartOption(string strOPTION_ID)
        {
            string query = @"select	a.OPTACT_ID, a.OPTION_ID, isnull(a.ACTING,'') as ACTING, isnull(a.ACT_NAME,'') as ACT_NAME, a.ROW_NO, a.SEQ_NO,
		                            b.ACT_ID,isnull(F_NAME_TH,'') as F_NAME_TH,isnull(L_NAME_TH,'') as L_NAME_TH,isnull(N_NAME_TH,'') as N_NAME_TH,
		                            isnull(DISPLAY_NAME,'') as DISPLAY_NAME,isnull(SEX,'') as SEX,
		                            isnull([IMAGE],'') as [IMAGE]
                            from	TR_OPTIONS_ACT a
                            left outer join	MT_ACTORS b on a.ACT_ID = b.ACT_ID
                            where	a.IS_DEL is null and OPTION_ID =" + strOPTION_ID + @"
                            order by ROW_NO,OPTACT_ID";
            DataTable resTable = new DataTable();
            resTable = dbHelper.DBGetData(query).Tables[0];

            OBJ_CHART_OPTION oChartOption = new OBJ_CHART_OPTION();
            OBJ_CHART_OPTION_ROW oChartOptionRow;
            Dara.Dara oACT;
            if (resTable.Rows.Count>0)
            {
                oChartOptionRow = new OBJ_CHART_OPTION_ROW();
                oChartOptionRow.ROW_NO = (int)resTable.Rows[0]["ROW_NO"];
                string strRowNo = resTable.Rows[0]["ROW_NO"].ToString();
                
                for (int i=0;i< resTable.Rows.Count;i++)
                {
                    if(strRowNo != resTable.Rows[i]["ROW_NO"].ToString())
                    {
                        oChartOption.OPTION_ROW.Add(oChartOptionRow);
                        oChartOptionRow = new OBJ_CHART_OPTION_ROW();
                        oChartOptionRow.ROW_NO = (int)resTable.Rows[i]["ROW_NO"];
                        strRowNo = resTable.Rows[i]["ROW_NO"].ToString();
                    }
                    oACT = new Dara.Dara();
                    oACT.OPTACT_ID = resTable.Rows[i]["OPTACT_ID"].ToString();
                    oACT.ACT_ID = resTable.Rows[i]["ACT_ID"].ToString();
                    oACT.CHART_ACTING = resTable.Rows[i]["ACTING"].ToString();
                    oACT.CHART_ACT_NAME = resTable.Rows[i]["ACT_NAME"].ToString();
                    oACT.F_NAME_TH = resTable.Rows[i]["F_NAME_TH"].ToString();
                    oACT.L_NAME_TH = resTable.Rows[i]["L_NAME_TH"].ToString();
                    oACT.N_NAME_TH = resTable.Rows[i]["N_NAME_TH"].ToString();
                    oACT.DISPLAY_NAME = resTable.Rows[i]["DISPLAY_NAME"].ToString();
                    oACT.SEX = resTable.Rows[i]["SEX"].ToString();
                    oACT.IMG_PATH = resTable.Rows[i]["IMAGE"].ToString();
                    oChartOptionRow.ACT.Add(oACT);
                }
                oChartOption.OPTION_ROW.Add(oChartOptionRow);

            }

            string strReturn = JsonConvert.SerializeObject(oChartOption, Newtonsoft.Json.Formatting.Indented);
            return Ok(strReturn);
        }

        [HttpGet("getShareList")]
        public IActionResult GetShareList(string strOPTION_ID)
        {
            string query = @"select	" + strOPTION_ID + @" as OPTION_ID,a.U_ID,MT_ROLE.U_ROLE,a.U_NAME,
		                            isnull((select top 1 CAN_VIEW from TR_SHARE_CHART where TR_SHARE_CHART.U_ID = a.U_ID and OPTION_ID = " + strOPTION_ID + @"),'') as [VIEW],
		                            isnull((select top 1 CAN_EDIT from TR_SHARE_CHART where TR_SHARE_CHART.U_ID = a.U_ID and OPTION_ID = " + strOPTION_ID + @"),'') as [EDIT],
		                            isnull((select top 1 SHARE_ID from TR_SHARE_CHART where TR_SHARE_CHART.U_ID = a.U_ID and OPTION_ID = " + strOPTION_ID + @"),'0') as SHARE_ID
                            from [WebPortal].[dbo].[MT_USER] a
                            inner join MT_USER_ROLE on a.U_ID = MT_USER_ROLE.U_ID
                            inner join MT_ROLE on MT_USER_ROLE.ROLE_ID = MT_ROLE.ROLE_ID 
                            where a.IS_DEL is null";
            DataTable resTable = new DataTable();
            resTable = dbHelper.DBGetData(query).Tables[0];
            string strReturn = JsonConvert.SerializeObject(resTable, Newtonsoft.Json.Formatting.Indented);
            return Ok(strReturn);
        }

        [HttpPost("createChart")]
        public IActionResult CreateChart([FromForm] formData oData)
        {
            resExecuted oRes = new resExecuted();
            var objChart = Newtonsoft.Json.JsonConvert.DeserializeObject<OBJ_CHART>(oData.Data);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<Dara.User>(oData.User);
            MyClass.AppConfigHelper appconfig = new MyClass.AppConfigHelper();
            appconfig.ReadAppconfig();
            try
            {
                string strSQL = "INSERT INTO TR_CHART(CHART_NAME, CREATE_DATE, CREATE_BY) " +
                               "values('" + objChart.CHART_NAME + "', getdate(),'" + objUser.U_NAME + "') ";
                dbHelper.DBExecute(strSQL);
                oRes.RESULT = "1";
                oRes.MESSAGE = "เพิ่มผังสำเร็จ";
            }catch (Exception e){
                oRes.RESULT = "E";
                oRes.MESSAGE = e.Message;
            }
            return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(oRes));
        }

        [HttpPost("updateChart")]
        public IActionResult UpdateChart([FromForm] formData oData)
        {
            resExecuted oRes = new resExecuted();
            var objChart = Newtonsoft.Json.JsonConvert.DeserializeObject<OBJ_CHART>(oData.Data);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<Dara.User>(oData.User);
            MyClass.AppConfigHelper appconfig = new MyClass.AppConfigHelper();
            appconfig.ReadAppconfig();
            try{
                string strSQL = "UPDATE TR_CHART set CHART_NAME='" + objChart.CHART_NAME + "', " +
                                                     "UPDATE_DATE = getdate(), UPDATE_BY = '" + objUser.U_NAME + "'" +
                                "WHERE CHART_ID = " + objChart.CHART_ID;
                dbHelper.DBExecute(strSQL);
                oRes.RESULT = "1";
                oRes.MESSAGE = "แก้ไขผังสำเร็จ";
            }catch (Exception e){
                oRes.RESULT = "E";
                oRes.MESSAGE = e.Message;
            }
            return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(oRes));
        }

        [HttpPost("updateOPTACT")]
        public IActionResult UpdateOPTACT([FromForm] formData oData)
        {
            resExecuted oRes = new resExecuted();
            var objOPTACT = Newtonsoft.Json.JsonConvert.DeserializeObject<OPTION_ACT>(oData.Data);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<Dara.User>(oData.User);
            MyClass.AppConfigHelper appconfig = new MyClass.AppConfigHelper();
            appconfig.ReadAppconfig();
            try
            {
                string strSQL = "UPDATE TR_OPTIONS_ACT set ACT_NAME=" + (objOPTACT.CHART_ACT_NAME==""?"null":"'" + objOPTACT.CHART_ACT_NAME + "'") + ", ACTING=" + (objOPTACT.CHART_ACTING == "" ? "null" : "'" + objOPTACT.CHART_ACTING + "'") + ", " +
                                                     "UPDATE_DATE = getdate(), UPDATE_BY = '" + objUser.U_NAME + "'" +
                                "WHERE OPTACT_ID = " + objOPTACT.OPTACT_ID;
                dbHelper.DBExecute(strSQL);
                oRes.RESULT = "1";
                oRes.MESSAGE = "บันทึกข้อมูลสำเร็จ";
            }catch (Exception e){
                oRes.RESULT = "E";
                oRes.MESSAGE = e.Message;
            }
            return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(oRes));
        }

        [HttpPost("createOption")]
        public IActionResult CreateOption([FromForm] formData oData)
        {
            resExecuted oRes = new resExecuted();
            var objChart = Newtonsoft.Json.JsonConvert.DeserializeObject<OBJ_CHART>(oData.Data);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<Dara.User>(oData.User);
            MyClass.AppConfigHelper appconfig = new MyClass.AppConfigHelper();
            appconfig.ReadAppconfig();
            try{
                string strSQL = "INSERT INTO TR_CHART_OPTIONS(CHART_ID, CREATE_DATE, CREATE_BY) " +
                               "values(" + objChart.CHART_ID + ", getdate(),'" + objUser.U_NAME + "')  SELECT SCOPE_IDENTITY()";
                string strResult = dbHelper.DBExecuteResult(strSQL);

                if (strResult != "-1"){
                    strSQL = "INSERT INTO TR_SHARE_CHART (OPTION_ID, U_ID, CAN_VIEW, CAN_EDIT, CREATE_DATE, CREATE_BY) " +
                             " VALUES(" + strResult + ",(select U_ID from[WebPortal].[dbo].[MT_USER] where U_NAME = '" + objUser.U_NAME + "'),'Y','Y',getdate(),'" + objUser.U_NAME + "')";
                    dbHelper.DBExecute(strSQL);
                    oRes.RESULT = "1";
                    oRes.MESSAGE = "เพิ่มตัวเลือกใหม่สำเร็จ";
                }
                             
            }catch (Exception e){
                oRes.RESULT = "E";
                oRes.MESSAGE = e.Message;
            }
            return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(oRes));
        }

        [HttpPost("addOptionAct")]
        public IActionResult AddOptionAct([FromForm] formData oData)
        {
            resExecuted oRes = new resExecuted();
            var objOptionAct = Newtonsoft.Json.JsonConvert.DeserializeObject<OPTION_ACT>(oData.Data);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<Dara.User>(oData.User);
            MyClass.AppConfigHelper appconfig = new MyClass.AppConfigHelper();
            appconfig.ReadAppconfig();
            try
            {
                string strSQL = "";
                if (objOptionAct.ROW_NO!="-1")
                {
                     strSQL = "INSERT INTO TR_OPTIONS_ACT(OPTION_ID,ACT_ID,ROW_NO,SEQ_NO,CREATE_DATE, CREATE_BY) " +
                               "values(" + objOptionAct.OPTION_ID + ", " + objOptionAct.ACT_ID + "," + objOptionAct.ROW_NO +
                               ",(select max(SEQ_NO)+1 as newSEQ_NO from [dbo].[TR_OPTIONS_ACT] where OPTION_ID = " + objOptionAct.OPTION_ID + " and ROW_NO = "+ objOptionAct.ROW_NO + "), getdate(),'" + objUser.U_NAME + "') ";
                }
                else
                {
                     strSQL = "INSERT INTO TR_OPTIONS_ACT(OPTION_ID,ACT_ID,ROW_NO,SEQ_NO,CREATE_DATE, CREATE_BY) " +
                             "values(" + objOptionAct.OPTION_ID + ", " + objOptionAct.ACT_ID + "," + (objOptionAct.ROW_NO != "-1" ? objOptionAct.ROW_NO : "(select isnull(max(ROW_NO),0)+1 as newROW_NO from [dbo].[TR_OPTIONS_ACT] where OPTION_ID = " + objOptionAct.OPTION_ID + ")") +
                             ",1, getdate(),'" + objUser.U_NAME + "') ";
                }
                
                dbHelper.DBExecute(strSQL);
                oRes.RESULT = "1";
                oRes.MESSAGE = "เพิ่มรายการเข้าผังสำเร็จ";
            }
            catch (Exception e)
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = e.Message;
            }
            return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(oRes));
        }

        [HttpPost("deleteOptionAct")]
        public IActionResult DeleteOptionAct([FromForm] formData oData)
        {
            resExecuted oRes = new resExecuted();
            var objOptionAct = Newtonsoft.Json.JsonConvert.DeserializeObject<OPTION_ACT>(oData.Data);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<Dara.User>(oData.User);
            MyClass.AppConfigHelper appconfig = new MyClass.AppConfigHelper();
            appconfig.ReadAppconfig();
            try
            {
                string strSQL = "UPDATE TR_OPTIONS_ACT SET IS_DEL ='Y',DELETE_DATE=getDate(),DELETE_BY='" + objUser.U_NAME +"' "+
                               " where OPTACT_ID = " + objOptionAct.OPTACT_ID;
                dbHelper.DBExecute(strSQL);
                oRes.RESULT = "1";
                oRes.MESSAGE = "ลบรายการสำเร็จ";
            }
            catch (Exception e)
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = e.Message;
            }
            return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(oRes));
        }

        [HttpPost("deleteChart")]
        public IActionResult DeleteChart([FromForm] formData oData)
        {
            resExecuted oRes = new resExecuted();
            var objChar = Newtonsoft.Json.JsonConvert.DeserializeObject<OBJ_CHART>(oData.Data);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<Dara.User>(oData.User);
            MyClass.AppConfigHelper appconfig = new MyClass.AppConfigHelper();
            appconfig.ReadAppconfig();
            try
            {
                string strSQL = "UPDATE TR_CHART SET IS_DEL ='Y',DELETE_DATE=getDate(),DELETE_BY='" + objUser.U_NAME + "' " +
                               " where CHART_ID = " + objChar.CHART_ID;
                dbHelper.DBExecute(strSQL);
                oRes.RESULT = "1";
                oRes.MESSAGE = "ลบผังสำเร็จ";
            }
            catch (Exception e)
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = e.Message;
            }
            return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(oRes));
        }

        [HttpPost("deleteOptionRow")]
        public IActionResult DeleteOptionRow([FromForm] formData oData)
        {
            resExecuted oRes = new resExecuted();
            var objOptionRow = Newtonsoft.Json.JsonConvert.DeserializeObject<OPTION_ROW>(oData.Data);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<Dara.User>(oData.User);
            MyClass.AppConfigHelper appconfig = new MyClass.AppConfigHelper();
            appconfig.ReadAppconfig();
            try
            {
                string strSQL = "UPDATE TR_OPTIONS_ACT SET IS_DEL ='Y',DELETE_DATE=getDate(),DELETE_BY='" + objUser.U_NAME + "' " +
                               " where OPTION_ID = "+ objOptionRow.OPTION_ID + " and ROW_NO = " + objOptionRow.ROW_NO;
                dbHelper.DBExecute(strSQL);
                oRes.RESULT = "1";
                oRes.MESSAGE = "ลบแถวสำเร็จ";
            }
            catch (Exception e)
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = e.Message;
            }
            return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(oRes));
        }

        [HttpPost("deleteOption")]
        public IActionResult DeleteOption([FromForm] formData oData)
        {
            resExecuted oRes = new resExecuted();
            var objChartOption = Newtonsoft.Json.JsonConvert.DeserializeObject<CHART_OPTION>(oData.Data);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<Dara.User>(oData.User);
            MyClass.AppConfigHelper appconfig = new MyClass.AppConfigHelper();
            appconfig.ReadAppconfig();
            try
            {
                string strSQL = "UPDATE TR_CHART_OPTIONS SET IS_DEL ='Y',DELETE_DATE=getDate(),DELETE_BY='" + objUser.U_NAME + "' " +
                               " where OPTION_ID = " + objChartOption.OPTION_ID;
                dbHelper.DBExecute(strSQL);
                oRes.RESULT = "1";
                oRes.MESSAGE = "ลบตัวเลือกสำเร็จ";
            }
            catch (Exception e)
            {
                oRes.RESULT = "E";
                oRes.MESSAGE = e.Message;
            }
            return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(oRes));
        }

        [HttpPost("updateShareChart")]
        public IActionResult UpdateShareChart([FromForm] formShareChart oData)
        {
            resExecuted oRes = new resExecuted();
            var objShareChart = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SHARE_CHART>>(oData.SHARE_CHART);
            var objUser = Newtonsoft.Json.JsonConvert.DeserializeObject<Dara.User>(oData.User);
            MyClass.AppConfigHelper appconfig = new MyClass.AppConfigHelper();
            appconfig.ReadAppconfig();

            string strSQL = "";

            for (int i = 0; i < objShareChart.Count; i++)
            {
                if (objShareChart[i].SHARE_ID == "0")
                {
                    strSQL = "INSERT INTO TR_SHARE_CHART (OPTION_ID, U_ID, CAN_VIEW, CAN_EDIT, CREATE_DATE, CREATE_BY) " +
                             " VALUES(" + objShareChart[i].OPTION_ID + "," + objShareChart[i].U_ID + "," + (objShareChart[i].VIEW.Trim() == "" || objShareChart[i].VIEW.Trim() == "N" ? "null" : "'" + objShareChart[i].VIEW.Trim() + "'") + "," + (objShareChart[i].EDIT.Trim() == "" || objShareChart[i].EDIT.Trim() == "N" ? "null" : "'" + objShareChart[i].EDIT.Trim() + "'") + ",getdate(),'" + objUser.U_NAME + "')";
                    dbHelper.DBExecute(strSQL);
                }
                else
                {
                    strSQL = "UPDATE TR_SHARE_CHART set CAN_VIEW = " + (objShareChart[i].VIEW.Trim() == "" || objShareChart[i].VIEW.Trim() == "N" ? "null" : "'" + objShareChart[i].VIEW.Trim() + "'") + ", CAN_EDIT = " + (objShareChart[i].EDIT.Trim()=="" || objShareChart[i].EDIT.Trim() == "N" ? "null":"'" + objShareChart[i].EDIT.Trim() + "'") + "," +
                                                        "UPDATE_DATE = getdate(), UPDATE_BY ='" + objUser.U_NAME + "' " +
                                "WHERE SHARE_ID=" + objShareChart[i].SHARE_ID;
                    dbHelper.DBExecute(strSQL);
                }
            }

            oRes.RESULT = "1";
            oRes.MESSAGE = "บันทึกข้อมูลสำเร็จ";
            return Ok(Newtonsoft.Json.JsonConvert.SerializeObject(oRes)); 
        }

        public class resExecuted
        {
            public string RESULT { get; set; }
            public string MESSAGE { get; set; }
        }
        public class OBJ_CHART
        {
            public string? CHART_ID { get; set; }
            public string? CHART_NAME { get; set; }
            public List<OBJ_CHART_OPTION>? OPTION = new List<OBJ_CHART_OPTION>();
        }
        public class OBJ_CHART_OPTION
        {
            public string OPTION_ID { get; set; }
            public List<OBJ_CHART_OPTION_ROW>? OPTION_ROW = new List<OBJ_CHART_OPTION_ROW>();
        }
        public class OBJ_CHART_OPTION_ROW
        {
            public int ROW_NO { get; set; }
            public List<Dara.Dara> ACT = new List<Dara.Dara>();
        }
        public class OPTION_ACT
        {
            public string? OPTACT_ID { get; set;}
            public string? OPTION_ID { get; set; }
            public string? ACT_ID { get; set; }
            public string? CHART_ACTING { get; set; }
            public string? CHART_ACT_NAME { get; set; }
            public string? ROW_NO { get; set; }
        }
        public class CHART_OPTION
        {
            public string? CHART_ID { get; set; }
            public string? OPTION_ID { get; set; }
        }
        public class OPTION_ROW
        {
            public string? OPTION_ID { get; set; }
            public string? ROW_NO { get; set; }
        }
        public class SHARE_CHART
        {
            public string? SHARE_ID { get; set; }
            public string? OPTION_ID { get; set; }
            public string? U_ID { get; set; }
            public string? VIEW { get; set; }
            public string? EDIT { get; set; }
        }
        public sealed class formShareChart
        {
            public string SHARE_CHART { get; set; }
            public string User { get; set; }
        }
        public sealed class formData
        {
            public string Data { get; set; }
            public string User { get; set; }
        }


    }
}
