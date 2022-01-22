using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models
{
    public sealed class PIDParameter : IStored
    {
        //kp, ki, kd;
        public PIDState State { get; set; }
        public string Name { get; set; }
        public double Kp { get; set; }
        public double Ki { get; set; }
        public double Kd { get; set; }
        public double Target { get; set; }

        public int WindowSize { get; set; }

        public double LastValue { get; set; }

        public double LastOutput { get; set; }
    }

    public class PIDOperationalParameters
    {
        public PIDParameter Params { get; set; }
        public PIDState State { get; set; }

        public bool ElementState { get; set; }
    }

    public enum PIDState
    {
        Started,
        Stopped
    }

}
