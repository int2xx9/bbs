using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Text.RegularExpressions;

namespace BBS
{
    public partial class Thread : Page
    {
        /// <summary>
        /// スレッドに含まれる投稿
        /// </summary>
        public IQueryable<BBSData.Post> MatchedPosts { get; private set; }

        /// <summary>
        /// 選択中のページに含まれる投稿
        /// </summary>
        public IQueryable<BBSData.Post> PagedPosts
        {
            get { return MatchedPosts; }
            private set { MatchedPosts = value; }
        }

        /// <summary>
        /// スレッドの情報
        /// </summary>
        public BBSData.Thread ThreadInfo { get; private set; }

        /// <summary>
        /// 指定されたスレッドID
        /// 0の場合は無効なスレッドIDが指定された
        /// </summary>
        public long ThreadId { get { return threadId; } private set { threadId = value; } }
        private long threadId;

        protected void Page_Load(object sender, EventArgs e)
        {
            ThreadInfo = null;
            long.TryParse(Request.QueryString["thread_id"], out threadId);
            GetThreadInfo();

            if (IsThreadAvailable())
            {
                Title = ThreadInfo.Title;

                if (Request.Form["PostButton"] == "書き込み")
                {
                    // 書き込みボタンが押された
                    CreatePost();
                    if (PostAlerts.HasNoAlerts())
                    {
                        // エラーが表示されていなければ自分自身にリダイレクト
                        // (更新で二重書き込み対策)
                        Response.Redirect(Request.Url.ToString());
                    }
                }

                // スレッド内容表示のクエリを生成
                GenerateQuery();

                // クッキーに記録しておいたデータをフォームに入れる
                if (FormName != null)
                {
                    name.Text = FormName;
                }
                if (FormEmail != null)
                {
                    email.Text = FormEmail;
                }

                // ナビゲーションバーに項目を追加
                headnav.NavigationLinks.Add(new KeyValuePair<string, string>("#post1", "1"));
                headnav.NavigationLinks.Add(new KeyValuePair<string, string>("#createPost", "新規書き込み"));
            }
        }

