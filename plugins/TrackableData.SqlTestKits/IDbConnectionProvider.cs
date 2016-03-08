using System.Data.Common;

namespace TrackableData.SqlTestKits
{
    public interface IDbConnectionProvider
    {
        DbConnection Connection { get; }
    }
}
