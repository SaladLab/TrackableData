namespace TrackableData.Sql.Tests
{
    public interface IItem : ITrackablePoco
    {
        short Kind { get; set; }
        int Count { get; set; }
        string Note { get; set; }
    }

    public class ItemData
    {
        public short Kind { get; set; }
        public int Count { get; set; }
        public string Note { get; set; }
    }
}
