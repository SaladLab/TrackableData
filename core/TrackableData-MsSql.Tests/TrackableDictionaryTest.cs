using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Xunit;

namespace TrackableData.Sql.Tests
{
    public class TrackableDictionaryTest : IClassFixture<Database>
    {
        private Database _db;

        public TrackableDictionaryTest(Database db)
        {
            _db = db;
        }

        private struct Context<TKey, TValue> : IDisposable
        {
            public TrackableDictionaryMsSqlMapper<TKey, TValue> SqlMapper;
            public SqlConnection Connection;

            public void Dispose()
            {
                if (Connection != null)
                {
                    Connection.Dispose();
                    Connection = null;
                }
            }
        }

        private async Task<Context<TKey, TValue>> PrepareAsync<TKey, TValue>(
            ColumnDefinition singleValueColumnDef = null, ColumnDefinition[] headKeyColumnDefs = null)
        {
            var sqlMapper = new TrackableDictionaryMsSqlMapper<TKey, TValue>(
                typeof(TValue).Name, new ColumnDefinition("Id"), singleValueColumnDef, headKeyColumnDefs);
            var connection = _db.Connection;
            await sqlMapper.ResetTableAsync(connection);
            return new Context<TKey, TValue> { SqlMapper = sqlMapper, Connection = connection };
        }

        private TrackableDictionary<int, ItemData> CreateTestInventory(bool withTracker)
        {
            var dict = new TrackableDictionary<int, ItemData>();
            if (withTracker)
                dict.SetDefaultTracker();
            dict.Add(1, new ItemData { Kind = 101, Count = 1, Note = "Handmade Sword" });
            dict.Add(2, new ItemData { Kind = 102, Count = 3, Note = "Lord of Ring" });
            return dict;
        }

        private enum ModificationWayType
        {
            Intrusive,
            IntrusiveAndMark,
            SetNew,
        }

        private void ModifyTestInventory(TrackableDictionary<int, ItemData> dict, ModificationWayType type)
        {
            dict.Remove(1);
            switch (type)
            {
                case ModificationWayType.Intrusive:
                    dict[2].Count -= 1;
                    dict[2].Note = "Destroyed";
                    break;

                case ModificationWayType.IntrusiveAndMark:
                    dict[2].Count -= 1;
                    dict[2].Note = "Destroyed";
                    dict.MarkModify(2);
                    break;

                case ModificationWayType.SetNew:
                    var item = dict[2];
                    dict[2] = new ItemData { Kind = item.Kind, Count = item.Count - 1, Note = "Destroyed" };
                    break;
            }
            dict.Add(3, new ItemData { Kind = 103, Count = 3, Note = "Just Arrived" });
        }

        // Regular Test

        [Fact]
        public async Task Test_SqlMapper_ResetTable()
        {
            using (var ctx = await PrepareAsync<int, ItemData>())
            {
                var dict = await ctx.SqlMapper.LoadAsync(ctx.Connection);
                Assert.Equal(0, dict.Count);
            }
        }

        [Fact]
        public async Task Test_SqlMapper_CreateAndLoad()
        {
            using (var ctx = await PrepareAsync<int, ItemData>())
            {
                var dict = CreateTestInventory(true);

                await ctx.SqlMapper.SaveAsync(ctx.Connection, dict.Tracker);

                var dict2 = await ctx.SqlMapper.LoadAsync(ctx.Connection);
                Assert.Equal(dict.Count, dict2.Count);
                foreach (var item in dict)
                {
                    Assert.Equal(item.Value.Kind, dict2[item.Key].Kind);
                    Assert.Equal(item.Value.Count, dict2[item.Key].Count);
                    Assert.Equal(item.Value.Note, dict2[item.Key].Note);
                }
            }
        }

