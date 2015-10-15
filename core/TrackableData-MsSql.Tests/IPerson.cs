using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackableData.Sql.Tests
{
    public interface IPerson : ITrackablePoco
    {
        [TrackableField("sql.primary-key")] int Id { get; set; }
        string Name { get; set; }
        int Age { get; set; }
    }

    public interface IPersonWithIdentity : ITrackablePoco
    {
        [TrackableField("sql.primary-key", "sql.identity")] int Id { get; set; }
        string Name { get; set; }
        int Age { get; set; }
    }
}
