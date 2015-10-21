using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Model;
using TrackableData.MsSql;

namespace MigrationTest
{
    public class MsSqlDriver
    {
        private static readonly ColumnDefinition[] HeadKeyColumnDefs =
        {
            new ColumnDefinition("Uid", typeof(int)),
        };

        private static TrackableContainerMsSqlMapper<IUserContext> _userMapper =
            new TrackableContainerMsSqlMapper<IUserContext>(new[]
            {
                Tuple.Create("Data", new object[]
                {
                    "tblUser",
                    HeadKeyColumnDefs
                }),
                Tuple.Create("Items", new object[]
                {
                    "tblItem",
                    new ColumnDefinition("ItemId"),
                    HeadKeyColumnDefs
                }),
                Tuple.Create("Teams", new object[]
                {
                    "tblTeam",
                    new ColumnDefinition("TeamId"),
                    HeadKeyColumnDefs
                }),
                Tuple.Create("Tanks", new object[]
                {
                    "tblTank",
                    new ColumnDefinition("TankId"),
                    HeadKeyColumnDefs
                }),
                Tuple.Create("Cards", new object[]
                {
                    "tblCard",
                    new ColumnDefinition("GroupNo"),
                    new ColumnDefinition("States"),
                    HeadKeyColumnDefs
                }),
                Tuple.Create("Friends", new object[]
                {
                    "tblFriend",
                    new ColumnDefinition("FriendUid"),
                    HeadKeyColumnDefs
                }),
                Tuple.Create("Missions", new object[]
                {
                    "tblMission",
                    new ColumnDefinition("SlotId"),
                    HeadKeyColumnDefs
                }),
                Tuple.Create("StageGrades", new object[]
                {
                    "tblStageGrade",
                    new ColumnDefinition("GroupNo"),
                    new ColumnDefinition("Grades"),
                    HeadKeyColumnDefs
                }),
                Tuple.Create("Posts", new object[]
                {
                    "tblPost",
                    new ColumnDefinition("PostId"),
                    HeadKeyColumnDefs
                }),
            });

        private readonly SqlConnection _connection;

        public MsSqlDriver(string connectionString)
        {
            _connection = new SqlConnection(connectionString);
            _connection.Open();
        }

        public Task CreateUserAsync(int uid, TrackableUserContext user)
        {
            return _userMapper.CreateAsync(_connection, user, uid);
        }

        public Task DeleteUserAsync(int uid)
        {
            return _userMapper.DeleteAsync(_connection, uid);
        }

        public async Task<TrackableUserContext> LoadUserAsync(int uid)
        {
            return (TrackableUserContext)(await _userMapper.LoadAsync(_connection, uid));
        }

        public Task SaveUserAsync(int uid, TrackableUserContextTracker tracker)
        {
            return _userMapper.SaveAsync(_connection, tracker, uid);
        }
    }
}
