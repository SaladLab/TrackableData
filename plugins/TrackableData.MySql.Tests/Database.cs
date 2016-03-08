using System;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using TrackableData.Sql;

namespace TrackableData.MySql.Tests
{
    public class Database : SqlTestKits.IDbConnectionProvider, IDisposable
    {
        public Database()
        {
            var cstr = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;

            // create TestDb if not exist

            var cstrWithoutDatabase = Regex.Replace(cstr, "Database=[^;]+", "");
            var databaseName = Regex.Match(cstr, "Database=([^;]+)").Groups[1].ToString();
            using (var conn = new MySqlConnection(cstrWithoutDatabase))
            {
                conn.Open();

                using (var cmd = new MySqlCommand())
                {
                    cmd.CommandText = string.Format(@"
                        DROP DATABASE IF EXISTS {0};
                        CREATE DATABASE {0};
                    ", new SqlConnectionStringBuilder(cstr).InitialCatalog);
                    cmd.Connection = conn;

                    var result = cmd.ExecuteScalar();
                }
            }
        }

        public DbConnection Connection
        {
            get
            {
                var cstr = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;
                var connection = new MySqlConnection(cstr);
                connection.Open();
                return connection;
            }
        }

        public void Dispose()
        {
        }

        public static ISqlProvider SqlProvider => MySqlProvider.Instance;
    }
}
