using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models
{
    public sealed class CountDown:IStored
    {
        public TimeSpan TargetCount
        { get; set; }
        

        public TimeSpan CurrentCount
        {
            get; set;
            } 

        public String Name { get; set; }

        public int Index { get; set; }

        public String TargetCountMMSS
        {
            get
            {
                int minutedisp = TargetCount.Minutes;
                minutedisp = (TargetCount.Hours * 60) + minutedisp;
                return minutedisp.ToString("00") + ":" + TargetCount.Seconds.ToString("00");
            }
        }

        public String CurrentCountMMSS
        {
            get
            {
                int minutedisp = CurrentCount.Minutes;
                minutedisp = (CurrentCount.Hours * 60) + minutedisp;
                return minutedisp.ToString("00") + ":" + CurrentCount.Seconds.ToString("00");
                //return CurrentCount.Minutes.ToString("00") + ":" + CurrentCount.Seconds.ToString("00");

            }
        }

        public CountDown()
        {
            CurrentCount = TargetCount;
        }

    }

    public enum CountDownStatus
    {
        Reset,
        Started,
        Stopped
    }
}
