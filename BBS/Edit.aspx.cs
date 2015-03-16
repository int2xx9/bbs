using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BBS
{
    public partial class Edit : Page
    {
        public long ThreadId { get; private set; }
        public long PostId { get; private set; }
        protected bool IsInvalidThreadId { get; private set; }
        protected bool IsInvalidPostId { get; private set; }
        public BBSData.Thread Thread { get; private set; }
        public BBSData.Post Post { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!String.IsNullOrEmpty(Request.QueryString["thread_id"]))
                {
                    ThreadId = long.Parse(Request.QueryString["thread_id"]);
                }
                else if (!String.IsNullOrEmpty(Request.Form["thread_id"]))
                {
                    ThreadId = long.Parse(Request.Form["thread_id"]);
                }
                else
                {
                    IsInvalidThreadId = true;
                }
                if (!IsInvalidThreadId)
                {
                    using (var db = BBSData.OpenDatabase())
                    {
                        Thread = db.Threads.Where(x => x.Id == ThreadId).FirstOrDefault();
                        if (Thread == null)
                        {
                            IsInvalidThreadId = true;
                        }
                    }
                }
            }
            catch (ArgumentNullException) { IsInvalidThreadId = true; }
            catch (FormatException) { IsInvalidThreadId = true; }
            catch (OverflowException) { IsInvalidThreadId = true; }
            try
            {
                if (!String.IsNullOrEmpty(Request.QueryString["post_id"]))
                {
                    PostId = long.Parse(Request.QueryString["post_id"]);
                }
                else if (!String.IsNullOrEmpty(Request.Form["post_id"]))
                {
                    PostId = long.Parse(Request.Form["post_id"]);
                }
                else
                {
                    IsInvalidPostId = true;
                }
                if (!IsInvalidThreadId && !IsInvalidPostId)
                {
                    using (var db = BBSData.OpenDatabase())
                    {
                        Post = db.Posts.Where(x => x.ThreadId == ThreadId && x.PostId == PostId).FirstOrDefault();
                        if (Post == null)
                        {
                            IsInvalidPostId = true;
                        }
                    }
                }
            }
            catch (ArgumentNullException) { IsInvalidPostId = true; }
            catch (FormatException) { IsInvalidPostId = true; }
            catch (OverflowException) { IsInvalidPostId = true; }

            if (!IsInvalidPostId && !IsInvalidThreadId)
            {
                if (Request.Form["EditButton"] != null)
                {
                    // 編集ボタンが押された
                    if (EditPost())
                    {
                        // 編集に成功したのでスレッドページに戻る
                        Response.Redirect("/Thread.aspx?thread_id=" + ThreadId);
                    }
                }
                else if (Request.Form["ThreadDeleteButton"] != null && PostId == 1)
                {
                    // 削除ボタン(スレッド)が押された
                    if (DeletePost())
                    {
                        // 削除に成功したのでスレ削除の場合トップに戻る
                        Response.Redirect("/");
                    }
                }
                else if (Request.Form["PostDeleteButton"] != null && PostId != 1)
                {
                    // 削除ボタン(書き込み)が押された
                    if (DeletePost())
                    {
                        // 削除に成功したのでスレッドページに戻る
                        Response.Redirect("/Thread.aspx?thread_id=" + ThreadId);
                    }
                }
                else
                {
                    // ボタンは押されてない
                    title.Text = Thread.Title;
                    name.Text = Post.Name;
                    email.Text = Post.Email;
                    text.Text = Post.Text;
                }
            }
        }

        private bool EditPost()
        {
            if (!IsLoginedAsAdmin() && Post.Password != Request.Form["password"])
            {
                EditAlerts.AddDanger("パスワードが間違っています");
                return false;
            }
            using (var db = BBSData.OpenDatabase())
            {
                if (PostId == 1)
                {
                    var thread = db.Threads.Where(x => x.Id == ThreadId).SingleOrDefault();
                    thread.Title = title.Text;
                }
                var post = db.Posts.Where(x => x.PostId == PostId && x.ThreadId == ThreadId).SingleOrDefault();
                post.Name = name.Text;
                post.Email = email.Text;
                post.Text = text.Text;
                db.SaveChanges();
            }
            return true;
        }

        private bool DeletePost()
        {
            if (!IsLoginedAsAdmin() && Post.Password != Request.Form["password"])
            {
                EditAlerts.AddDanger("パスワードが間違っています");
                return false;
            }
            if (PostId == 1)
            {
                using (var db = BBSData.OpenDatabase())
                {
                    db.Posts.RemoveRange(db.Posts.Where(x => x.ThreadId == ThreadId));
                    db.Threads.RemoveRange(db.Threads.Where(x => x.Id == ThreadId));
                    db.SaveChanges();
                }
            }
            else
            {
                using (var db = BBSData.OpenDatabase())
                {
                    db.Posts.Remove(db.Posts.Where(x => x.ThreadId == ThreadId && x.PostId == PostId).SingleOrDefault());
                    db.SaveChanges();
                }
           }
           return true;
        }
    }
}