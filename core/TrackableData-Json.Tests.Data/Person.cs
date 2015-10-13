using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackableData.Json.Tests.Data
{
    public interface IPerson : ITrackablePoco
    {
        string Name { get; set; }
        int Age { get; set; }
        IHand LeftHand { get; set; }
        IHand RightHand { get; set; }
    }

    public interface IHand : ITrackablePoco
    {
        IRing MainRing { get; set; }
        IRing SubRing { get; set; }
    }

    public interface IRing : ITrackablePoco
    {
        string Name { get; set; }
        int Power { get; set; }
    }
}
