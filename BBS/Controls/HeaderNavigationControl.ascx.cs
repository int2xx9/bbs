using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BBS.Controls
{
    public partial class HeaderNavigationControl : System.Web.UI.UserControl
    {
        /// <summary>
        /// 管理者としてログインしているかどうか
        /// trueの場合ログイン中、falseの場合未ログインとして扱い、
        /// nullの場合は管理者ログインリンクを表示しない
        /// </summary>
        public bool? LoginedAsAdmin { set; get; }

        /// <summary>
        /// Keyにリンク先、Valueに表示するテキストを指定したナビゲーションリンクのリスト
        /// </summary>
        public List<KeyValuePair<string, string>> NavigationLinks {
            get { return _navigationLinks; }
        }
        private List<KeyValuePair<string, string>> _navigationLinks = new List<KeyValuePair<string, string>>();

        /// <summary>
        /// 検索ボックスを表示するかしないか
        /// </summary>
        public bool SearchBoxEnabled = true;

        public HeaderNavigationControl() { }

        protected void Page_Load(object sender, EventArgs e)
        {
            DataBind();
        }
    }
}