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

        private static TrackableDictionaryMsSqlMapper<byte, UserTeam> _userTeamMapper =
            new TrackableDictionaryMsSqlMapper<byte, UserTeam>(
                "tblTeam",
                new ColumnDefinition("TeamId"),
                new[] { new ColumnDefinition("Uid", typeof(int)) });

        private static TrackableDictionaryMsSqlMapper<int, UserTank> _userTanksMapper =
            new TrackableDictionaryMsSqlMapper<int, UserTank>(
                "tblTank",
                new ColumnDefinition("TankId"),
                new[] { new ColumnDefinition("Uid", typeof(int)) });

        private static TrackableDictionaryMsSqlMapper<byte, long> _userCardMapper =
            new TrackableDictionaryMsSqlMapper<byte, long>(
                "tblCard",
                new ColumnDefinition("GroupNo"),
                new ColumnDefinition("States"),
                new[] { new ColumnDefinition("Uid", typeof(int)) });

        private static TrackableDictionaryMsSqlMapper<int, UserFriend> _userFriendMapper =
            new TrackableDictionaryMsSqlMapper<int, UserFriend>(
                "tblFriend",
                new ColumnDefinition("FriendUid"),
                new[] { new ColumnDefinition("Uid", typeof(int)) });

        private static TrackableDictionaryMsSqlMapper<byte, UserMission> _userMissionMapper =
            new TrackableDictionaryMsSqlMapper<byte, UserMission>(
                "tblMission",
                new ColumnDefinition("SlotId"),
                new[] { new ColumnDefinition("Uid", typeof(int)) });

        private static TrackableDictionaryMsSqlMapper<byte, long> _userStageGradeMapper =
            new TrackableDictionaryMsSqlMapper<byte, long>(
                "tblStageGrade",
                new ColumnDefinition("GroupNo"),
                new ColumnDefinition("Grades"),
                new[] { new ColumnDefinition("Uid", typeof(int)) });

        private static TrackableDictionaryMsSqlMapper<int, UserPost> _userPostMapper =
            new TrackableDictionaryMsSqlMapper<int, UserPost>(
                "tblPost",
                new ColumnDefinition("PostId"),
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
                Items = await _userItemMapper.LoadAsync(_connection, uid),
                Teams = await _userTeamMapper.LoadAsync(_connection, uid),
                Tanks = await _userTanksMapper.LoadAsync(_connection, uid),
                Cards = await _userCardMapper.LoadAsync(_connection, uid),
                Friends = await _userFriendMapper.LoadAsync(_connection, uid),
                Missions = await _userMissionMapper.LoadAsync(_connection, uid),
                StageGrades = await _userStageGradeMapper.LoadAsync(_connection, uid),
                Posts = await _userPostMapper.LoadAsync(_connection, uid),
            };
        }
    }
}
