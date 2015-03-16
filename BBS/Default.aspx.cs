using System;
using System.Linq;
using System.Data.SQLite;
using System.Collections.Generic;

namespace BBS
{
    /// <summary>
    /// 投稿数、作成日時、最終投稿日時が含まれるスレッドの情報
    /// </summary>
    public class ThreadWithDetail
    {
        /// <summary>
        /// スレッドID
        /// </summary>
        public long Id { set; get; }

        /// <summary>
        /// スレッドタイトル
        /// </summary>
        public string Title { set; get; }

        /// <summary>
        /// 投稿数
        /// </summary>
        public long PostCount { set; get; }

        /// <summary>
        /// スレッドの作成日時
        /// </summary>
        public DateTime? CreatedAt { set; get; }

        /// <summary>
        /// 最終投稿日時
        /// </summary>
        public DateTime? LastPostAt { set; get; }
    }

    public partial class WebForm1 : Page
    {
        /// <summary>
        /// 全スレッドの情報
        /// </summary>
        public IQueryable<ThreadWithDetail> AllThreads { get; private set; }

        /// <summary>
        /// 選択中のページのスレッドの情報
        /// </summary>
        public IQueryable<ThreadWithDetail> PagedThreads { get; private set; }

        /// <summary>
        /// 選択中のページ番号
        /// </summary>
        public int PageNumber {
            get { return _pageNumber; }
            private set { _pageNumber = value; }
        }
        private int _pageNumber;

        /// <summary>
        /// 最後のページの番号
        /// </summary>
        protected int LastPageNumber { get { return Util.CalcLastPageNumber(AllThreads.Count(), Config.ThreadsPerPage); } }

        protected void Page_Load(object sender, EventArgs e)
        {
            // ページ番号を取得
            // ページ番号が1以下の場合1にする
            int.TryParse(Request.QueryString["page"], out _pageNumber);
            if (PageNumber < 1) PageNumber = 1;

            if (Request.Form["CreateThreadButton"] == "作成")
            {
                CreateThread();
                if (ThreadAlerts.HasNoAlerts())
                {
                    Response.Redirect(Request.Url.ToString());
                }
            }

            // クエリの生成
            var db = BBSData.OpenDatabase();
            AllThreads = db.Threads
                .Select(x => new ThreadWithDetail()
                {
                    Id = x.Id,
                    Title = x.Title,
                    PostCount = x.Posts.Count,
                    CreatedAt = x.Posts.Min(y => y.CreatedAt),
                    LastPostAt = x.Posts.Max(y => y.CreatedAt)
                })
                .OrderByDescending(x => x.LastPostAt);
            PagedThreads = AllThreads
                .Skip(GetOffset())
                .Take(Config.ThreadsPerPage);

            // クッキーに記録しておいたデータをフォームに入れる
            if (FormName != null)
            {
                name.Text = FormName;
            }
            if (FormEmail != null)
            {
                email.Text = FormEmail;
            }

            headnav.NavigationLinks.Add(new KeyValuePair<string, string>("#threadList", "スレッド一覧"));
            headnav.NavigationLinks.Add(new KeyValuePair<string, string>("#createThread", "スレッド作成"));
        }

        private int GetOffset()
        {
            return (PageNumber - 1) * Config.ThreadsPerPage;
        }

        private void CreateThread()
        {
            var db = BBSData.OpenDatabase();
            var thread = new BBSData.Thread();
            var post = new BBSData.Post() { CreatedAt = DateTime.Now };
            if (!String.IsNullOrEmpty(Request.Form["title"])) thread.Title = Request.Form["title"];
            if (!String.IsNullOrEmpty(Request.Form["text"])) post.Text = Request.Form["text"];
            if (!String.IsNullOrEmpty(Request.Form["name"])) post.Name = Request.Form["name"];
            if (!String.IsNullOrEmpty(Request.Form["email"])) post.Email = Request.Form["email"];
            if (!String.IsNullOrEmpty(Request.Form["password"])) post.Password = Request.Form["password"];
            try
            {
                db.CreateThread(thread, post);
            }
            catch (ArgumentException ex)
            {
                ThreadAlerts.AddDanger("エラー: " + ex.Message);
            }

            // クッキーに名前とメールアドレスを記録しておく
            FormName = post.Name;
            FormEmail = post.Email;
        }
    }
}