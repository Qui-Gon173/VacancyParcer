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
        public virtual string Link
        {
            get
            {
                return "";
            }
        }
        public virtual int WaitTime { get; set; }

        static PageLoader()
        {
            TimeWaiter = new TimeWaiter();
        }

        protected string LoadPage(string link)
        {
            string page;
            using (var client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                TimeWaiter.Wait();
                page = client.DownloadString(link);
            }
            return page;
        }

        protected static string RemoveDeviders(string str,params char[] deviders)
        {
            var res = str;
            foreach(var ch in deviders)
                res=res.Replace(ch.ToString(),"");
            return res;
        }

        public abstract void Init();
        public abstract VacancyData[] GetVacancy();
    }
}
