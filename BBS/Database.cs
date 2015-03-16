using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Pluralization;
using System.Data.SQLite;

/* データベースの構造(2014/10/30)
Threadテーブル
    Id int primary key autoincrement
        ID (自動的に1ずつ増えていく)
    Title string not null
        スレッドタイトル

Postsテーブル
    PostId int primary key
        書き込み番号
    ThreadId int primary key references Thread(Id)
        投稿先スレッドID
    Name string
        投稿者名
    Email string
        メールアドレス
    CreatedAt string
        投稿日時 ('年-月-日 時:分:秒'の形式の文字列データ)
    Text string not null
        本文
    Password string
        管理キー
*/

/* 使い方(基本)
//データベースに接続
var db = BBS.BBSData.OpenDatabase();

// SQL文でレコードの取得(スレッドの場合)
// スレッドの一覧が配列(みたいなもの)で返る
var records = db.Database.SqlQuery<BBSData.Thread>("select * from Threads");
string title = records[0].Title; // スレッドタイトルの取得

// SQL文でレコードの取得(書き込みの場合)
var records = db.Database.SqlQuery<BBSData.Post>("select * from Posts");

// SQL文でレコードの追加,更新,削除
db.Database.ExecuteSqlCommand("insert into Threads (Title) values ('タイトル')");
db.Database.ExecuteSqlCommand("update Threads set Title='新タイトル' where Id=1");
db.Database.ExecuteSqlCommand("delete from Posts");
*/

namespace BBS
{
    /// <summary>
    /// データベース操作用クラス
    /// </summary>
    public class BBSData : DbContext
    {
        /// <summary>
        /// Threadsテーブル用クラス
        /// </summary>
        public class Thread
        {
            /// <summary>
            /// ID (PrimaryKey)
            /// </summary>
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            [Key]
            public long Id { set; get; }

            /// <summary>
            /// タイトル (not null)
            /// </summary>
            [Required]
            public string Title { set; get; }

            /// <summary>
            /// スレッドに含まれる書き込み
            /// </summary>
            [Column(Order = 1)]
            public virtual ICollection<Post> Posts { set; get; }
        }

        /// <summary>
        /// Postsテーブル用クラス
        /// </summary>
        public class Post
        {
            /// <summary>
            /// 書き込み番号 (PrimaryKey)
            /// </summary>
            [Key]
            [Column(Order = 0)]
            public long PostId { set; get; }

            /// <summary>
            /// スレッドID (not null)
            /// </summary>
            [Key]
            [Column(Order = 1)]
            public long ThreadId { set; get; }

            /// <summary>
            /// 投稿者名
            /// </summary>
            public string Name { set; get; }

            /// <summary>
            /// メールアドレス
            /// </summary>
            public string Email { set; get; }

            /// <summary>
            /// 投稿日時
            /// </summary>
            [Required]
            public DateTime? CreatedAt { set; get; }

            /// <summary>
            /// 本文 (not null)
            /// </summary>
            [Required]
            public string Text { set; get; }

            /// <summary>
            /// 管理キー
            /// </summary>
            public string Password { set; get; }

            /// <summary>
            /// 書き込みが含まれるスレッド
            /// </summary>
            public virtual Thread Thread { set; get; }
        }

        /// <summary>
        /// Threadsテーブル
        /// </summary>
        public DbSet<Thread> Threads { set; get; }

