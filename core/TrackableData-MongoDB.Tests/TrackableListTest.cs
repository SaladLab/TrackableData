using System.Threading.Tasks;
using MongoDB.Bson;
using Xunit;

namespace TrackableData.MongoDB.Tests
{
    public class TrackableListTest : IClassFixture<Database>
    {
        private Database _db;

        public TrackableListTest(Database db)
        {
            _db = db;
        }
    }
}
