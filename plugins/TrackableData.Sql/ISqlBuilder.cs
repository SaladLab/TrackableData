using System;

namespace TrackableData.Sql
{
    public interface ISqlBuilder
    {
        // ex: id -> [id]
        string EscapeId(string id);
    }
}
