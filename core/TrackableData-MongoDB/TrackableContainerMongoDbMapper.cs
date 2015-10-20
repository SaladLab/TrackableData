using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Reflection;

namespace TrackableData.MongoDB
{
    public class TrackableContainerMongoDbMapper<T>
        where T : ITrackableContainer<T>
    {
        private class PropertyItem
        {
            public string Name;
            public PropertyInfo Property;
            public object Mapper;
            public Func<T, BsonValue> ConvertToBson;
            public Func<BsonValue, T> ConvertToTrackable;
            public Func<UpdateDefinition<BsonDocument>, T, UpdateDefinition<BsonDocument>> SaveChanges;
        }

        private static PropertyItem[] PropertyData;

        static TrackableContainerMongoDbMapper()
        {
            var propertyItems = new List<PropertyItem>();
            foreach (var property in typeof(T).GetProperties())
            {
                propertyItems.Add(new PropertyItem
                {
                    Name = property.Name,
                    Property = property,
                    Mapper = null,
                    ConvertToBson = null,
                    ConvertToTrackable = null,
                    SaveChanges = null,
                });
            }

            // TODO: Get Mapper
            // if (tracker.DataTracker.HasChange)
            //    update = _userDataMapper.BuildUpdatesForSave(update, tracker.DataTracker, "Data");
        }

        public TrackableContainerMongoDbMapper()
        {
        }

        #region Helpers

        public async Task CreateAsync(IMongoCollection<BsonDocument> collection, T value, params object[] keyValues)
        {
            if (keyValues.Length == 0)
                throw new ArgumentException("At least 1 keyValue required.");

            // TODO:
            /*
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
            */
        }

        public Task<int> DeleteAsync(IMongoCollection<BsonDocument> collection, params object[] keyValues)
        {
            return DocumentHelper.DeleteAsync(collection, keyValues);
            //             await this["User"].DeleteOneAsync(Builders<BsonDocument>.Filter.Eq("_id", uid));

        }

        public async Task<T> LoadAsync(IMongoCollection<BsonDocument> collection, params object[] keyValues)
        {
            if (keyValues.Length == 0)
                throw new ArgumentException("At least 1 keyValue required.");

            // TODO:
            /*
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

            */
            return default(T);
        }

        public Task<UpdateResult> SaveAsync(IMongoCollection<BsonDocument> collection,
                                            ITracker tracker,
                                            params object[] keyValues)
        {
            return SaveAsync(collection, (IContainerTracker<T>)tracker, keyValues);
        }

        public Task<UpdateResult> SaveAsync(IMongoCollection<BsonDocument> collection,
                                            IContainerTracker<T> tracker,
                                            params object[] keyValues)
        {
            if (keyValues.Length == 0)
                throw new ArgumentException("At least 1 keyValue required.");

            // TODO:
            /*
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

            */
            return null;
        }

        #endregion
    }
}
