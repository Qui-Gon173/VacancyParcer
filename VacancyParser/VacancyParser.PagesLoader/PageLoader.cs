using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace VacancyParser.PagesLoader
{
    public abstract class PageLoader
    {
        
        protected static TimeWaiter TimeWaiter;
        protected static bool IsInited;
        public virtual string Link { get; }
        public virtual int WaitTime { get; set; }

        static PageLoader()
        {
            TimeWaiter = new TimeWaiter();
        }

        protected string LoadPage(string link, Dictionary<string, string> args = null)
        {
            string page;
            using (var client = new WebClient())
            {
                if (args != null)
                    foreach (var pair in args)
                        client.QueryString.Add(pair.Key, pair.Value);
                TimeWaiter.Wait();
                page = client.DownloadString(link);
            }
            return page;
        }
        public abstract void Init();
        public abstract VacancyData[] GetVacancy();
    }
}
