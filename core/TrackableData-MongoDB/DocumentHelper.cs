using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace TrackableData.MongoDB
{
    public static class DocumentHelper
    {
        // When T has id
        //
        //   O: T.id -> { T }
        //      k[0] -> { k[1]: { T.id: { ... } } } with keyValues
        //
        //   X: objectid -> { T }
        //      k[0] -> { k[1]: { ... } } with keyValues

        public static string ToDotPath(IEnumerable<object> keys)
        {
            return string.Join(".", keys.Select(x => x.ToString()));
        }

        public static string ToDotPathWithTrailer(IEnumerable<object> keys)
        {
            var path = string.Join(".", keys.Select(x => x.ToString()));
            return path.Length > 0 ? path + "." : path;
        }

        public static string CombineDotPath(string a, string b)
        {
            if (string.IsNullOrEmpty(a))
                return b;
            if (string.IsNullOrEmpty(b))
                return a;
            if (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b))
                return string.Empty;
            else
                return a + "." + b;
        }

        public static BsonValue QueryValue(BsonDocument doc, IEnumerable<object> keys)
        {
            if (doc == null)
                return null;

            BsonValue curDoc = doc;
            foreach (var key in keys)
            {
                if (curDoc.IsBsonDocument == false)
                    return null;

                BsonValue subValue;
                if (curDoc.AsBsonDocument.TryGetValue(key.ToString(), out subValue) == false)
                    return null;

                curDoc = subValue;
            }

            return curDoc;
        }

        public static async Task<int> RemoveAsync(IMongoCollection<BsonDocument> collection, params object[] keyValues)
        {
            if (keyValues.Length == 0)
                throw new ArgumentException("At least 1 keyValue required.");

            if (keyValues.Length == 1)
            {
                var ret = await collection.DeleteOneAsync(Builders<BsonDocument>.Filter.Eq("_id", keyValues[0]));
                return ret != null ? (int)ret.DeletedCount : 0;
            }
            else
            {
                var keyPath = ToDotPath(keyValues.Skip(1));
                var ret = await collection.UpdateOneAsync(Builders<BsonDocument>.Filter.Eq("_id", keyValues[0]),
                                                          Builders<BsonDocument>.Update.Unset(keyPath));
                return ret != null ? (int)ret.ModifiedCount : 0;
            }
        }
    }
}
