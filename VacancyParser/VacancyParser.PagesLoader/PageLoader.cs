using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VacancyParser.PagesLoader
{
    public abstract class PageLoader
    {
        private static TimeWaiter TimeWaiter;
        public string Link { get; protected set; }
        public int WaitTime { get; set; }

        public abstract void Init();
        public abstract VacancyData[] GetVacancy();
    }
}
