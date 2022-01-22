using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models
{
    public class TempSensor : IStored
    {
        public string SensorId { get; set; }
        
        public string Name { get; set; }

        public bool DefaultSelected { get; set; }

        public SensorState State { get; set; }
    }

    public enum SensorState
    {
        New,
        Known,
        Deleted
    }
    //public class TempSensorReading
    //{
    //    public string SensorId { get; set; }

    //    public DateTime ReadingTime { get; set; }
    //    public double Reading { get; set; }
    //}

}
