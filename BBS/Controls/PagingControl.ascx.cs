using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BBS.Controls
{
    /// <summary>
    /// ページ番号を指定して移動するユーザコントロール
    /// </summary>
    public partial class PagingControl : System.Web.UI.UserControl
    {
        /// <summary>
        /// 最後のページ番号
        /// </summary>
        public int LastPage { set; get; }

        /// <summary>
        /// 現在のページ番号
        /// </summary>
        public int CurrentPage { set; get; }

        /// <summary>
        /// ページ指定のパラメータを除くURL
        /// </summary>
        public string BaseUrl { set; get; }

        /// <summary>
        /// ページ数を指定するパラメータの名前
        /// デフォルトではpageが使用される
        /// </summary>
        public string PageParameterName {
            set { _pageParameterName = value; }
            get { return _pageParameterName; }
        }
        private string _pageParameterName = "page";

        protected PagingControl() { }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.DataBind();
        }

        /// <summary>
        /// 指定したページ番号のページのURLを取得
        /// </summary>
        /// <param name="pageNumber">ページ番号</param>
        /// <returns>URLの文字列</returns>
        protected string GetPageUrl(int pageNumber)
        {
            Uri url;

            try
            {
                // 完全なURLの場合
                url = new Uri(BaseUrl);
            }
            catch (UriFormatException)
            {
                // BaseUrlがhttp://から始まる完全なものでない場合
                url = new Uri(Request.Url, BaseUrl);
            }

            // クエリが無い場合は?、クエリがある場合は&で連結してpageパラメータを追加して相対URLの文字列を返す
            return url.PathAndQuery +
                (url.Query == "" ? "?" : "&") +
                PageParameterName + "=" + pageNumber;
        }

        /// <summary>
        /// 現在のページより前のページ番号の配列を取得
        /// </summary>
        /// <returns></returns>
        protected int[] GetPrevPageNumbers()
        {
            return Enumerable.Range(CurrentPage - 3, 3)
                .Where(x => x > 0)
                .Where(x => x <= CurrentPage)
                .ToArray();
        }

        /// <summary>
        /// 現在のページより後のページ番号の配列を取得
        /// </summary>
        /// <returns></returns>
        protected int[] GetNextPageNumbers()
        {
            return Enumerable.Range(CurrentPage + 1, 3)
                .Where(x => x <= LastPage)
                .ToArray();
        }
    }
}