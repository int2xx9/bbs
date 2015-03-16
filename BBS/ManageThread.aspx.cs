using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BBS
{
    public partial class ManageThread : Page
    {
        /// <summary>
        /// スレッドの情報
        /// </summary>
        public BBSData.Thread ThreadInfo { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            GetThreadInfo();
            if (ThreadInfo != null)
            {
                newTitleTextBox.Text = ThreadInfo.Title;
            }
        }

        /// <summary>
        /// スレッドの情報の取得
        /// </summary>
        private void GetThreadInfo()
        {
            int threadId;
            ThreadInfo = null;
            if (int.TryParse(Request.QueryString["thread_id"], out threadId))
            {
                var db = BBSData.OpenDatabase();
                ThreadInfo = db.Threads.Where(x => x.Id == threadId).FirstOrDefault();
            }
        }

        protected void deleteButton_Click(object sender, EventArgs e)
        {
            try
            {
                var db = BBSData.OpenDatabase();
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // スレッドに含まれる投稿すべてを削除
                        foreach (var post in db.Posts.Where(x => x.ThreadId == ThreadInfo.Id))
                        {
                            db.Posts.Remove(post);
                        }
                        // スレッドの情報の削除
                        db.Threads.Remove(db.Threads.Where(x => x.Id == ThreadInfo.Id).First());
                        db.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // 削除に失敗したらロールバックして呼び出し元に例外を投げる
                        transaction.Rollback();
                        throw ex;
                    }
                }
                Response.Redirect("/");
            }
            catch (Exception)
            {
                ManageThreadAlerts.AddDanger("エラー: スレッドの削除時にエラーが発生しました");
            }
        }

        protected void newTitleButton_Click(object sender, EventArgs e)
        {
            try
            {
                var db = BBSData.OpenDatabase();
                db.Threads.Single(x => x.Id == ThreadInfo.Id).Title = Request.Form["newTitleTextBox"];
                db.SaveChanges();
                Response.Redirect("/");
            }
            catch (Exception)
            {
                ManageThreadAlerts.AddDanger("エラー: スレッドタイトル変更時にエラーが発生しました");
            }
        }
    }
}