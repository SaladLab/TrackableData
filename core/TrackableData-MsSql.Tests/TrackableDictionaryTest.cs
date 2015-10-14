using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace TrackableData.Sql.Tests
{
    public class TrackableDictionaryTest : IClassFixture<Database>
    {
        private Database _db;

        public TrackableDictionaryTest(Database db)
        {
            _db = db;
        }

        [Fact]
        public async Task Test_SqlMapper_TODO()
        {
        }
    }
}
