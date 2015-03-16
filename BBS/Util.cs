using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BBS
{
    public class Util
    {
        /// <summary>
        /// 最後のページ番号を計算する
        /// </summary>
        /// <param name="count">全項目数</param>
        /// <param name="perPage">ページあたりの項目数</param>
        /// <returns>最後のページ番号</returns>
        static public int CalcLastPageNumber(int count, int perPage)
        {
            return count / perPage + (count % perPage > 0 ? 1 : 0);
        }
    }

    public static class ExtensionUtil
    {
        /// <summary>
        /// 改行と空白を含め文字列をHTMLで表示できるようにする
        /// </summary>
        /// <param name="str">対象の文字列</param>
        /// <returns>エスケープ後の文字列</returns>
        public static string ToHtml(this string str)
        {
            return System.Web.HttpUtility.HtmlEncode(str)
                .Replace(" ", "&nbsp;")
                .Replace("\n", "<br>\n");
        }
    }
}
