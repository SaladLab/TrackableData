using System;
using System.Threading;
using MongoDB.Bson;

namespace TrackableData.MongoDB
{
    public static class UniqueInt64Id
    {
        private static ulong _baseId;
        private static int _staticIncrement;

        static UniqueInt64Id()
        {
            var refId = ObjectId.GenerateNewId();
            var m = refId.Machine;
            var p = refId.Pid;
            _baseId = ((ulong)((m >> 16) ^ (m >> 8) ^ (m)) << 24) |
                      ((ulong)((p >> 8) ^ (p)) << 16);
        }

        public static void SetMachineNumber(byte number)
        {
            _baseId = (_baseId & 0xFFFFFFFF00FFFFFFUL) | ((ulong)number << 24);
        }

        public static void SetProcessNumber(byte number)
        {
            _baseId = (_baseId & 0xFFFFFFFFFF00FFFFUL) | ((ulong)number << 16);
        }

        // GenerateNewId like MongoDB.ObjectID but use 8 bytes instead of 12 bytes
        // ID = T T T T M P I I (T: Timestamp, M: Machine, P: Process, I: Increment)
        public static long GenerateNewId()
        {
            var increment = (ulong)(Interlocked.Increment(ref _staticIncrement) & 0x0000ffff);
            var timestamp = (ulong)GetTimestampFromDateTime(DateTime.UtcNow);
            return (long)(_baseId | increment | (timestamp << 32));
        }

        private static int GetTimestampFromDateTime(DateTime timestamp)
        {
            var secondsSinceEpoch = (long)Math.Floor((BsonUtils.ToUniversalTime(timestamp) -
                                                      BsonConstants.UnixEpoch).TotalSeconds);
            if (secondsSinceEpoch < int.MinValue || secondsSinceEpoch > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(timestamp));
            }
            return (int)secondsSinceEpoch;
        }
    }
}
