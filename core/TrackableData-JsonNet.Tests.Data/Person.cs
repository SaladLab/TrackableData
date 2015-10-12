using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackableData.JsonNet.Tests.Data
{
    public class Person : ITrackablePoco
    {
        public virtual string Name { get; set; }
        public virtual int Age { get; set; }
        public virtual Hand LeftHand { get; set; }
        public virtual Hand RightHand { get; set; }
    }

    public class Hand : ITrackablePoco
    {
        public virtual Ring MainRing { get; set; }
        public virtual Ring SubRing { get; set; }
    }

    public class Ring : ITrackablePoco
    {
        public virtual string Name { get; set; }
        public virtual int Power { get; set; }
    }
}
