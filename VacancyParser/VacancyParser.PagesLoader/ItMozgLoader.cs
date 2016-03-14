using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace VacancyParser.PagesLoader
{
    public class ItMozgLoader:PageLoader
    {
        private Queue<VacancyData> _loadedData = new Queue<VacancyData>();
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
                return @"http://itmozg.ru/search/vacancy/";
            }
        }

        private void ParceVacancyList(string link)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(LoadPage(link));
            var threadArray = doc.DocumentNode
                .SelectNodes(".vac>a.vacancy")
                .Select(el => new { 
                    href = el.GetAttributeValue("href", ""), 
                    title = el.SelectSingleNode("span").InnerText.Trim() 
                })
                .Where(el => !string.IsNullOrEmpty(el.href)&&!string.IsNullOrEmpty(el.title))
                .Select(el => new Thread(() => ParceVacancy(el.href,el.title)))
                .ToArray();
            foreach (var el in threadArray)
                el.Start();
            while(threadArray.Any(el=>el.IsAlive))
            {
                Thread.Sleep(WaitTime * 2);
            }
        }

        private void ParceVacancy(string link,string title)
        {
            var result = new VacancyData();
            var doc = new HtmlDocument();
            doc.LoadHtml(LoadPage(link));
            result.Job = title;

            var spesiality = "Специализация";
            var experiance = "Описание работы";

            var text = doc.DocumentNode
                .SelectSingleNode(".listing-body>.fleft")
                .InnerText;
            var start = text.IndexOf(spesiality) + spesiality.Length;
            result.Skils = text.Substring(start, text.IndexOf(experiance) - start);

            var tableContent = doc.DocumentNode
                .SelectSingleNode(".listing-summary");
            var header=tableContent.SelectSingleNode("thead").SelectSingleNode("tr").ChildNodes;
            var tbody = tableContent.SelectSingleNode("tbody").SelectSingleNode("tr").ChildNodes;
            if (header.Count != tbody.Count)
                return;

            for (var i = 0; i < header.Count; i ++)
            {
                switch (header[i].InnerText.Trim(' ', ':', '\t'))
                {
                    case "Доход:": result.Salary = tbody[i].InnerText.Trim(' ', ':', '\t'); break;
                    case "Город:": result.Location = tbody[i].InnerText.Trim(' ', ':', '\t'); break;
                    case "Требуемый опыт работы": result.Experiance = tbody[i].InnerText.Trim(' ', ':', '\t'); break;
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
            var strCount = doc.DocumentNode.SelectSingleNode(".title_vacancy")
                .SelectSingleNode(".context>.context")
                .InnerText;
            var vacancyCount =int.Parse(strCount);
            var pagesCount = vacancyCount / 20;
            if (vacancyCount % 20 != 0)
                pagesCount++;
            var threads = new Thread[vacancyCount];
            for (var i = 0; i < pagesCount; i++)
            {
                var link=new StringBuilder(Link);
                link.AppendFormat("?page={0}",i+1);
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