        [Fact]
        public async Task Test_SqlMapper_Update()
        {
            using (var ctx = await PrepareAsync<int, ItemData>())
            {
                var dict = CreateTestInventory(true);

                await ctx.SqlMapper.SaveAsync(ctx.Connection, dict.Tracker);
                dict.Tracker.Clear();

                ModifyTestInventory(dict, ModificationWayType.SetNew);
                await ctx.SqlMapper.SaveAsync(ctx.Connection, dict.Tracker);

                var dict2 = await ctx.SqlMapper.LoadAsync(ctx.Connection);
                Assert.Equal(dict.Count, dict2.Count);
                foreach (var item in dict)
                {
                    Assert.Equal(item.Value.Kind, dict2[item.Key].Kind);
                    Assert.Equal(item.Value.Count, dict2[item.Key].Count);
                    Assert.Equal(item.Value.Note, dict2[item.Key].Note);
                }
            }
        }

        // With Head Key Columns

        private static readonly ColumnDefinition[] HeadKeyColumnDefs =
        {
            new ColumnDefinition("Head1", typeof(int)),
            new ColumnDefinition("Head2", typeof(string), 100)
        };

        [Fact]
        public async Task Test_SqlMapperWithHead_ResetTable()
        {
            using (var ctx = await PrepareAsync<int, ItemData>(null, HeadKeyColumnDefs))
            {
                var dict = await ctx.SqlMapper.LoadAsync(ctx.Connection);
                Assert.Equal(0, dict.Count);
            }
        }

        [Fact]
        public async Task Test_SqlMapperWithHead_CreateAndLoad()
        {
            using (var ctx = await PrepareAsync<int, ItemData>(null, HeadKeyColumnDefs))
            {
                var dict = CreateTestInventory(true);

                await ctx.SqlMapper.SaveAsync(ctx.Connection, dict.Tracker, 1, "One");

                var dict2 = await ctx.SqlMapper.LoadAsync(ctx.Connection, 1, "One");
                Assert.Equal(dict.Count, dict2.Count);
                foreach (var item in dict)
                {
                    Assert.Equal(item.Value.Kind, dict2[item.Key].Kind);
                    Assert.Equal(item.Value.Count, dict2[item.Key].Count);
                    Assert.Equal(item.Value.Note, dict2[item.Key].Note);
                }
            }
        }

        [Fact]
        public async Task Test_SqlMapperWithHead_Update()
        {
            using (var ctx = await PrepareAsync<int, ItemData>(null, HeadKeyColumnDefs))
            {
                var dict = CreateTestInventory(true);

                await ctx.SqlMapper.SaveAsync(ctx.Connection, dict.Tracker, 1, "One");
                dict.Tracker.Clear();

                ModifyTestInventory(dict, ModificationWayType.SetNew);
                await ctx.SqlMapper.SaveAsync(ctx.Connection, dict.Tracker, 1, "One");

                var dict2 = await ctx.SqlMapper.LoadAsync(ctx.Connection, 1, "One");
                Assert.Equal(dict.Count, dict2.Count);
                foreach (var item in dict)
                {
                    Assert.Equal(item.Value.Kind, dict2[item.Key].Kind);
                    Assert.Equal(item.Value.Count, dict2[item.Key].Count);
                    Assert.Equal(item.Value.Note, dict2[item.Key].Note);
                }
            }
        }

        // With value which has tracker

        [Fact]
        public async Task Test_SqlMapperWithTrackableValue_ResetTable()
        {
            using (var ctx = await PrepareAsync<int, TrackableItem>())
            {
                var dict = await ctx.SqlMapper.LoadAsync(ctx.Connection);
                Assert.Equal(0, dict.Count);
            }
        }

        [Fact]
        public async Task Test_SqlMapperWithTrackableValue_CreateAndLoad()
        {
            using (var ctx = await PrepareAsync<int, TrackableItem>())
            {
                var dict = new TrackableDictionary<int, TrackableItem>();
                dict.SetDefaultTracker();
                dict.Add(1, new TrackableItem { Kind = 101, Count = 1, Note = "Handmade Sword" });
                dict[1].SetDefaultTracker();
                dict.Add(2, new TrackableItem { Kind = 102, Count = 3, Note = "Lord of Ring" });
                dict[2].SetDefaultTracker();

                await ctx.SqlMapper.SaveAsync(ctx.Connection, dict.Tracker);

                var dict2 = await ctx.SqlMapper.LoadAsync(ctx.Connection);
                Assert.Equal(dict.Count, dict2.Count);
                foreach (var item in dict)
                {
                    Assert.Equal(item.Value.Kind, dict2[item.Key].Kind);
                    Assert.Equal(item.Value.Count, dict2[item.Key].Count);
                    Assert.Equal(item.Value.Note, dict2[item.Key].Note);
                }
            }
        }

