using StackExchange.Redis;
using System;
using System.Configuration;

namespace TrackableData.Redis.Tests
{
    public class Redis : IDisposable
    {
        public ConnectionMultiplexer Connection { get; private set; }

        public IDatabase Db
        {
            get { return Connection.GetDatabase(0); }
        }

        public Redis()
        {
            var cstr = ConfigurationManager.ConnectionStrings["TestRedis"].ConnectionString;
            Connection = ConnectionMultiplexer.ConnectAsync(cstr + ",allowAdmin=true").Result;
        }

        public void Dispose()
        {
            if (Connection != null)
            {
                Connection.Dispose();
                Connection = null;
            }
        }
    }
}
