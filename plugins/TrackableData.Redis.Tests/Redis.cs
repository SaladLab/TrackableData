using System;
using System.Configuration;
using StackExchange.Redis;

namespace TrackableData.Redis.Tests
{
    public sealed class Redis : IDisposable
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
