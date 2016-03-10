using System;
using System.Data.Common;
using System.Threading.Tasks;
using Model;
using TrackableData.MsSql;
using TrackableData.Sql;

namespace MigrationTest
{
    public class SqlDriver
    {
        private readonly TrackableContainerSqlMapper<IUser> _userMapper;
        private readonly DbConnection _connection;

        public SqlDriver(ISqlProvider sqlProvider, DbConnection connection)
        {
            var HeadKeyColumnDefs = new[] { new ColumnDefinition("Uid", typeof(int)) };

            _userMapper = new TrackableContainerSqlMapper<IUser>(
                MsSqlProvider.Instance,
                new[]
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
                        new ColumnDefinition("MissionId"),
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

            _connection = connection;
        }

        public Task CreateUserAsync(int uid, TrackableUser user)
        {
            return _userMapper.CreateAsync(_connection, user, uid);
        }

        public Task DeleteUserAsync(int uid)
        {
            return _userMapper.DeleteAsync(_connection, uid);
        }

        public async Task<TrackableUser> LoadUserAsync(int uid)
        {
            return (TrackableUser)(await _userMapper.LoadAsync(_connection, uid));
        }

        public Task SaveUserAsync(int uid, TrackableUserTracker tracker)
        {
            return _userMapper.SaveAsync(_connection, tracker, uid);
        }
    }
}
