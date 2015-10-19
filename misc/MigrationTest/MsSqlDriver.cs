using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Model;
using TrackableData.MsSql;

namespace MigrationTest
{
    public class MsSqlDriver
    {
        private static TrackablePocoMsSqlMapper<IUserData> _userDataMapper =
            new TrackablePocoMsSqlMapper<IUserData>(
                "tblUser",
                new[] { new ColumnDefinition("Uid", typeof(int)) });

        private static TrackableDictionaryMsSqlMapper<int, UserItem> _userItemMapper =
            new TrackableDictionaryMsSqlMapper<int, UserItem>(
                "tblItem",
                new ColumnDefinition("ItemId"),
                new[] { new ColumnDefinition("Uid", typeof(int)) });

        private readonly SqlConnection _connection;

        public MsSqlDriver(string connectionString)
        {
            _connection = new SqlConnection(connectionString);
            _connection.Open();
        }

        public async Task<TrackableUserContext> LoadUser(int uid)
        {
            return new TrackableUserContext
            {
                Data = (TrackableUserData)(await _userDataMapper.LoadAsync(_connection, uid)),
                Items = await _userItemMapper.LoadAsync(_connection, uid)
            };
        }
    }
}
