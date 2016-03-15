using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using HtmlAgilityPack;
using System.Threading;

namespace VacancyParser.PagesLoader
{
    public class DarwinRecruitmentLoader:PageLoader
    {
        private Queue<VacancyData> _loadedData = new Queue<VacancyData>();
        private const string domain = @"http://www.darwinrecruitment.com";
        public override int WaitTime
        {
            get
            {
                return TimeWaiter.TimeToWaitInMs;
            }
            set
            {
                TimeWaiter.TimeToWaitInMs = value;
            }
        }
        public override string Link
        {
            get
            {
                return domain+@"/jobs/job-search-results/";
            }
        }

        static DarwinRecruitmentLoader()
        {
            TimeWaiter = new TimeWaiter();
        }

        private void ParceVacancyList(string link)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(LoadPage(link));
            var threadArray=doc.DocumentNode
                .SelectNodes("//*[contains(@class,'summary')]")
                .Select(el => el.SelectSingleNode("h2/a")
                    .GetAttributeValue("href", ""))
                .Where(el=>!string.IsNullOrEmpty(el))
                .Select(el=> new Thread(()=>ParceVacancy(domain+el)))
                .ToArray();
            foreach (var el in threadArray)
                el.Start();
            while(threadArray.Any(el=>el.IsAlive))
            {
                Thread.Sleep(WaitTime * 2);
            }
        }

        private void ParceVacancy(string link)
        {
            var result = new VacancyData();
            var doc = new HtmlDocument();
            doc.LoadHtml(LoadPage(link));
            
            result.Job = doc.DocumentNode
                .SelectSingleNode("//*[contains(@class,'content')]/h1")
                .InnerText;

            var dlContent = doc.DocumentNode
                .SelectSingleNode("//*[contains(@class,'job-header')]/dl")
                .SelectNodes("dt|dd");
            var deviders = new[] { ' ', ':', '\t', '\r', '\n' };
            for (var i = 0; i < dlContent.Count; i += 2)
            {
                var InnerText = RemoveDeviders(dlContent[i].InnerText, deviders);
                switch (InnerText)
                {
                    case "Location": result.Location = RemoveDeviders(dlContent[i + 1].InnerText, deviders); break;
                    case "Salary": result.Salary = RemoveDeviders(dlContent[i + 1].InnerText, deviders); break;
                    case "Sector": result.Sector = RemoveDeviders(dlContent[i + 1].InnerText, deviders); break;
                }
            }
            lock (_loadedData)
                _loadedData.Enqueue(result);
        }

        public override void Init()
        {
            if (IsInited)
                return;
            var doc = new HtmlDocument();
            doc.LoadHtml(LoadPage(Link));
            var strCount = doc.DocumentNode.SelectSingleNode("//*[contains(@class,'results-count')]/strong")
                .InnerText;
            var vacancyCount =int.Parse(strCount);
            var pagesCount = vacancyCount / 100;
            if (vacancyCount % 100 != 0)
                pagesCount++;
            var threads = new Thread[vacancyCount];
            for (var i = 0; i < pagesCount; i++)
            {
                var link=new StringBuilder(Link);
                link.AppendFormat("?pagesize=100&page={0}",i+1);
                threads[i] = new Thread(()=>ParceVacancyList(link.ToString()));
                threads[i].Start();
            }
            while (threads.Any(el => el.IsAlive))
            {
                Thread.Sleep(WaitTime*3);
            }
            IsInited = true;
        }

        public override VacancyData[] GetVacancy()
        {
            return _loadedData.ToArray();
        }
    }
}
