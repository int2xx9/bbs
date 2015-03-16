using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BBS.Controls;

namespace BBS.Controls
{
    /// <summary>
    /// BootstrapAlertControlを複数表示するユーザコントロール
    /// </summary>
    public partial class BootstrapAlertListControl : System.Web.UI.UserControl
    {
        private Panel panel = new Panel();

        protected void Page_Load(object sender, EventArgs e)
        {
            panel.ID = ID;
            panel.ClientIDMode = System.Web.UI.ClientIDMode.Static;
            Controls.Add(panel);
        }

        /// <summary>
        /// アラートがあるかどうか
        /// </summary>
        /// <returns>アラートがある場合true、ない場合false</returns>
        public Boolean HasNoAlerts()
        {
            return panel.Controls.Count <= 0;
        }

        /// <summary>
        /// アラートを表示
        /// </summary>
        /// <param name="type">アラートの種類</param>
        /// <param name="message">アラートの内容</param>
        public void AddAlert(BootstrapAlertControl.AlertType type, string message)
        {
            var alertControl = (BootstrapAlertControl)Page.LoadControl("~/Controls/BootstrapAlertControl.ascx");
            alertControl.Type = type;
            alertControl.Message = message;
            panel.Controls.Add(alertControl);
        }

        /// <summary>
        /// type:Successのアラートを表示
        /// </summary>
        /// <param name="message">アラートの内容</param>
        public void AddSuccess(string message)
        {
            AddAlert(BootstrapAlertControl.AlertType.Success, message);
        }

        /// <summary>
        /// type:Infoのアラートを表示
        /// </summary>
        /// <param name="message">アラートの内容</param>
        public void AddInfo(string message)
        {
            AddAlert(BootstrapAlertControl.AlertType.Info, message);
        }

        /// <summary>
        /// type:Warningのアラートを表示
        /// </summary>
        /// <param name="message">アラートの内容</param>
        public void AddWarning(string message)
        {
            AddAlert(BootstrapAlertControl.AlertType.Warning, message);
        }

        /// <summary>
        /// type:Dangerのアラートを表示
        /// </summary>
        /// <param name="message">アラートの内容</param>
        public void AddDanger(string message)
        {
            AddAlert(BootstrapAlertControl.AlertType.Danger, message);
        }
    }
}