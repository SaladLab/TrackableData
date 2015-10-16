using MongoDB.Bson;

namespace TrackableData.MongoDB.Tests
{
    public interface IPerson : ITrackablePoco
    {
        ObjectId Id { get; set; }
        string Name { get; set; }
        int Age { get; set; }
    }
}

