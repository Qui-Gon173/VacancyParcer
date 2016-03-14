using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VacancyParser.PagesLoader
{
    public class TimeWaiter
    {
        private DateTime _dateLastWait;
        private object _lockObject = new object();
        public int TimeToWaitInMs { get; set; }

        public void Wait()
        {
            lock(_lockObject)
            {
                var substract = DateTime.Now.Subtract(_dateLastWait).TotalMilliseconds;
                if(substract<TimeToWaitInMs)
                {
                    var lastTimeToWait = TimeToWaitInMs - (int)substract;
                    System.Threading.Thread.Sleep(lastTimeToWait);
                    _dateLastWait = DateTime.Now;
                }
            }
        }
    }
}
