using System;
using System.Configuration;
using MongoDB.Driver;

namespace TrackableData.MongoDB.Tests
{
    public class Database : IDisposable
    {
        public MongoClient Client { get; private set; }

        public IMongoDatabase Test => Client.GetDatabase("Test");

        public Database()
        {
            var cstr = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString;
            Client = new MongoClient(cstr);
        }

        public void Dispose()
        {
            Client = null;
        }
    }
}
