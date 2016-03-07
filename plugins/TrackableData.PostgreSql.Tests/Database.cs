using System;
using System.Configuration;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Npgsql;

namespace TrackableData.PostgreSql.Tests
{
    public class Database : IDisposable
    {
        private List<NpgsqlConnection> _connections = new List<NpgsqlConnection>();

        public Database()
        {
            var cstr = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;

            // create TestDb if not exist

            var cstrWithoutDatabase = Regex.Replace(cstr, "Database=[^;]+", "");
            var databaseName = Regex.Match(cstr, "Database=([^;]+)").Groups[1].ToString();
            using (var conn = new NpgsqlConnection(cstrWithoutDatabase))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand())
                {
                    cmd.CommandText = string.Format(@"
                        DROP DATABASE IF EXISTS {0};
                        CREATE DATABASE {0};
                    ", databaseName);
                    cmd.Connection = conn;

                    var result = cmd.ExecuteScalar();
                }
            }
        }

        public NpgsqlConnection Connection
        {
            get
            {
                var cstr = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;
                var connection = new NpgsqlConnection(cstr);
                connection.Open();
                _connections.Add(connection);
                return connection;
            }
        }

        public void Dispose()
        {
            foreach (var connection in _connections)
                connection.Dispose();
        }
    }
}
