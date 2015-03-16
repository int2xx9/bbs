using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BBS.Controls
{
    /// <summary>
    /// BootstrapのAlert用ユーザコントロール
    /// </summary>
    [ParseChildren(true, "Message")]
    public partial class BootstrapAlertControl : System.Web.UI.UserControl
    {
        /// <summary>
        /// アラートの種類
        /// </summary>
        public enum AlertType
        {
            Success,
            Info,
            Warning,
            Danger,
        }

        /// <summary>
        /// 表示するアラートの種類
        /// </summary>
        public AlertType? Type { set; get; }

        /// <summary>
        /// 表示する内容
        /// </summary>
        public string Message { set; get; }
        
        public BootstrapAlertControl(AlertType? type = null, string message = null)
        {
            this.Type = type;
            this.Message = message;
        }

        /// <summary>
        /// アラートの種類を文字列にする
        /// </summary>
        /// <returns>アラートの種類を表す文字列</returns>
        protected string GetAlertTypeString()
        {
            switch (Type)
            {
                case AlertType.Success: return "success";
                case AlertType.Info: return "info";
                case AlertType.Warning: return "warning";
                case AlertType.Danger: return "danger";
            }
            return "";
        }
    }
}