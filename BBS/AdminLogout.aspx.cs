using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BBS
{
    public partial class AdminLogout : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session.Remove("admin_logined");
            Response.Redirect("/");
        }
    }
}