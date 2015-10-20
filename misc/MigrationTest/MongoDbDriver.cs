using System.Data.SqlClient;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using Model;
using MongoDB.Bson;
using MongoDB.Driver;
using TrackableData;
using TrackableData.MongoDB;

namespace MigrationTest
{
    class MongoDbDriver
    {
        private static TrackablePocoMongoDbMapper<IUserData> _userDataMapper =
            new TrackablePocoMongoDbMapper<IUserData>();

        private static TrackableDictionaryMongoDbMapper<int, UserItem> _userItemMapper =
            new TrackableDictionaryMongoDbMapper<int, UserItem>();

        private static TrackableDictionaryMongoDbMapper<byte, UserTeam> _userTeamMapper =
            new TrackableDictionaryMongoDbMapper<byte, UserTeam>();

        private static TrackableDictionaryMongoDbMapper<int, UserTank> _userTankMapper =
            new TrackableDictionaryMongoDbMapper<int, UserTank>();

        private static TrackableDictionaryMongoDbMapper<byte, long> _userCardMapper =
            new TrackableDictionaryMongoDbMapper<byte, long>();

        private static TrackableDictionaryMongoDbMapper<int, UserFriend> _userFriendMapper =
            new TrackableDictionaryMongoDbMapper<int, UserFriend>();

        private static TrackableDictionaryMongoDbMapper<byte, UserMission> _userMissionMapper =
            new TrackableDictionaryMongoDbMapper<byte, UserMission>();

        private static TrackableDictionaryMongoDbMapper<byte, long> _userStageGradeMapper =
            new TrackableDictionaryMongoDbMapper<byte, long>();

        private static TrackableDictionaryMongoDbMapper<int, UserPost> _userPostMapper =
            new TrackableDictionaryMongoDbMapper<int, UserPost>();

        private readonly MongoClient _client;
        private readonly IMongoDatabase _database;

        public MongoDbDriver(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            _client = new MongoClient(builder.DataSource);
            _database = _client.GetDatabase(builder.InitialCatalog);
        }

        public IMongoDatabase Database => _database;

        public IMongoCollection<BsonDocument> this[string name]
        {
            get { return _database.GetCollection<BsonDocument>(name); }
        }

        public async Task CreateUserAsync(int uid, TrackableUserContext user)
        {
            var bson = new BsonDocument();
            bson.Add("_id", uid);
            bson.Add("Data", _userDataMapper.ConvertToBsonDocument(user.Data));
            bson.Add("Items", _userItemMapper.ConvertToBsonDocument(user.Items));
            bson.Add("Teams", _userTeamMapper.ConvertToBsonDocument(user.Teams));
            bson.Add("Tanks", _userTankMapper.ConvertToBsonDocument(user.Tanks));
            bson.Add("Cards", _userCardMapper.ConvertToBsonDocument(user.Cards));
            bson.Add("Friends", _userFriendMapper.ConvertToBsonDocument(user.Friends));
            bson.Add("Missions", _userMissionMapper.ConvertToBsonDocument(user.Missions));
            bson.Add("StageGrades", _userStageGradeMapper.ConvertToBsonDocument(user.StageGrades));
            bson.Add("Posts", _userPostMapper.ConvertToBsonDocument(user.Posts));

            await this["User"].InsertOneAsync(bson);
        }

        public async Task RemoveUserAsync(int uid)
        {
            await this["User"].DeleteOneAsync(Builders<BsonDocument>.Filter.Eq("_id", uid));
        }

        public async Task<TrackableUserContext> LoadUserAsync(int uid)
        {
            var doc = await this["User"].Find(Builders<BsonDocument>.Filter.Eq("_id", uid))
                                        .FirstOrDefaultAsync();
            if (doc == null)
                return null;

            var user = new TrackableUserContext();
            user.Data = (TrackableUserData)_userDataMapper.ConvertToTrackablePoco(doc, "Data") ?? new TrackableUserData();
            user.Items = _userItemMapper.ConvertToTrackableDictionary(doc, "Items")
                         ?? new TrackableDictionary<int, UserItem>();
            user.Teams = _userTeamMapper.ConvertToTrackableDictionary(doc, "Teams")
                         ?? new TrackableDictionary<byte, UserTeam>();
            user.Tanks = _userTankMapper.ConvertToTrackableDictionary(doc, "Tanks")
                         ?? new TrackableDictionary<int, UserTank>();
            user.Cards = _userCardMapper.ConvertToTrackableDictionary(doc, "Cards")
                         ?? new TrackableDictionary<byte, long>();
            user.Friends = _userFriendMapper.ConvertToTrackableDictionary(doc, "Friends")
                           ?? new TrackableDictionary<int, UserFriend>();
            user.Missions = _userMissionMapper.ConvertToTrackableDictionary(doc, "Missions")
                            ?? new TrackableDictionary<byte, UserMission>();
            user.StageGrades = _userStageGradeMapper.ConvertToTrackableDictionary(doc, "StageGrades")
                               ?? new TrackableDictionary<byte, long>();
            user.Posts = _userPostMapper.ConvertToTrackableDictionary(doc, "Posts")
                         ?? new TrackableDictionary<int, UserPost>();
            return user;
        }

        public async Task SaveUserAsync(int uid, TrackableUserContextTracker tracker)
        {
            if (tracker.HasChange == false)
                return;

            UpdateDefinition<BsonDocument> update = null;
            if (tracker.DataTracker.HasChange)
                update = _userDataMapper.BuildUpdatesForSave(update, tracker.DataTracker, "Data");
            if (tracker.ItemsTracker.HasChange)
                update = _userItemMapper.BuildUpdatesForSave(update, tracker.ItemsTracker, "Items");
            if (tracker.TeamsTracker.HasChange)
                update = _userTeamMapper.BuildUpdatesForSave(update, tracker.TeamsTracker, "Teams");
            if (tracker.TanksTracker.HasChange)
                update = _userTankMapper.BuildUpdatesForSave(update, tracker.TanksTracker, "Tanks");
            if (tracker.CardsTracker.HasChange)
                update = _userCardMapper.BuildUpdatesForSave(update, tracker.CardsTracker, "Cards");
            if (tracker.FriendsTracker.HasChange)
                update = _userFriendMapper.BuildUpdatesForSave(update, tracker.FriendsTracker, "Friends");
            if (tracker.MissionsTracker.HasChange)
                update = _userMissionMapper.BuildUpdatesForSave(update, tracker.MissionsTracker, "Missions");
            if (tracker.StageGradesTracker.HasChange)
                update = _userStageGradeMapper.BuildUpdatesForSave(update, tracker.StageGradesTracker, "StageGrades");
            if (tracker.PostsTracker.HasChange)
                update = _userPostMapper.BuildUpdatesForSave(update, tracker.PostsTracker, "Posts");

            await this["User"].UpdateOneAsync(
                Builders<BsonDocument>.Filter.Eq("_id", uid),
                update);
        }
    }
}
