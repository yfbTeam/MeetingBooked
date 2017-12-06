using Bll;
using SMSUtility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;

namespace Handler
{
    /// <summary>
    /// MeetingBookedWeb 的摘要说明
    /// </summary>
    public class MeetingBookedWeb : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            string action = context.Request.QueryString["action"];
            if (!string.IsNullOrEmpty(action))
            {
                switch (action)
                {
                    case "Login": Login(context); break;
                    case "GetLeftNavigationMenu": GetLeftNavigationMenu(context); break;
                    case "EditPassword": EditPassword(context); break;
                    case "GetMeeting": GetMeeting(context); break;
                    case "SetMeeting": SetMeeting(context); break;
                    case "BindMeeting": BindMeeting(context); break;
                    case "InMeeting": InMeeting(context); break;
                    case "GetTimeSection": GetTimeSection(context); break;
                    case "SetTimeSection": SetTimeSection(context); break;
                    case "BindTimeSection": BindTimeSection(context); break;
                    case "InTimeSection": InTimeSection(context); break;
                    case "GetUserInfo": GetUserInfo(context); break;
                    case "SetUserInfoIsDelete": SetUserInfoIsDelete(context); break;
                    case "BindUserInfo": BindUserInfo(context); break;
                    case "InUserInfo": InUserInfo(context); break;
                    case "WhatBooked": WhatBooked(context); break;
                }
            }
        }





        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="context"></param>
        public void Login(HttpContext context)
        {
            string callback = context.Request["jsoncallback"];
            try
            {
                BLLCommon bll_com = new BLLCommon();
                
                string loginName = context.Request["loginName"];
                string passWord = bll_com.Md5Encrypting(context.Request["passWord"]);
                //序列化
                System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                DataTable dt = new Bll.MeetingWebBll().Login(loginName, passWord);
                string result = "";
                if (dt != null && dt.Rows.Count > 0)
                {
                    if (string.IsNullOrWhiteSpace(Convert.ToString(dt.Rows[0]["RoleID"])) || Convert.ToString(dt.Rows[0]["RoleID"]) == "2")
                    {
                        context.Response.Write(callback + "({\"result\":'" + result + "',\"msg\":\"noqx\"})");
                    }
                    else
                    {
                        result = dt.Rows[0]["id"].ToString() + "," + dt.Rows[0]["Name"].ToString() + "," + dt.Rows[0]["IDCard"].ToString() + "," + dt.Rows[0]["Phone"].ToString() + "," + dt.Rows[0]["RoleID"].ToString() + ","
                            + dt.Rows[0]["PassWord"].ToString() + "," + dt.Rows[0]["LoginName"].ToString();
                        context.Response.Write(callback + "({\"result\":'" + result + "',\"msg\":\"ok\"})");
                    }
                }
                else
                {
                    context.Response.Write(callback + "({\"result\":'" + result + "',\"msg\":\"null\"})");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                context.Response.Write(callback + "({\"result\":\"\",\"msg\":\"error\"})");
            }
            
            //输出Json
           
        }


        /// <summary>
        /// 绑定菜单
        /// </summary>
        /// <param name="context"></param>
        public void GetLeftNavigationMenu(HttpContext context)
        {
            string callback = context.Request["jsoncallback"];
            string Roleid = context.Request["Roleid"];
            //序列化
            System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            DataTable dt = new Bll.MeetingWebBll().GetMenuInfo(Roleid);
            StringBuilder orgJson = new StringBuilder();
            DataRow[] parMenu = dt.Select("Pid=0");
            for (int i = 0; i < parMenu.Count(); i++)
            {
                orgJson.Append("<li>");
                orgJson.Append("<a class='menuclick' href='#'><i class='" + parMenu[i]["iconClass"] + "'></i>" + parMenu[i]["Name"] + "<span class='iconfont icon-icoxiala'></span></a>");
                DataRow[] subMenu = dt.Select(" Pid=" + parMenu[i]["Id"]);
                orgJson.Append("<ul class='submenu' style='display:none;'>");
                for (int j = 0; j < subMenu.Count(); j++)
                {
                    orgJson.Append("<li><a href='javascript:void(0);' data-src='" + subMenu[j]["Url"] + "'>" + subMenu[j]["Name"] + "</a></li>");
                }
                orgJson.Append("</ul>");
                orgJson.Append("</li>");
            }
            //输出Json
            context.Response.Write(callback + "({\"result\":\"" + orgJson.ToString() + "\"})");
        }


        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="context"></param>
        public void EditPassword(HttpContext context)
        {
            BLLCommon bll_com = new BLLCommon();
            string callback = context.Request["jsoncallback"];
            string oldpwd = bll_com.Md5Encrypting(context.Request["oldpwd"]);
            string id = context.Request["id"];
            string pwd = bll_com.Md5Encrypting(context.Request["pwd"]);
            DataTable dt = new Bll.MeetingWebBll().EditPassword(id, oldpwd, pwd);
            string result = "";
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    result = dt.Rows[0][0].ToString();
                }
            }
            System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            context.Response.Write(callback + "({\"result\":" + jss.Serialize(result) + "})");
        }

        /// <summary>
        /// 查询会议室
        /// </summary>
        /// <param name="context"></param>
        public void GetMeeting(HttpContext context)
        {
            string callback = context.Request["jsoncallback"];
            Hashtable ht = new Hashtable();
            if (!string.IsNullOrEmpty(context.Request["MeetingName"]))
            {
                ht.Add("MeetingName", context.Request["MeetingName"].ToString());
            }
            ht.Add("PageIndex", context.Request["PageIndex"].ToString());
            ht.Add("PageSize", context.Request["PageSize"].ToString());
            Bll.jsonModel.JsonModel Model = new Bll.MeetingWebBll().GetMeeting(ht);

            System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            context.Response.Write(callback + "({\"result\":" + jss.Serialize(Model) + "})");
        }


        /// <summary>
        /// 启用/禁用会议室
        /// </summary>
        /// <param name="context"></param>
        public void SetMeeting(HttpContext context)
        {
            string callback = context.Request["jsoncallback"];
            string id = context.Request["id"];
            string IsDelete = context.Request["IsDelete"];
            DataTable dt = new Bll.MeetingWebBll().SetMeeting(id, IsDelete);
            string result = "";
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    result = dt.Rows[0][0].ToString();
                }
            }
            System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            context.Response.Write(callback + "({\"result\":" + jss.Serialize(result) + "})");
        }


        /// <summary>
        /// 根据ID查询会议室
        /// </summary>
        /// <param name="context"></param>
        public void BindMeeting(HttpContext context)
        {
            string callback = context.Request["jsoncallback"];
            string id = context.Request["id"];
            DataTable dt = new Bll.MeetingWebBll().BindMeeting(id);
            string result = "";
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    result = dt.Rows[0][0].ToString();
                }
            }
            System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            context.Response.Write(callback + "({\"result\":" + jss.Serialize(result) + "})");
        }

        /// <summary>
        /// 修改或新增会议室
        /// </summary>
        /// <param name="context"></param>
        public void InMeeting(HttpContext context)
        {
            string callback = context.Request["jsoncallback"];
            string id = context.Request["id"];
            string MeetingName = context.Request["MeetingName"];
            string userid = context.Request["userid"];
            string Type = context.Request["Type"];
            DataTable dt = new Bll.MeetingWebBll().InMeeting(MeetingName, userid, id, Type);
            string result = "";
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    result = dt.Rows[0][0].ToString();
                }
            }
            System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            context.Response.Write(callback + "({\"result\":" + jss.Serialize(result) + "})");
        }



        /// <summary>
        /// 查询时间段
        /// </summary>
        /// <param name="context"></param>
        public void GetTimeSection(HttpContext context)
        {
            string callback = context.Request["jsoncallback"];
            Hashtable ht = new Hashtable();
            if (!string.IsNullOrEmpty(context.Request["TimeSectionName"]))
            {
                ht.Add("TimeSectionName", context.Request["TimeSectionName"].ToString());
            }
            ht.Add("PageIndex", context.Request["PageIndex"].ToString());
            ht.Add("PageSize", context.Request["PageSize"].ToString());
            Bll.jsonModel.JsonModel Model = new Bll.MeetingWebBll().GetTimeSection(ht);

            System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            context.Response.Write(callback + "({\"result\":" + jss.Serialize(Model) + "})");
        }



        /// <summary>
        /// 启用/禁用时间段
        /// </summary>
        /// <param name="context"></param>
        public void SetTimeSection(HttpContext context)
        {
            string callback = context.Request["jsoncallback"];
            string id = context.Request["id"];
            string IsDelete = context.Request["IsDelete"];
            DataTable dt = new Bll.MeetingWebBll().SetTimeSection(id, IsDelete);
            string result = "";
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    result = dt.Rows[0][0].ToString();
                }
            }
            System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            context.Response.Write(callback + "({\"result\":" + jss.Serialize(result) + "})");
        }



        /// <summary>
        /// 根据ID查询时间段
        /// </summary>
        /// <param name="context"></param>
        public void BindTimeSection(HttpContext context)
        {
            string callback = context.Request["jsoncallback"];
            string id = context.Request["id"];
            DataTable dt = new Bll.MeetingWebBll().BindTimeSection(id);
            string result = "";
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    result = dt.Rows[0][0].ToString();
                }
            }
            System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            context.Response.Write(callback + "({\"result\":" + jss.Serialize(result) + "})");
        }


        /// <summary>
        /// 修改或新增时间段
        /// </summary>
        /// <param name="context"></param>
        public void InTimeSection(HttpContext context)
        {
            string callback = context.Request["jsoncallback"];
            string id = context.Request["id"];
            string TimeSectionName = context.Request["TimeSectionName"];
            string userid = context.Request["userid"];
            string Type = context.Request["Type"];
            DataTable dt = new Bll.MeetingWebBll().InTimeSection(TimeSectionName, userid, id, Type);
            string result = "";
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    result = dt.Rows[0][0].ToString();
                }
            }
            System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            context.Response.Write(callback + "({\"result\":" + jss.Serialize(result) + "})");
        }


        /// <summary>
        ///  查询用户信息
        /// </summary>
        /// <param name="context"></param>
        public void GetUserInfo(HttpContext context)
        {
            string callback = context.Request["jsoncallback"];
            Hashtable ht = new Hashtable();
            if (!string.IsNullOrEmpty(context.Request["LoginName"]))
            {
                ht.Add("LoginName", context.Request["LoginName"].ToString());
            }
            if (!string.IsNullOrEmpty(context.Request["Name"]))
            {
                ht.Add("Name", context.Request["Name"].ToString());
            }
            if (!string.IsNullOrEmpty(context.Request["Phone"]))
            {
                ht.Add("Phone", context.Request["Phone"].ToString());
            }
            ht.Add("PageIndex", context.Request["PageIndex"].ToString());
            ht.Add("PageSize", context.Request["PageSize"].ToString());
            Bll.jsonModel.JsonModel Model = new Bll.MeetingWebBll().GetUserInfo(ht);

            System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            context.Response.Write(callback + "({\"result\":" + jss.Serialize(Model) + "})");
        }


        /// <summary>
        /// 启用/禁用账号
        /// </summary>
        /// <param name="context"></param>
        public void SetUserInfoIsDelete(HttpContext context)
        {
            string callback = context.Request["jsoncallback"];
            string id = context.Request["id"];
            string IsDelete = context.Request["IsDelete"];
            DataTable dt = new Bll.MeetingWebBll().SetUserInfoIsDelete(id, IsDelete);
            string result = "";
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    result = dt.Rows[0][0].ToString();
                }
            }
            System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            context.Response.Write(callback + "({\"result\":" + jss.Serialize(result) + "})");
        }


        /// <summary>
        /// 根据ID查询人员信息
        /// </summary>
        /// <param name="context"></param>
        public void BindUserInfo(HttpContext context)
        {
            string callback = context.Request["jsoncallback"];
            string id = context.Request["id"];
            jsonModel.JsonModel js = new Bll.MeetingWebBll().BindUserInfo(id);
           
            System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            context.Response.Write(callback + "({\"result\":" + jss.Serialize(js) + "})");
        }


        /// <summary>
        /// 修改或新增人员信息
        /// </summary>
        /// <param name="context"></param>
        public void InUserInfo(HttpContext context)
        {
            string callback = context.Request["jsoncallback"];
            string id = context.Request["id"];
            string Name = context.Request["name"];
            string LoginName = context.Request["LoginName"];
            string Phone = context.Request["Phone"];
            string IDCard = context.Request["IDCard"];
            string RoleID = context.Request["RoleID"];
            string Type = context.Request["Type"];
            DataTable dt = new Bll.MeetingWebBll().InUserInfo(Name,IDCard,Phone,RoleID,LoginName,id,Type);
            string result = "";
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    result = dt.Rows[0][0].ToString();
                }
            }
            System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            context.Response.Write(callback + "({\"result\":" + jss.Serialize(result) + "})");
            
        }


        /// <summary>
        /// 设置/取消是否需要审核
        /// </summary>
        /// <param name="context"></param>
        public void WhatBooked(HttpContext context)
        {
            string callback = context.Request["jsoncallback"];
            string index = context.Request["index"];
            DataTable dt = new Bll.MeetingWebBll().WhatBooked(index);
            string result = "";
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    result = dt.Rows[0][0].ToString();
                }
            }
            System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
            context.Response.Write(callback + "({\"result\":" + jss.Serialize(result) + "})");
            
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