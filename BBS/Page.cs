using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Util;

namespace BBS
{
    public class Page : System.Web.UI.Page
    {
        /// <summary>
        /// 管理者としてログインしているかどうか
        /// </summary>
        /// <returns>管理者としてログインしている場合true、ログインしていない場合false</returns>
        protected bool IsLoginedAsAdmin()
        {
            return (bool?)Session["admin_logined"] ?? false;
        }

        /// <summary>
        /// クッキーに入っているフォームに入力された名前
        /// </summary>
        protected string FormName
        {
            set
            {
                Response.Cookies["formdata"]["name"] = HttpUtility.UrlEncode(value);
            }
            get
            {
                try
                {
                    return HttpUtility.UrlDecode(Request.Cookies["formdata"]["name"]);
                }
                catch (NullReferenceException)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// クッキーに入っているフォームに入力されたメールアドレス
        /// </summary>
        protected string FormEmail
        {
            set
            {
                Response.Cookies["formdata"]["email"] = HttpUtility.UrlEncode(value);
            }
            get
            {
                try
                {
                    return HttpUtility.UrlDecode(Request.Cookies["formdata"]["email"]);
                }
                catch (NullReferenceException)
                {
                    return null;
                }
            }
        }
    }
}