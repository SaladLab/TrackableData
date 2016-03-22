using System.Collections.Generic;
using ProtoBuf.Meta;
using Xunit;

namespace TrackableData.Protobuf.Tests
{
    public class TrackableSetTest
    {
        private TrackableSet<int> CreateTestSet()
        {
            return new TrackableSet<int>() { 1, 2, 3 };
        }

        private TrackableSet<int> CreateTestSetWithTracker()
        {
            var set = CreateTestSet();
            set.SetDefaultTrackerDeep();
            return set;
        }

        private TypeModel CreateTypeModel()
        {
            var model = TypeModel.Create();
            model.Add(typeof(TrackableSetTracker<int>), false)
                 .SetSurrogate(typeof(TrackableSetTrackerSurrogate<int>));
            return model;
        }

        [Fact]
        public void Test_TrackableSet_Serialize_Work()
        {
            var set = CreateTestSetWithTracker();

            var typeModel = CreateTypeModel();
            var set2 = (IEnumerable<int>)typeModel.DeepClone(set);

            Assert.Equal(new HashSet<int> { 1, 2, 3 }, set2);
        }

        [Fact]
        public void Test_TrackableSetTracker_Serialize_Work()
        {
            var set = CreateTestSetWithTracker();
            set.Remove(1);
            set.Remove(2);
            set.Add(4);
            set.Add(5);

            var typeModel = CreateTypeModel();
            var tracker2 = (TrackableSetTracker<int>)typeModel.DeepClone(set.Tracker);

            var set2 = CreateTestSet();
            tracker2.ApplyTo(set2);

            Assert.Equal(new HashSet<int> { 3, 4, 5 }, set2);
        }
    }
}
