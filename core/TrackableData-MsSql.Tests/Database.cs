using System;
using System.Configuration;
using System.Data.SqlClient;

namespace TrackableData.Sql.Tests
{
    public class Database : IDisposable
    {
        public Database()
        {
            var cstr = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;

            // create TestDb if not exist

            var cstrForMaster = "";
            {
                var connectionBuilder = new SqlConnectionStringBuilder(cstr);
                connectionBuilder.InitialCatalog = "master";
                cstrForMaster = connectionBuilder.ToString();
            }

            using (var conn = new SqlConnection(cstrForMaster))
            {
                conn.Open();

                using (var cmd = new SqlCommand())
                {
                    cmd.CommandText = string.Format(@"
                        IF db_id('{0}') IS NULL
                            BEGIN
                                CREATE DATABASE {0}
                            END
                    ", new SqlConnectionStringBuilder(cstr).InitialCatalog);
                    cmd.Connection = conn;

                    var result = cmd.ExecuteScalar();
                }
            }
        }

        public SqlConnection Connection
        {
            get
            {
                var cstr = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;
                var connection = new SqlConnection(cstr);
                connection.Open();
                return connection;
            }
        }

        public void Dispose()
        {
            /*
            var connectionString = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand())
            {
                conn.Open();
                cmd.CommandText = @"
                    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'EventJournal') BEGIN DELETE FROM dbo.EventJournal END;
                    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'SnapshotStore') BEGIN DELETE FROM dbo.SnapshotStore END";
                cmd.Connection = conn;
                cmd.ExecuteNonQuery();
            }
            */
        }
    }
}