        /// <summary>
        /// スレッドの情報を取得するクエリの生成
        /// </summary>
        private void GenerateQuery()
        {
            var db = BBSData.OpenDatabase();
            MatchedPosts = db.Posts.Where(x => x.ThreadId == ThreadInfo.Id);
            if (Request.QueryString["range"] != null)
            {
                // rangeパラメータが指定されていた時の処理
                // 対応している指定の例
                // 1: 書き込み番号1の書き込みを表示
                // 1,2: 書き込み番号1,2の書き込みを表示
                // 1-5: 書き込み番号1,2,3,4,5の書き込みを表示
                // 1-: 書き込み番号1以降の書き込みを表示
                // -100: 書き込み番号100までの書き込みを表示
                // 1,5-10: 書き込み番号1、書き込み番号5～10の書き込みを表示

                // ラムダ式に渡された引数
                var linqParam = Expression.Parameter(typeof(BBSData.Post));
                // ラムダ式に渡された引数のPostIdプロパティを参照する式木
                var PostIDProperty = Expression.Property(linqParam, "PostId");
                // 条件文のリスト
                var orexprs = new List<Expression>();

                // ,で区切られている数字/範囲を分解
                var ranges = Request.QueryString["range"].Split(',');

                // 数字単体の指定をlong型の整数の配列にする
                var rangesSingle = ranges.Where(x => Regex.IsMatch(x, @"^\d+$")).Select(x => long.Parse(x));
                foreach (var number in rangesSingle)
                {
                    // x => x.PostId == number
                    Expression expr = Expression.Equal(PostIDProperty, Expression.Constant(number));
                    // 条件文に追加
                    orexprs.Add(expr);
                }

                // 1-2、1-、-2みたいな形式を{開始位置, 終了位置}(共にlong型)にする
                // 始点あるいは終点が省略されていたらnullにする
                var rangesRange = ranges.Where(x => Regex.IsMatch(x, @"^(\d+-\d+|\d+-|-\d+)$")).Select<string, long?[]>(x =>
                {
                    // 前後分割
                    var split = x.Split(new char[] { '-' }, 2);
                    if (split[0] != "" && split[1] == "") return new long?[] { long.Parse(split[0]), null };
                    if (split[0] == "" && split[1] != "") return new long?[] { null, long.Parse(split[1]) };
                    return new long?[] { long.Parse(split[0]), long.Parse(split[1]) };
                });
                foreach (var range in rangesRange)
                {
                    Expression expr = null;
                    // 範囲の開始書き込み番号
                    var rangeStart = range[0] == null ? null : Expression.Constant(range[0]);
                    // 範囲の終了書き込み番号
                    var rangeEnd = range[1] == null ? null : Expression.Constant(range[1]);

                    if (rangeStart != null && rangeEnd != null)
                    {
                        // 範囲の開始と終了が両方指定されている
                        // x => x >= range_start && x <= range_end
                        expr = Expression.AndAlso(
                            Expression.GreaterThanOrEqual(PostIDProperty, rangeStart),
                            Expression.LessThanOrEqual(PostIDProperty, rangeEnd));
                    }
                    else if (rangeStart != null)
                    {
                        // 範囲の開始のみが指定されている
                        // x => x.PostId >= range_start
                        expr = Expression.GreaterThanOrEqual(PostIDProperty, rangeStart);
                    }
                    else if (rangeEnd != null)
                    {
                        // 範囲の終了のみが指定されている
                        // x => x.PostId <= range_end
                        expr = Expression.LessThanOrEqual(PostIDProperty, rangeEnd);
                    }
                    // 条件文に追加
                    orexprs.Add(expr);
                }

                if (orexprs.Count > 0)
                {
                    // 上で生成した条件文をOR条件で連結
                    Expression linqExpr = orexprs.FirstOrDefault();
                    foreach (var expr in orexprs.Skip(1))
                    {
                        linqExpr = Expression.OrElse(expr, linqExpr);
                    }
                    // MatchedPostsの条件に加える
                    MatchedPosts = MatchedPosts.Where(Expression.Lambda<Func<BBSData.Post, bool>>(linqExpr, linqParam));
                }
            }
            MatchedPosts = MatchedPosts.OrderBy(x => x.CreatedAt);
        }

        /// <summary>
        /// スレッドの情報の取得
        /// </summary>
        private void GetThreadInfo()
        {
            if (ThreadId > 0)
            {
                var db = BBSData.OpenDatabase();
                ThreadInfo = db.Threads.Where(x => x.Id == ThreadId).FirstOrDefault();
            }
        }

        /// <summary>
        /// 書き込み
        /// </summary>
        private void CreatePost()
        {
            // 書き込みデータの準備
            // CreatedAtはユーザからの入力データではないので、この場で生成し初期値として設定しておく
            var post = new BBSData.Post() { CreatedAt = DateTime.Now };
            if (!String.IsNullOrEmpty(Request.Form["text"])) post.Text = Request.Form["text"];
            if (!String.IsNullOrEmpty(Request.Form["name"])) post.Name = Request.Form["name"];
            if (!String.IsNullOrEmpty(Request.Form["email"])) post.Email = Request.Form["email"];
            if (!String.IsNullOrEmpty(Request.Form["password"])) post.Password = Request.Form["password"];
 
            // 書き込み
            var db = BBSData.OpenDatabase();
            try
            {
                db.CreatePost(ThreadInfo.Id, post);
            }
            catch (ArgumentException ex)
            {
                PostAlerts.AddDanger("エラー: " + ex.Message);
            }

            // クッキーに名前とメールアドレスを記録しておく
            FormName = post.Name;
            FormEmail = post.Email;
        }

        /// <summary>
        /// スレッドが存在するかどうか
        /// </summary>
        /// <returns>存在する場合true、存在しない場合false</returns>
        protected bool IsThreadAvailable()
        {
            return ThreadInfo != null;
        }
    }
}