        /// <summary>
        /// Postsテーブル
        /// </summary>
        public DbSet<Post> Posts { set; get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="connstr">接続文字列</param>
        protected BBSData(string connstr) : base(connstr)
        {
            Database.SetInitializer(new Initializer());
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="conn">データベースへの接続</param>
        protected BBSData(DbConnection conn) : base(conn, true)
        {
            Database.SetInitializer(new Initializer());
        }

        /// <summary>
        /// スレッドの作成
        /// </summary>
        /// <param name="thread">作成するスレッド</param>
        /// <param name="post">作成するスレッドに書き込む内容</param>
        public void CreateThread(Thread thread, Post post)
        {
            if (String.IsNullOrEmpty(thread.Title)) throw new ArgumentException("タイトルが必要です");
            if (!post.CreatedAt.HasValue) throw new ArgumentException("書き込み日時が必要です");
            if (String.IsNullOrEmpty(post.Text)) throw new ArgumentException("本文が必要です");

            post.Thread = thread;
            post.PostId = 1;
            using (var transaction = this.Database.BeginTransaction())
            {
                try
                {
                    Threads.Add(thread);
                    Posts.Add(post);
                    SaveChanges();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    // 例外が発生したらロールバックして例外を呼び出し元に投げる
                    transaction.Rollback();
                    throw e;
                }
            }
        }

        /// <summary>
        /// スレッドに書き込み
        /// </summary>
        /// <param name="thread_id">書き込むスレッドのID</param>
        /// <param name="post">書き込む内容</param>
        public void CreatePost(long thread_id, Post post)
        {
            CreatePost(Threads.Where(x => x.Id == thread_id).First(), post);
        }

        /// <summary>
        /// スレッドに書き込み
        /// </summary>
        /// <param name="thread">書き込むスレッド</param>
        /// <param name="post">書き込む内容</param>
        public void CreatePost(Thread thread, Post post)
        {
            if (Threads.Where(x => x.Id == thread.Id).Count() == 0) throw new ArgumentException("スレッドが存在しません");
            if (!post.CreatedAt.HasValue) throw new ArgumentException("書き込み日時が必要です");
            if (String.IsNullOrEmpty(post.Text)) throw new ArgumentException("本文が必要です");
            post.Thread = thread;
            Posts.Add(post);
            post.PostId = Posts.Where(x => x.ThreadId == thread.Id).Max(x => x.PostId) + 1;
            SaveChanges();
        }

        /// <summary>
        /// データベースを開く
        /// </summary>
        /// <returns>開いたデータベース</returns>
        public static BBSData OpenDatabase()
        {
            var conn = new SQLiteConnection(Config.ConnectionString);
            var db = new BBSData(conn);

            // 外部キー制約の有効化
            var cmd = conn.CreateCommand();
            conn.Open();
            cmd.CommandText = "pragma foreign_keys=on;";
            cmd.ExecuteNonQuery();

            // テーブルの存在チェック
            var count = db.Database.SqlQuery<TableCount>("select count(*) as Count from sqlite_master where type='table' and name in ('Threads', 'Posts')");
            if (count.Count() == 0 || count.FirstOrDefault().Count < 2)
            {
                // 存在しないので作る
                new Initializer().InitializeDatabase(db);
            }

            return db;
        }

        /// <summary>
        /// テーブル存在チェックの戻り値
        /// </summary>
        private class TableCount
        {
            public long Count { set; get; }
        }

        /// <summary>
        /// データベース初期化用クラス
        /// </summary>
        private class Initializer : IDatabaseInitializer<BBSData>
        {
            /// <summary>
            /// テーブルのフィールドの情報を現すクラス
            /// </summary>
            private class FieldInfo
            {
                /// <summary>
                /// C#での型とSQLiteでの型名の対応
                /// </summary>
                private static readonly Dictionary<Type, string> cstypeToSqltypeTbl = new Dictionary<Type, string>()
                {
                    {typeof(long), "integer"},
                    {typeof(string), "text"},
                    {typeof(DateTime?), "text"},
                };

                /// <summary>
                /// フィールドの種類
                /// </summary>
                public Type FieldType { set; get; }

                /// <summary>
                /// フィールドの名前
                /// </summary>
                public string FieldName { set; get; }

                /// <summary>
                /// primary keyかどうか
                /// </summary>
                public bool IsKey { set; get; }

                /// <summary>
                /// autoincrementであるかどうか
                /// </summary>
                public bool IsIdentity { set; get; }

                /// <summary>
                /// not nullであるかどうか
                /// </summary>
                public bool IsRequired { set; get; }

                /// <summary>
                /// 外部キー
                /// </summary>
                public Type References { set; get; }

                public FieldInfo(string fieldName, Type fieldType, bool isKey = false, bool isIdentity = false, bool isRequired = false, Type references = null)
                {
                    this.FieldName = fieldName;
                    this.FieldType = fieldType;
                    this.IsKey = isKey;
                    this.IsIdentity = isIdentity;
                    this.IsRequired = isRequired;
                    this.References = references;
                }

                public override string ToString()
                {
                    var pluraService = new EnglishPluralizationService();
                    var sqlQuery = "\"" + this.FieldName.Replace("\"", "\"\"") + "\" " + FieldInfo.cstypeToSqltypeTbl[this.FieldType];
                    // autoincrementが付いている場合のみprimary key制約をつける (autoincrementが付いてない場合はInitializeDatabase内で制約を付ける)
                    if (this.IsKey && this.IsIdentity) sqlQuery += " primary key";
                    if (this.IsRequired) sqlQuery += " not null";
                    if (this.IsIdentity) sqlQuery += " autoincrement";
                    if (this.References != null) sqlQuery += " references " + pluraService.Pluralize(this.References.Name).Replace("\"", "\"\"") + "(Id)";
                    return sqlQuery;
                }

                /// <summary>
                /// 指定したC#の型がこのクラスで対応してるかどうか
                /// </summary>
                /// <param name="type">チェックする型</param>
                /// <returns>対応している場合true、未対応の場合false</returns>
                public static bool IsSupportedType(Type type)
                {
                    return cstypeToSqltypeTbl.Any(x => x.Key == type);
                }
            }

            public void InitializeDatabase(BBSData db)
            {
                // BBSDataの中のDbSet<T>クラスのプロパティを探す
                // (= データベース中のテーブルとなるクラスの一覧を取得)
                var tableTypes = db.GetType().GetProperties()
                    .Where(propinfo => propinfo.GetGetMethod().ReturnType.IsGenericType)
                    .Where(propinfo => propinfo.GetGetMethod().ReturnType.GetGenericTypeDefinition() == typeof(DbSet<>))
                    .Select(propinfo => propinfo.GetGetMethod().ReturnType.GetGenericArguments()[0]);
                // 単数形(クラス名)→複数形(テーブル名)変換サービスの初期化
                var pluraService = new EnglishPluralizationService();

                foreach (var tableType in tableTypes)
                {
                    // テーブル名の取得
                    string tableName;
                    if (Attribute.GetCustomAttributes(tableType).Any(x => x.GetType() == typeof(TableAttribute)))
                    {
                        // Attributeが指定されている場合そちらから取得
                        tableName = ((TableAttribute)(Attribute.GetCustomAttributes(tableType)
                            .Where(x => x.GetType() == typeof(TableAttribute))
                            .FirstOrDefault())).Name;
                    }
                    else
                    {
                        // 指定されていない場合クラス名を複数形に変えてテーブル名にする
                        tableName = pluraService.Pluralize(tableType.Name);
                    }

                    // CREATE TABLE文の生成
                    var fields = new List<FieldInfo>(); // テーブルに含まれるフィールドの一覧
                    var sqlQuery = "create table if not exists \"" + tableName.Replace("\"", "\"\"") + "\" (";
                    foreach (var fieldProp in tableType.GetProperties())
                    {
                        // フィールド名
                        var fieldName = fieldProp.Name;
                        // プロパティの型
                        var returnType = fieldProp.GetGetMethod().ReturnType;

                        // FieldInfoでreturnTypeの型がサポートされている場合、そのプロパティ用のフィールドを作成
                        if (FieldInfo.IsSupportedType(returnType))
                        {
                            var field = new FieldInfo(fieldName, returnType);

                            // XxxxIdでXxxxがテーブル名の単数形と違う場合外部キーを設定
                            // (例: Postsテーブル上のPostIdフィールドにはこの処理を行わなず、
                            //  　  Postsテーブル上のThreadIdフィールドはこの処理を行う)
                            if (fieldName.Substring(0, fieldName.Length - 2) != pluraService.Singularize(tableName) &&
                                fieldName.Substring(fieldName.Length - 2) == "Id")
                            {
                                field.References = tableTypes
                                    .Where(x => x.Name == fieldProp.Name.Substring(0, fieldProp.Name.Length - 2))
                                    .FirstOrDefault();
                            }

                            // その他制約の設定
                            foreach (var attr in Attribute.GetCustomAttributes(fieldProp))
                            {
                                field.IsIdentity |= attr.GetType() == typeof(DatabaseGeneratedAttribute) && ((DatabaseGeneratedAttribute)attr).DatabaseGeneratedOption == DatabaseGeneratedOption.Identity;
                                field.IsRequired |= attr.GetType() == typeof(RequiredAttribute);
                                field.IsKey |= attr.GetType() == typeof(KeyAttribute);
                            }

                            fields.Add(field);
                        }
                    }

                    // それぞれのフィールドの情報をsqlQueryに追加
                    sqlQuery += String.Join(", ", fields.Select(x => x.ToString()));

                    // 主キー制約を付ける
                    // (autoincrementが付いていない主キーが存在するときのみ)
                    var primaryKeys = fields.Where(x => x.IsKey && !x.IsIdentity);
                    if (primaryKeys.Count() > 0)
                    {
                        sqlQuery += ", primary key (" + String.Join(", ", primaryKeys.Select(x => "\"" + x.FieldName.Replace("\"", "\"\"") + "\"")) + ")";
                    }

                    sqlQuery += ");";
                    System.Diagnostics.Debug.WriteLine(sqlQuery);

                    // テーブルの作成
                    var cmd = db.Database.Connection.CreateCommand();
                    cmd.CommandText = sqlQuery;
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
