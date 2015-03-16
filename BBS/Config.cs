using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace BBS
{
    public sealed class Config : ConfigurationSection
    {
        // シングルトン化
        private static Config instance = null;
        private static Config GetInstance()
        {
            if (instance == null)
            {
                instance = (Config)ConfigurationManager.GetSection("bbsConfig");
            }
            return instance;
        }
        // コンストラクタをprivateにして外からnewできないようにする
        private Config() { }

        /// <summary>
        /// 接続文字列
        /// </summary>
        public static string ConnectionString { get { return "Data Source=" + DatabasePath; } }

        /// <summary>
        /// データベースファイルの場所
        /// </summary>
        public static string DatabasePath { get { return GetInstance()._DatabasePath; } }

        /// <summary>
        /// 管理者パスワード
        /// </summary>
        public static string AdminPassword { get { return GetInstance()._AdminPassword; } }

        /// <summary>
        /// ページごとに表示するスレッドの数
        /// </summary>
        public static int ThreadsPerPage { get { return GetInstance()._ThreadsPerPage; } }

        [ConfigurationProperty("databasePath", IsRequired = true)]
        private string _DatabasePath { get { return (string)this["databasePath"]; } }
        [ConfigurationProperty("adminPassword", IsRequired = true)]
        private string _AdminPassword { get { return (string)this["adminPassword"]; } }
        [ConfigurationProperty("threadsPerPage", IsRequired = true)]
        private int _ThreadsPerPage { get { return (int)this["threadsPerPage"]; } }
    }
}