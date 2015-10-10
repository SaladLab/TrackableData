using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackableData;

namespace Basic.Data
{
    public class UserData : ITrackablePoco
    {
        public virtual string Name { get; set; }
        public virtual int Gold { get; set; }
        public virtual int Level { get; set; }
        public virtual UserHandData LeftHand { get; set; }
        public virtual UserHandData RightHand { get; set; }
    }

    public class UserHandData : ITrackablePoco
    {
        public virtual int FingerCount { get; set; }
        public virtual bool Dirty { get; set; }
    }
}