        [Fact]
        public async Task Test_SqlMapperWithTrackableValue_Update()
        {
            using (var ctx = await PrepareAsync<int, TrackableItem>())
            {
                var dict = new TrackableDictionary<int, TrackableItem>();
                dict.SetDefaultTracker();
                dict.Add(1, new TrackableItem { Kind = 101, Count = 1, Note = "Handmade Sword" });
                dict[1].SetDefaultTracker();
                dict.Add(2, new TrackableItem { Kind = 102, Count = 3, Note = "Lord of Ring" });
                dict[2].SetDefaultTracker();

                await ctx.SqlMapper.SaveAsync(ctx.Connection, dict);
                dict.Tracker.Clear();

                dict.Remove(1);
                dict[2].Count -= 1;
                dict[2].Note = "Destroyed";
                dict.Add(3, new TrackableItem { Kind = 103, Count = 3, Note = "Just Arrived" });
                dict[3].SetDefaultTracker();

                await ctx.SqlMapper.SaveAsync(ctx.Connection, dict);

                var dict2 = await ctx.SqlMapper.LoadAsync(ctx.Connection);
                Assert.Equal(dict.Count, dict2.Count);
                foreach (var item in dict)
                {
                    Assert.Equal(item.Value.Kind, dict2[item.Key].Kind);
                    Assert.Equal(item.Value.Count, dict2[item.Key].Count);
                    Assert.Equal(item.Value.Note, dict2[item.Key].Note);
                }
            }
        }

        // With Value

        private static readonly ColumnDefinition SingleValueColumnDef = new ColumnDefinition("Value", typeof(string));

        [Fact]
        public async Task Test_SqlMapperWithValue_ResetTable()
        {
            using (var ctx = await PrepareAsync<int, string>(SingleValueColumnDef))
            {
                var dict = await ctx.SqlMapper.LoadAsync(ctx.Connection);
                Assert.Equal(0, dict.Count);
            }
        }

        [Fact]
        public async Task Test_SqlMapperWitValue_CreateAndLoad()
        {
            using (var ctx = await PrepareAsync<int, string>(SingleValueColumnDef))
            {
                var dict = new TrackableDictionary<int, string>();
                dict.SetDefaultTracker();
                dict.Add(1, "One");
                dict.Add(2, "Two");
                dict.Add(3, "Three");

                await ctx.SqlMapper.SaveAsync(ctx.Connection, dict.Tracker);

                var dict2 = await ctx.SqlMapper.LoadAsync(ctx.Connection);
                Assert.Equal(dict.Count, dict2.Count);
                foreach (var item in dict)
                {
                    Assert.Equal(item.Value, dict2[item.Key]);
                }
            }
        }

        [Fact]
        public async Task Test_SqlMapperWithValue_Update()
        {
            using (var ctx = await PrepareAsync<int, string>(SingleValueColumnDef))
            {
                var dict = new TrackableDictionary<int, string>();
                dict.SetDefaultTracker();
                dict.Add(1, "One");
                dict.Add(2, "Two");
                dict.Add(3, "Three");

                await ctx.SqlMapper.SaveAsync(ctx.Connection, dict.Tracker);
                dict.Tracker.Clear();

                dict.Remove(1);
                dict[2] = "TwoTwo";
                dict.Add(4, "Four");
                await ctx.SqlMapper.SaveAsync(ctx.Connection, dict.Tracker);

                var dict2 = await ctx.SqlMapper.LoadAsync(ctx.Connection);
                Assert.Equal(dict.Count, dict2.Count);
                foreach (var item in dict)
                {
                    Assert.Equal(item.Value, dict2[item.Key]);
                }
            }
        }
    }
}
