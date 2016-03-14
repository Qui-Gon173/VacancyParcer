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
                return domain+@"http://www.darwinrecruitment.com/jobs/job-search-results/";
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
                .SelectNodes(".summary")
                .Select(el => el.SelectSingleNode("h2")
                    .SelectSingleNode("a")
                    .GetAttributeValue("href",""))
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
                .SelectSingleNode(".content")
                .SelectSingleNode("h1")
                .InnerText;
                
            var dlContent=doc.DocumentNode
                .SelectSingleNode(".job-header")
                .SelectSingleNode("dl")
                .ChildNodes;
            
            for(var i=0;i<dlContent.Count;i+=2)
            {
                switch(dlContent[i].InnerText.Trim(' ',':','\t'))
                {
                    case "Location:": result.Location = dlContent[i + 1].InnerText.Trim(' ', ':', '\t'); break;
                    case "Salary": result.Salary=dlContent[i+1].InnerText.Trim(' ',':','\t'); break;
                    case "Sector": result.Sector=dlContent[i+1].InnerText.Trim(' ',':','\t'); break;
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
            var strCount = doc.DocumentNode.SelectSingleNode(".results-count")
                .ChildNodes
                .Single(el => el.Name == "strong")
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
