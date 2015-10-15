using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackableData;

namespace Basic.Data
{
    public interface IUserData : ITrackablePoco
    {
        string Name { get; set; }
        int Gold { get; set; }
        int Level { get; set; }
        IUserHandData LeftHand { get; set; }
        IUserHandData RightHand { get; set; }
    }

    public interface IUserHandData : ITrackablePoco
    {
        int FingerCount { get; set; }
        bool Dirty { get; set; }
    }
}
