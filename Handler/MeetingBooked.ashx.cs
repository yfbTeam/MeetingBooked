using Model;
using SMSUtility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace Handler
{
    /// <summary>
    /// MeetingBooked 的摘要说明
    /// </summary>
    public class MeetingBooked : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            string action = context.Request.QueryString["action"];
            if (!string.IsNullOrEmpty(action))
            {
                switch (action)
                {
                    case "getList": getList(context); break;
                    case "SetList": SetList(context); break;
                    case "GetSeeMeeting": GetSeeMeeting(context); break;
                    case "UpMeetingBooked": UpMeetingBooked(context); break;
                    case "getUserInfo": getUserInfo(context); break;
                    case "GetSeeMeetings": GetSeeMeetings(context); break;
                    case "getMeetingBooked": getMeetingBooked(context); break;
                    case "qxBooked": qxBooked(context); break;
                    case "InUserLog": InUserLog(context); break;
                }
            }
        }


        /// <summary>
        /// 查询会议室和时间段数据用于绑定
        /// </summary>
        /// <param name="context"></param>
        public void getList(HttpContext context)
        {
            try
            {

                string callback = context.Request["jsoncallback"];
                DataSet ds = new Bll.MeetingBll().GetList();

                //获取近3天的日期
                DataTable dt = new DataTable();
                dt.Columns.Add("datetime");
                dt.Columns.Add("date");

                int obj = Convert.ToInt32(DateTime.Now.Day.ToString());
                DataRow dr = dt.NewRow();
                for (int i = 0; i < 6; i++)
                {
                    dr = dt.NewRow();
                    dr["datetime"] = DateTime.Now.AddDays(i).ToString("yyyy-MM-dd");
                    dr["date"] = GetWeek(DateTime.Now.AddDays(i).DayOfWeek.ToString()) + "(" + DateTime.Now.AddDays(i).Month.ToString() + "月" + DateTime.Now.AddDays(i).Day.ToString() + "日)";
                    dt.Rows.Add(dr);
                }
                ds.Tables.Add(dt);

                string MeetingList = DataTableToJson(ds.Tables[0]);
                string TimeSectionList = DataTableToJson(ds.Tables[1]);
                string date = DataTableToJson(ds.Tables[2]);
                List<string> list = new List<string>();
                list.Add(MeetingList);
                list.Add(TimeSectionList);
                list.Add(date);

                JsonModel jsonModel = new JsonModel()
                {
                    Data = list,
                    Msg = "",
                    Status = "ok",
                    BackUrl = ""
                };
                JavaScriptSerializer jss = new JavaScriptSerializer();
                //context.Response.Write(callback + "({\"MeetingList\":\"" + MeetingList + "\",\"TimeSectionList\":\"" + TimeSectionList + "\"})");

                context.Response.Write(callback + "({\"MeetingList\":" + jss.Serialize(jsonModel) + "})");
                context.Response.End();
            }
            catch (Exception ex)
            {

                LogHelper.Debug(ex.Message);
            }
        }

        /// <summary>
        /// 提交会议预定申请
        /// </summary>
        /// <param name="context"></param>
        public void SetList(HttpContext context)
        {
            try
            {
                string callback = context.Request["jsoncallback"].ToString();
                string MeetingTitle = context.Request["MeetingTitle"].ToString();
                string MeetingID = context.Request["MeetingID"].ToString(); ;
                string TimeSectionID = context.Request["TimeSectionID"].ToString();
                string Name = context.Request["Name"].ToString();
                string Phone = context.Request["Phone"].ToString();
                string BookedDate = context.Request["BookedDate"].ToString();
                string Remark = context.Request["Remark"].ToString();
                string str = new Bll.MeetingBll().setList(MeetingTitle, MeetingID, TimeSectionID, BookedDate, Remark, Name, Phone);
                JavaScriptSerializer jss = new JavaScriptSerializer();
                context.Response.Write(callback + "({\"result\":" + jss.Serialize(str) + "})");
                context.Response.End();

            }
            catch (Exception ex)
            {

                LogHelper.Debug(ex.Message);
            }

        }


        /// <summary>
        /// 管理员获取预约信息
        /// </summary>
        /// <param name="context"></param>
        public void GetSeeMeeting(HttpContext context)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            JsonModel jsonModel = null;
            string callback = context.Request["jsoncallback"].ToString();
            try
            {

                string id = context.Request["id"].ToString();
                string Name = context.Request["Name"].ToString();
                string Phone = context.Request["Phone"].ToString();
                jsonModel = new Bll.MeetingBll().GetSeeMeeting(id, Name, Phone);
                context.Response.Write(callback + "({\"result\":" + jss.Serialize(jsonModel) + "})");
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                jsonModel = new JsonModel()
                {
                    Msg = ex.Message,
                    errNum = -1,
                    Data = null,
                    Status = "error"
                };
                context.Response.Write(callback + "({\"result\":" + jss.Serialize(jsonModel) + "})");
            }
        }


        /// <summary>
        /// 普通用户获取信息
        /// </summary>
        /// <param name="context"></param>
        public void GetSeeMeetings(HttpContext context)
        {
            string callback = context.Request["jsoncallback"].ToString();
            string id = context.Request["id"].ToString();
            string Name = context.Request["Name"].ToString();
            string Phone = context.Request["Phone"].ToString();
            DataTable dt = new Bll.MeetingBll().GetSeeMeetings(id, Name, Phone);
            string str = DataTableToJson(dt);
            List<string> list = new List<string>();
            list.Add(str);
            JsonModel jsonModel = new JsonModel()
            {
                Data = list,
                Msg = "",
                Status = "",
                BackUrl = ""
            };
            JavaScriptSerializer jss = new JavaScriptSerializer();
            context.Response.Write(callback + "({\"result\":" + jss.Serialize(jsonModel) + "})");
            context.Response.End();


        }


        /// <summary>
        /// 获取当前会议室是否被占用
        /// </summary>
        /// <param name="context"></param>
        public void getMeetingBooked(HttpContext context)
        {
            try
            {
                string callback = context.Request["jsoncallback"].ToString();
                string BookedDate = context.Request["BookedDate"].ToString();
                string MeetingID = context.Request["MeetingID"].ToString();
                DataTable dt = new Bll.MeetingBll().getMeetingBooked(BookedDate, MeetingID);
                string str = DataTableToJson(dt);
                List<string> list = new List<string>();
                list.Add(str);
                JsonModel jsonModel = new JsonModel()
                {
                    Data = list,
                    Msg = "",
                    Status = "",
                    BackUrl = ""
                };
                JavaScriptSerializer jss = new JavaScriptSerializer();
                context.Response.Write(callback + "({\"result\":" + jss.Serialize(jsonModel) + "})");
                context.Response.End();

            }
            catch (Exception ex)
            {

                LogHelper.Debug(ex.Message);
            }


        }



        /// <summary>
        /// 审核预约信息
        /// </summary>
        /// <param name="context"></param>
        public void UpMeetingBooked(HttpContext context)
        {
            try
            {
                string callback = context.Request["jsoncallback"].ToString();
                string id = context.Request["id"].ToString();
                string status = context.Request["status"].ToString() == "2" ? "1" : context.Request["status"].ToString();
                string BookedRemark = context.Request["BookedRemark"].ToString();
                DataTable dt = new Bll.MeetingBll().UpMeetingBooked(id, status, BookedRemark);
                string str = dt.Rows[0][0].ToString();
                JavaScriptSerializer jss = new JavaScriptSerializer();
                context.Response.Write(callback + "({\"result\":" + jss.Serialize(str) + "})");
                context.Response.End();
            }
            catch (Exception ex)
            {

                LogHelper.Debug(ex.Message);
            }
        }


        /// <summary>
        /// 取消预约
        /// </summary>
        /// <param name="context"></param>
        public void qxBooked(HttpContext context)
        {
            try
            {
                string callback = context.Request["jsoncallback"].ToString();
                string id = context.Request["id"].ToString();

                DataTable dt = new Bll.MeetingBll().qxBooked(id);
                string str = dt.Rows[0][0].ToString();
                JavaScriptSerializer jss = new JavaScriptSerializer();
                context.Response.Write(callback + "({\"result\":" + jss.Serialize(str) + "})");
                context.Response.End();
            }
            catch (Exception ex)
            {

                LogHelper.Debug(ex.Message);
            }


        }


        /// <summary>
        /// 钉钉登陆判断账号所属
        /// </summary>
        /// <param name="context"></param>
        public void getUserInfo(HttpContext context)
        {
            try
            {
                string callback = context.Request["jsoncallback"].ToString();
                string Name = context.Request["Name"].ToString();
                string Phone = context.Request["Phone"].ToString();
                DataTable dt = new Bll.MeetingBll().getUserInfo(Name, Phone);
                string str = "";
                if (dt == null)
                {
                    str = "NO";
                }
                else
                {
                    if (dt.Rows.Count > 0)
                    {
                        str = "OK";
                    }
                    else
                    {
                        str = "NO";

                    }
                }
                JavaScriptSerializer jss = new JavaScriptSerializer();
                context.Response.Write(callback + "({\"result\":" + jss.Serialize(str) + "})");
                context.Response.End();
            }
            catch (Exception ex)
            {

                LogHelper.Debug(ex.Message);
            }

        }

        /// <summary>
        /// 新用户登陆时候自动注册账号
        /// </summary>
        /// <param name="context"></param>
        public void InUserLog(HttpContext context)
        {
            try
            {
                string callback = context.Request["jsoncallback"].ToString();
                string Name = context.Request["Name"].ToString();
                string Phone = context.Request["Phone"].ToString();
                DataTable dt = new Bll.MeetingBll().InUserLog(Name, Phone);
                string str = dt.Rows[0][0].ToString();

                JavaScriptSerializer jss = new JavaScriptSerializer();
                context.Response.Write(callback + "({\"result\":" + jss.Serialize(str) + "})");
                context.Response.End();
            }
            catch (Exception ex)
            {

                LogHelper.Debug(ex.Message);
            }

        }

        public string DataTableToJson(DataTable dt)
        {
            if (dt == null) return string.Empty;
            StringBuilder sb = new StringBuilder();
            sb.Append("{\"");
            sb.Append(dt.TableName);
            sb.Append("\":[");
            foreach (DataRow r in dt.Rows)
            {
                sb.Append("{");
                foreach (DataColumn c in dt.Columns)
                {
                    sb.Append("\"");
                    sb.Append(c.ColumnName);
                    sb.Append("\":\"");
                    sb.Append(r[c].ToString());
                    sb.Append("\",");
                }
                sb.Remove(sb.Length - 1, 1);
                sb.Append("},");
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append("]}");
            return sb.ToString();
        }

        public string GetWeek(string dt)
        {
            string week = "";
            switch (dt)
            {
                case "Monday":
                    week = "周一";
                    break;
                case "Tuesday":
                    week = "周二";
                    break;
                case "Wednesday":
                    week = "周三";
                    break;
                case "Thursday":
                    week = "周四";
                    break;
                case "Friday":
                    week = "周五";
                    break;
                case "Saturday":
                    week = "周六";
                    break;
                case "Sunday":
                    week = "周日";
                    break;
            }
            return week;
        }
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}