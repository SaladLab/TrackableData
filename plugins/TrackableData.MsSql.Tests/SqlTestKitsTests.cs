using TrackableData.TestKits;
using Xunit;

namespace TrackableData.MsSql.Tests
{
    public class TrackablePocoTest : SqlTestKits.TrackablePocoTest, IClassFixture<Database>
    {
        public TrackablePocoTest(Database db)
            : base(db, Database.SqlProvider)
        {
        }
    }

    public class TrackablePocoWithAutoIdTest : SqlTestKits.TrackablePocoWithAutoIdTest, IClassFixture<Database>
    {
        public TrackablePocoWithAutoIdTest(Database db)
            : base(db, Database.SqlProvider)
        {
        }
    }

    public class TrackablePocoWithHeadKeysTest : SqlTestKits.TrackablePocoWithHeadKeysTest, IClassFixture<Database>
    {
        public TrackablePocoWithHeadKeysTest(Database db)
            : base(db, Database.SqlProvider)
        {
        }
    }

    public class TypeTest : SqlTestKits.TypeTest, IClassFixture<Database>
    {
        public TypeTest(Database db)
            : base(db, Database.SqlProvider)
        {
        }
    }

    public class TypeNullableTest : SqlTestKits.TypeNullableTest,
        IClassFixture<Database>
    {
        public TypeNullableTest(Database db)
            : base(db, Database.SqlProvider)
        {
        }
    }

    public class TrackableDictionaryDataTest : SqlTestKits.TrackableDictionaryDataTest, IClassFixture<Database>
    {
        public TrackableDictionaryDataTest(Database db)
            : base(db, Database.SqlProvider)
        {
        }
    }

    public class TrackableDictionaryDataWithHeadKeysTest : SqlTestKits.TrackableDictionaryDataWithHeadKeysTest,
        IClassFixture<Database>
    {
        public TrackableDictionaryDataWithHeadKeysTest(Database db)
            : base(db, Database.SqlProvider)
        {
        }
    }

    public class TrackableDictionaryStringTest : SqlTestKits.TrackableDictionaryStringTest, IClassFixture<Database>
    {
        public TrackableDictionaryStringTest(Database db)
            : base(db, Database.SqlProvider)
        {
        }
    }

    public class TrackableSetValueTest : SqlTestKits.TrackableSetValueTest, IClassFixture<Database>
    {
        public TrackableSetValueTest(Database db)
            : base(db, Database.SqlProvider)
        {
        }
    }

    public class TrackableSetValueWithHeadKeysTest : SqlTestKits.TrackableSetValueWithHeadKeysTest,
        IClassFixture<Database>
    {
        public TrackableSetValueWithHeadKeysTest(Database db)
            : base(db, Database.SqlProvider)
        {
        }
    }

    public class TrackableContainerTest : SqlTestKits.TrackableContainerTest, IClassFixture<Database>
    {
        public TrackableContainerTest(Database db)
            : base(db, Database.SqlProvider)
        {
        }
    }

    public class TrackableContainerIgnoreTest : SqlTestKits.TrackableContainerIgnoreTest, IClassFixture<Database>
    {
        public TrackableContainerIgnoreTest(Database db)
            : base(db, Database.SqlProvider)
        {
        }
    }
}
