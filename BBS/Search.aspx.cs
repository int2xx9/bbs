using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace BBS
{
    public partial class Search : Page
    {
        /// <summary>
        /// 検索結果
        /// </summary>
        public IQueryable<BBSData.Post> MatchedPosts { get; private set; }

        /// <summary>
        /// 選択中のページの検索結果
        /// </summary>
        public IEnumerable<BBSData.Post> PagedPosts { get; private set; }

        /// <summary>
        /// 検索キーワード
        /// </summary>
        public string Keyword { get; private set; }

        /// <summary>
        /// 選択中のページ番号
        /// </summary>
        public int PageNumber { get { return _pageNumber; } private set { _pageNumber = value; } }
        private int _pageNumber;

        /// <summary>
        /// 最後のページの番号
        /// </summary>
        protected int LastPageNumber {
            get {
                if (MatchedPosts == null) return 0;
                return Util.CalcLastPageNumber(MatchedPosts.Count(), Config.ThreadsPerPage);
            }
        }

        /// <summary>
        /// AdvanceSearchが有効かどうか
        /// </summary>
        public bool IsAdvanceSearch { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            // advanceSearchFormに何か入っていればAdvanceSearchが有効とフラグを立てる
            IsAdvanceSearch = !String.IsNullOrEmpty(Request.Params["advancedSearchForm"]);

            // ページ番号を取得
            // ページ番号が1以下の場合1にする
            int.TryParse(Request.QueryString["page"], out _pageNumber);
            if (PageNumber < 1) PageNumber = 1;

            if (IsAdvanceSearch)
            {
                AdvanceSearch();
            }
            else
            {
                GetKeyword();
                searchKeyword.Text = Keyword;

                if (String.IsNullOrEmpty(Keyword))
                {
                    SearchAlerts.AddDanger("エラー: キーワードを入力してください");
                }
                else
                {
                    var db = BBSData.OpenDatabase();
                    SearchQuery query = null;
                    Expression<Func<BBSData.Post, bool>> queryLambda = null;
                    try
                    {
                        query = SearchQuery.Parse(Keyword);
                        queryLambda = query.ToLambdaExpression();
                    }
                    catch (FormatException ex)
                    {
                        SearchAlerts.AddDanger("エラー: " + ex.Message);
                    }
                    catch (SearchQuery.UnexpectedEndOfQueryException)
                    {
                        SearchAlerts.AddDanger("エラー: 検索キーワードがおかしいです (ダブルクオーテーションを閉じていない？)");
                    }
                    if (queryLambda == null)
                    {
                        // クエリが生成できなかったのでダミーのクエリをMatchedPostsにいれておく
                        MatchedPosts = db.Posts.Where(x => false);
                    }
                    else
                    {
                        // クエリが生成できたのでそのキーワードで絞り込む
                        MatchedPosts = db.Posts.Where(queryLambda).OrderByDescending(x => x.CreatedAt);
                    }

                    PagedPosts = MatchedPosts.ToList()
                        .Skip((PageNumber - 1) * Config.ThreadsPerPage)
                        .Take(Config.ThreadsPerPage);
                }
            }
        }

        /// <summary>
        /// 指定された検索キーワードの取得
        /// </summary>
        private void GetKeyword()
        {
            switch (Request.HttpMethod)
            {
                case "POST":
                    Keyword = Request.Form["searchKeyword"];
                    if (Keyword == null)
                    {
                        // Request.Form["searchKeyword"]にキーワードが入っていなければ、HeaderNavigationControlを使用しており
                        // input要素のnameがheadnav$searchKeywordみたいな名前になってるので、$を境に2つに分けて
                        // 後ろがsearchKeywordならキーワードとしてKeywordに代入する
                        var targetKey = Request.Form.AllKeys.Where(x => {
                            var split = x.Split(new char[]{'$'}, 2);
                            return split.Length == 2 ? (split[1] == "searchKeyword") : false;
                        }).FirstOrDefault();
                        if (targetKey != null)
                        {
                            Keyword = Request.Form[targetKey];
                        }
                    }
                    break;
                case "GET":
                    Keyword = Request.QueryString["keyword"];
                    break;
            }
        }

        /// <summary>
        /// AdvanceSearch用の処理
        /// 別々に指定されたキーワードをページ上部の検索フォームで検索するときと同じ形式にして
        /// その形式の文字列で検索した検索ページにリダイレクト
        /// </summary>
        private void AdvanceSearch()
        {
            var query = new SearchQuery();
            string redirectTo;

            var keywordTable = new Dictionary<string, string>()
            {
                // aspx上の名前,       SearchQuery上のプロパティ名
                {"searchInKeyword",    "Keyword"},
                {"searchInName",       "InName"},
                {"searchInEmail",      "InEmail"},
                {"searchInTitle",      "InTitle"},
                {"searchNotInKeyword", "ExcludeKeyword"},
                {"searchNotInName",    "ExcludeInName"},
                {"searchNotInEmail",   "ExcludeInEmail"},
                {"searchNotInTitle",   "ExcludeInTitle"},
            };

            // 指定されたキーワードをSearchQueryに入れる
            foreach (var keyword in keywordTable)
            {
                string[] keywords;
                try
                {
                    keywords = SearchQuery
                        .SplitQuery(Request.Params[keyword.Key])
                        .Select(x => SearchQuery.UnescapeQueryString(x))
                        .ToArray();
                }
                catch (SearchQuery.UnexpectedEndOfQueryException)
                {
                    SearchAlerts.AddDanger("エラー: 検索キーワードがおかしいです (ダブルクオーテーションを閉じていない？)");
                    break;
                }

                // 対応するプロパティを取得し末尾に値を追加
                var list = (List<string>)typeof(SearchQuery).GetProperty(keyword.Value).GetValue(query, null);
                list.AddRange(keywords);
            }

            if (!String.IsNullOrEmpty(Request.Params["searchSince"]))
            {
                try
                {
                    query.Since = DateTime.Parse(Request.Params["searchSince"]);
                }
                catch (FormatException)
                {
                    SearchAlerts.AddDanger("エラー: 検索範囲の開始日時の形式が無効です");
                }
            }
            if (!String.IsNullOrEmpty(Request.Params["searchUntil"]))
            {
                try
                {
                    query.Until = DateTime.Parse(Request.Params["searchUntil"]);
                }
                catch (FormatException)
                {
                    SearchAlerts.AddDanger("エラー: 検索範囲の終了日時の形式が無効です");
                }
            }

            if (SearchAlerts.HasNoAlerts())
            {
                // 何もエラーが出ていなければ?keywordを付加したSearch.aspxにリダイレクト
                redirectTo = Request.Url.ToString();
                if (redirectTo.LastIndexOf("?") >= 0)
                {
                    redirectTo = redirectTo.Substring(0, redirectTo.LastIndexOf("?"));
                }
                redirectTo = new Uri(Request.Url, "?keyword=" + System.Web.HttpUtility.UrlEncode(query.ToString())).ToString();

                Response.Redirect(redirectTo);
            }
        }

        /// <summary>
        /// 検索クエリ
        /// </summary>
        class SearchQuery
        {
            /// <summary>
            /// 検索範囲(開始日時)
            /// </summary>
            public DateTime? Since { set; get; }

            /// <summary>
            /// 検索範囲(終了日時)
            /// </summary>
            public DateTime? Until { set; get; }

            /// <summary>
            /// タイトルに含まれるキーワード
            /// </summary>
            public List<string> InTitle { get { return _inTitle; } }
            private List<string> _inTitle = new List<string>();

            /// <summary>
            /// タイトルに含まれないキーワード
            /// </summary>
            public List<string> ExcludeInTitle { get { return _excludeInTitle; } }
            private List<string> _excludeInTitle = new List<string>();

            /// <summary>
            /// 名前に含まれるキーワード
            /// </summary>
            public List<string> InName { get { return _inName; } }
            private List<string> _inName = new List<string>();

            /// <summary>
            /// 名前に含まれないキーワード
            /// </summary>
            public List<string> ExcludeInName { get { return _excludeInName; } }
            private List<string> _excludeInName = new List<string>();

            /// <summary>
            /// メールアドレスに含まれるキーワード
            /// </summary>
            public List<string> InEmail { get { return _inEmail; } }
            private List<string> _inEmail = new List<string>();

            /// <summary>
            /// メールアドレスに含まれないキーワード
            /// </summary>
            public List<string> ExcludeInEmail { get { return _excludeInEmail; } }
            private List<string> _excludeInEmail = new List<string>();

            /// <summary>
            /// 本文中に含まれるキーワード
            /// </summary>
            public List<string> Keyword { get { return _keyword; } }
            private List<string> _keyword = new List<string>();

            /// <summary>
            /// 本文中に含まれないキーワード
            /// </summary>
            public List<string> ExcludeKeyword { get { return _excludeKeyword; } }
            private List<string> _excludeKeyword = new List<string>();

            /// <summary>
            /// 検索クエリの生成
            /// </summary>
            public SearchQuery() { }

            /// <summary>
            /// 既に存在する検索クエリをコピーして新しい検索クエリを生成
            /// </summary>
            /// <param name="basequery">元の検索クエリ</param>
            public SearchQuery(SearchQuery basequery)
            {
                if (basequery.Since.HasValue)
                {
                    this.Since = basequery.Since.Value;
                }
                if (basequery.Until.HasValue)
                {
                    this.Until = basequery.Until.Value;
                }
                this._inTitle = new List<string>(basequery.InTitle);
                this._excludeInTitle = new List<string>(basequery.ExcludeInTitle);
                this._inName = new List<string>(basequery.InName);
                this._excludeInName = new List<string>(basequery.ExcludeInName);
                this._inEmail = new List<string>(basequery.InEmail);
                this._excludeInEmail = new List<string>(basequery.ExcludeInEmail);
                this._keyword = new List<string>(basequery.Keyword);
                this._excludeKeyword = new List<string>(basequery.ExcludeKeyword);
            }

            /// <summary>
            /// クエリのラムダ式として検索クエリを取得
            /// </summary>
            /// <returns>クエリのラムダ式</returns>
            public Expression<Func<BBSData.Post, bool>> ToLambdaExpression()
            {
                var param = Expression.Parameter(typeof(BBSData.Post));
                var expr = ToExpression(param);
                return expr == null ? null : Expression.Lambda<Func<BBSData.Post, bool>>(ToExpression(param), param);
            }

            /// <summary>
            /// 式木として検索クエリを取得
            /// </summary>
            /// <param name="param">パラメータ</param>
            /// <returns>クエリの式木</returns>
            public Expression ToExpression(ParameterExpression param)
            {
                var exprs = new List<Expression>();
                Expression retExpr = null;
                if (Since.HasValue)
                {
                    // x => x.CreatedAt.Value >= Since.Value
                    exprs.Add(Expression.GreaterThanOrEqual(
                        Expression.Property(Expression.Property(param, "CreatedAt"), "Value"),
                        Expression.Constant(Since.Value)));
                }
                if (Until.HasValue)
                {
                    // x => x.CreatedAt.Value <= Until.Value
                    exprs.Add(Expression.LessThanOrEqual(
                        Expression.Property(Expression.Property(param, "CreatedAt"), "Value"),
                        Expression.Constant(Until.Value)));
                }

                // 式木に変換するSearchQueryクラスのプロパティとBBSData.*クラスのプロパティの対応
                var targetTbl = new Dictionary<string, Expression>()
                {
                    {"InTitle", Expression.Property(Expression.Property(param, "Thread"), "Title")},
                    {"ExcludeInTitle", Expression.Property(Expression.Property(param, "Thread"), "Title")},
                    {"InName", Expression.Property(param, "Name")},
                    {"ExcludeInName", Expression.Property(param, "Name")},
                    {"InEmail", Expression.Property(param, "Email")},
                    {"ExcludeInEmail", Expression.Property(param, "Email")},
                    {"Keyword", Expression.Property(param, "Text")},
                    {"ExcludeKeyword", Expression.Property(param, "Text")},
                };

                foreach (var target in targetTbl)
                {
                    var is_exclude = target.Key.StartsWith("Exclude");
                    var keywords = (List<string>)this.GetType().GetProperty(target.Key).GetGetMethod().Invoke(this, null);

                    foreach (var value in keywords)
                    {
                        // target.Value.Contains(value)
                        Expression expr = Expression.Call(target.Value, typeof(string).GetMethod("Contains"), Expression.Constant(value));
                        // 除外条件だったらnotをつける
                        if (is_exclude)
                        {
                            expr = Expression.Not(expr);
                        }
                        exprs.Add(expr);
                    }
                }

                if (exprs.Count >= 0)
                {
                    retExpr = exprs[0];
                    foreach (var expr in exprs.Skip(1))
                    {
                        // x => retExpr && expr
                        retExpr = Expression.AndAlso(retExpr, expr);
                    }
                }
                return retExpr;
            }

            /// <summary>
            /// 検索クエリから検索キーワードの文字列の生成
            /// </summary>
            /// <param name="type">キーワードの種類 (intitleなど)</param>
            /// <param name="values">キーワードの値</param>
            /// <param name="exclude">除外キーワードかどうか</param>
            /// <param name="escapeIfContains">存在したらエスケープする文字の一覧</param>
            /// <returns></returns>
            private string GeneratePrettyQueryString(string type, IEnumerable<string> values, bool exclude = false, IEnumerable<char> escapeIfContains = null)
            {
                if (values == null) return null;

                var escapeIfContains2 = new List<char>();
                if (escapeIfContains != null)
                {
                    // escapeIfContainsが指定されていればそれを設定
                    escapeIfContains2.AddRange(escapeIfContains);
                }
                else
                {
                    // escapeIfContainsが未指定なら初期値(' 'が存在した時のみエスケープ)を設定
                    escapeIfContains2.Add(' ');
                }
                // "は必ずエスケープするのでescapeIfContainsの指定未指定関係なく対象にする
                escapeIfContains2.Add('"');

                // キーワードに含める文字列の取得
                // 空文字列と重複しているキーワードは必要ないので取り除く
                var targetValues = values.Where(x => x != "").Distinct();

                // 文字列のうち\を含むものは\\、"を含むものは\"にする
                targetValues = targetValues.Select(x => x.Replace("\\", "\\\\").Replace("\"", "\\\""));

                // values中の文字列のうち
                // * escapeIfContainsの文字のどれかを含む
                // * typeが未指定で除外条件ではないが-から始まっている
                // のどちらかを満たす場合"を文字列の前後に付加する
                var escapedValues = targetValues
                    .Select(x => x.IndexOfAny(escapeIfContains2.ToArray()) > 0 || (type == null && !exclude && x.StartsWith("-"))
                            ? "\"" + x + "\""
                            : x);
                var finalStrings = escapedValues;

                // typeが指定されていた場合先頭にtype:をつける
                if (type != null)
                {
                    finalStrings = finalStrings.Select(x => type + ":" + x);
                }

                // excludeが指定されていた場合先頭に-をつける
                if (exclude)
                {
                    finalStrings = finalStrings.Select(x => "-" + x);
                }

                // " "で連結
                return String.Join(" ", finalStrings);
            }

            /// <summary>
            /// 検索クエリを文字列にする
            /// </summary>
            /// <returns>検索クエリの文字列</returns>
            public override string ToString()
            {
                var str = "";
                str += " " + GeneratePrettyQueryString(null, Keyword, false, new []{' ', ':'});
                str += " " + GeneratePrettyQueryString(null, ExcludeKeyword, true, new []{' ', ':'});
                str += " " + GeneratePrettyQueryString("inname", InName, false);
                str += " " + GeneratePrettyQueryString("inname", ExcludeInName, true);
                str += " " + GeneratePrettyQueryString("inemail", InEmail, false);
                str += " " + GeneratePrettyQueryString("inemail", ExcludeInEmail, true);
                str += " " + GeneratePrettyQueryString("intitle", InTitle, false);
                str += " " + GeneratePrettyQueryString("intitle", ExcludeInTitle, true);
				if (Since.HasValue)
                {
                    var datetimeStr = Since.Value.ToString("yyyy-MM-dd");
                    if (Since.Value.Hour == 0 && Since.Value.Minute == 0 && Since.Value.Second == 0)
                    {
                        // 時分秒どれも0なら時刻は表示しない
                    }
                    else if (Since.Value.Hour > 0 && Since.Value.Minute > 0 && Since.Value.Second == 0)
                    {
                        datetimeStr += " " + Since.Value.ToString("hh:mm");
                    }
                    else
                    {
                        datetimeStr += " " + Since.Value.ToString("hh:mm:ss");
                    }
                    if (datetimeStr.IndexOf(" ") > 0)
                    {
                        datetimeStr = "\"" + datetimeStr + "\"";
                    }
                    str += " since:" + datetimeStr;
				}
				if (Until.HasValue)
                {
                    var datetimeStr = Until.Value.ToString("yyyy-MM-dd");
                    if (Until.Value.Hour == 0 && Until.Value.Minute == 0 && Until.Value.Second == 0)
                    {
                        // 時分秒どれも0なら時刻は表示しない
                    }
                    else if (Until.Value.Hour > 0 && Until.Value.Minute > 0 && Until.Value.Second == 0)
                    {
                        datetimeStr += " " + Until.Value.ToString("hh:mm");
                    }
                    else
                    {
                        datetimeStr += " " + Until.Value.ToString("hh:mm:ss");
                    }
                    if (datetimeStr.IndexOf(" ") > 0)
                    {
                        datetimeStr = "\"" + datetimeStr + "\"";
                    }
                    str += " until:" + datetimeStr;
				}
                return str.Trim();
            }

            /// <summary>
            /// 2つの検索クエリを結合する
            /// </summary>
            /// <param name="b">元になるクエリ</param>
            /// <param name="q">追加するクエリ</param>
            /// <returns>結合された検索クエリ</returns>
            public static SearchQuery operator+(SearchQuery b, SearchQuery q)
            {
                var newquery = new SearchQuery(b);
                if (!newquery.Since.HasValue || (q.Since.HasValue && newquery.Since.Value < q.Since.Value))
                {
                    newquery.Since = q.Since;
                }
                if (!newquery.Until.HasValue || (q.Until.HasValue && newquery.Until.Value < q.Until.Value))
                {
                    newquery.Until = q.Until;
                }
                newquery.Keyword.AddRange(q.Keyword);
                newquery.ExcludeKeyword.AddRange(q.ExcludeKeyword);
                newquery.InTitle.AddRange(q.InTitle);
                newquery.ExcludeInTitle.AddRange(q.ExcludeInTitle);
                newquery.InName.AddRange(q.InName);
                newquery.ExcludeInName.AddRange(q.ExcludeInName);
                newquery.InEmail.AddRange(q.InEmail);
                newquery.ExcludeInEmail.AddRange(q.ExcludeInEmail);
                return newquery;
            }

            /// <summary>
            /// エスケープを配慮したクエリ文字列の分解
            /// </summary>
            /// <param name="query">クエリ文字列</param>
            /// <returns>分割されたクエリ文字列</returns>
            public static string[] SplitQuery(string query)
            {
                var retStrings = new List<string>();
                int end;
                int begin = 0;
                string str;
                var isQuote = false;    // ダブルクオーテーションで囲まれている部分かどうか
                for (end = 0; end < query.Length; end++)
                {
                    if (query[end] == '"')
                    {
                        isQuote = !isQuote;
                    }
                    else if (isQuote && query[end] == '\\')
                    {
                        if (end + 1 < query.Length && query[end + 1] == '\\')
                        {
                            end++;
                        }
                        else if (end + 1 < query.Length && query[end + 1] == '\"')
                        {
                            end++;
                        }
                    }
                    else if (!isQuote && query[end] == ' ')
                    {
                        str = query.Substring(begin, end - begin);
                        if (str != "")
                        {
                            retStrings.Add(str);
                        }
                        begin = end + 1;
                    }
                }
                str = query.Substring(begin, end - begin);
                if (str != "")
                {
                    retStrings.Add(str);
                }
                // クオーテーションが閉じていないので例外
                if (isQuote)
                {
                    throw new UnexpectedEndOfQueryException();
                }
                return retStrings.ToArray();
            }

            /// <summary>
            /// クエリ文字列をパースして検索クエリを生成
            /// </summary>
            /// <param name="query">クエリ文字列</param>
            /// <returns>検索クエリ</returns>
            public static SearchQuery Parse(string query)
            {
                var retQuery = new SearchQuery();
                var queryString = SplitQuery(query);
                foreach (var str in queryString)
                {
                    var type = GetQueryType(str);
                    string unescapedType = null;
                    string unescapedValue = null;

                    if (type != null)
                    {
                        // クエリの種類が指定されていたのでそれに合わせてキーワードを追加
                        unescapedType = UnescapeQueryString(type).ToLower();
                        unescapedValue = UnescapeQueryString(str.Substring(type.Length + 1));

                        switch (unescapedType)
                        {
                            case "intitle": retQuery.InTitle.Add(unescapedValue); break;
                            case "-intitle": retQuery.ExcludeInTitle.Add(unescapedValue); break;
                            case "inname": retQuery.InName.Add(unescapedValue); break;
                            case "-inname": retQuery.ExcludeInName.Add(unescapedValue); break;
                            case "inemail": retQuery.InEmail.Add(unescapedValue); break;
                            case "-inemail": retQuery.ExcludeInEmail.Add(unescapedValue); break;
                            case "since":
                                try
                                {
                                    var sinceDateTime = DateTime.Parse(unescapedValue);
                                    if (retQuery.Since == null || retQuery.Since < sinceDateTime)
                                    {
                                        retQuery.Since = sinceDateTime;
                                    }
                                }
                                catch (FormatException e)
                                {
                                    throw new FormatException("sinceに無効な日付が指定されました: \"" + unescapedValue + "\"", e);
                                }
                                break;
                            case "until":
                                try
                                {
                                    var untilDateTime = DateTime.Parse(unescapedValue);
                                    if (retQuery.Until == null || retQuery.Until > untilDateTime)
                                    {
                                        retQuery.Until = untilDateTime;
                                    }
                                }
                                catch (FormatException e)
                                {
                                    throw new FormatException("untilに無効な日付が指定されました: \"" + unescapedValue + "\"", e);
                                }
                                break;
                            default:
                                unescapedValue = UnescapeQueryString(str);
                                if (unescapedValue.Length > 0)
                                {
                                    if (unescapedValue[0] == '-' && str[0] != '"')
                                    {
                                        retQuery.ExcludeKeyword.Add(unescapedValue.Substring(1));
                                    }
                                    else
                                    {
                                        retQuery.Keyword.Add(unescapedValue);
                                    }
                                }
                                break;
                        }
                    }
                    else
                    {
                        // クエリの種類が指定されていないので本文のキーワードとして追加
                        unescapedValue = UnescapeQueryString(str);
                        if (unescapedValue.Length > 0)
                        {
                            if (unescapedValue[0] == '-' && str[0] != '"')
                            {
                                retQuery.ExcludeKeyword.Add(unescapedValue.Substring(1));
                            }
                            else
                            {
                                retQuery.Keyword.Add(unescapedValue);
                            }
                        }
                    }
                }
                return retQuery;
            }

            /// <summary>
            /// クエリの種類の取得
            /// </summary>
            /// <param name="str">クエリ文字列</param>
            /// <returns>クエリの種類</returns>
            public static string GetQueryType(string str)
            {
                var isQuote = false;
                for (var i = 0; i < str.Length; i++)
                {
                    if (str[i] == '"' && (i == 0 || (i > 0 && str[i - 1] != '\\')))
                    {
                        isQuote = !isQuote;
                    }
                    if (!isQuote && str[i] == ':')
                    {
                        var type = str.Substring(0, i);

                        // エスケープされている"(\")の数を数える
                        int escapedQuoteCount = 0;
                        int indexof = 0;
                        while (true)
                        {
                            indexof = type.IndexOf("\\\"", indexof);
                            if (indexof == -1) break;
                            escapedQuoteCount++;
                            indexof += 2;
                        }

                        // (type中の"の数)-(\"の数)が偶数であればクオートは完結しているので処理を完了しtypeを返す
                        if ((type.Count(x => x == '"') - escapedQuoteCount) % 2 == 0) return type;
                    }
                }
                return null;
            }

            /// <summary>
            /// エスケープされたクエリ文字列をエスケープされていない文字列に変換
            /// </summary>
            /// <param name="str">エスケープされたクエリ文字列</param>
            /// <returns>エスケープを解除したクエリ文字列</returns>
            public static string UnescapeQueryString(string str)
            {
                var retString = "";
                var isQuote = false;
                for (var i = 0; i < str.Length; i++)
                {
                    if (str[i] == '"')
                    {
                        isQuote = !isQuote;
                    }
                    else if (isQuote && str[i] == '\\')
                    {
                        if (i + 1 < str.Length && str[i + 1] == '\\')
                        {
                            retString += "\\";
                            i++;
                        }
                        else if (i + 1 < str.Length && str[i + 1] == '\"')
                        {
                            retString += "\"";
                            i++;
                        }
                    }
                    else
                    {
                        retString += str[i];
                    }
                }

                // クオーテーションが閉じていないので例外
                if (isQuote)
                {
                    throw new UnexpectedEndOfQueryException();
                }

                return retString;
            }

            /// <summary>
            /// 途中でキーワードが終了してしまったときの例外
            /// (ダブルクオーテーションの閉じ忘れなど)
            /// </summary>
            public class UnexpectedEndOfQueryException : Exception { }
        }
    }
}
