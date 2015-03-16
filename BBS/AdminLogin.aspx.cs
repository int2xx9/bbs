using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BBS.Controls;

namespace BBS
{
    public partial class AdminLogin : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // 既に管理者としてログインしていたら/に戻る
            if (IsLoginedAsAdmin()) Response.Redirect("/");

            if (Request.Form["loginButton"] == "ログイン")
            {
                // ログインボタンが押された
                if (Request.Form["adminPassword"] == Config.AdminPassword)
                {
                    // ログイン成功
                    // 管理者ログインフラグを立てる
                    Session.Add("admin_logined", true);
                    Response.Redirect("/");
                }
                else
                {
                    // ログイン失敗
                    // エラー表示
                    LoginAlerts.AddDanger("エラー: パスワードが違います");
                }
            }
        }
    }
}