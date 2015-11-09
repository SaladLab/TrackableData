using System;
using System.Configuration;
using System.Data.SqlClient;

namespace TrackableData.MsSql.Tests
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
        }
    }
}
