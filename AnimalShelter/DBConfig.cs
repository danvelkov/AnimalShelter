using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace AnimalShelter
{
    class DBConfig
    {
        private static string Server = "localhost";
        private static string Database = "shelter";
        private static string ID = "admin";
        private static string PASS = "admin";

        static string conf = "Server=" + Server + ";Database=" + Database + ";Uid=" + ID + ";Pwd=" + PASS;

        static MySqlConnection db;

        public static MySqlConnection Connection
        {
            get
            {
                if (db == null)
                {
                    LazyInitializer.EnsureInitialized(ref db, CreateConnection);
                }
                return db;
            }
        }

        static MySqlConnection CreateConnection()
        {
            var db = new MySqlConnection(conf);
            return db;
        }

        static void CloseConnection()
        {
            if (db != null)
            {
                db.Close();
                db.Dispose();
                db = null;
            }
        }
    }
}
