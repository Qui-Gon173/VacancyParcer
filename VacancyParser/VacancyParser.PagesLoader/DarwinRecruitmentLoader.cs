using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using HtmlAgilityPack;
using System.Threading;

namespace VacancyParser.PagesLoader
{
    public class DarwinRecruitmentLoader : PageLoader
    {

        private static DarwinRecruitmentLoader _instance;

        public static DarwinRecruitmentLoader Instance
        {
            get
            {
                return _instance ?? (_instance = new DarwinRecruitmentLoader());
            }
        }

        private const string domain = @"http://www.darwinrecruitment.com";

        public override string Link
        {
            get
            {
                return domain + @"/jobs/job-search-results/";
            }
        }
        protected DarwinRecruitmentLoader() : base() { }
        private void ParceVacancyList(string link, int _try = 0)
        {
            if (_try > 5)
            {
                if (Logger != null)
                    Logger.Debug("Can't load this link:{0}", link);
            }
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(LoadPage(link));
                var threadArray = doc.DocumentNode
                    .SelectNodes("//*[contains(@class,'summary')]")
                    .Select(el => el.SelectSingleNode("h2/a")
                        .GetAttributeValue("href", ""))
                    .Where(el => !string.IsNullOrEmpty(el))
                    .Select(el => new Thread(() => ParceVacancy(domain + el)))
                    .ToArray();
                foreach (var el in threadArray)
                    el.Start();
                while (threadArray.Any(el => el.IsAlive))
                {
                    Thread.Sleep(WaitTime * 2);
                }
                if (Logger != null)
                    Logger.Info("List loaded:{0}", link);
            }
            catch (WebException e)
            {
                if (Logger != null)
                    Logger.Error("{0}){1}:\n{2}\n\n", _try, link, e);
                ParceVacancyList(link, _try++);
            }
            catch (Exception e)
            {
                if (Logger != null)
                    Logger.Error("Another exception in {0}\n{1}\n\n", link, e);
            }
        }

        private void ParceVacancy(string link, int _try = 0)
        {
            if (_try > 5)
            {
                if (Logger != null)
                    Logger.Debug("Can't load this link:{0}", link);
            }
            try
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
                        case "Location": result.Location = dlContent[i + 1].InnerHtml; break;
                        case "Salary": result.Salary = dlContent[i + 1].InnerHtml; break;
                        case "Sector": result.Sector = dlContent[i + 1].InnerHtml; break;
                    }
                }
                lock (_loadedData)
                    _loadedData.Enqueue(result);
                if (Logger != null)
                    Logger.Info("Loaded:{0}", link);
            }
            catch (WebException e)
            {
                if (Logger != null)
                    Logger.Error("{0}){1}:\n{2}\n\n", _try, link, e);
                ParceVacancy(link, _try++);
            }
            catch (Exception e)
            {
                if (Logger != null)
                    Logger.Error("Another exception in {0}\n{1}\n\n", link, e);
            }
        }

        public override void Init()
        {
            try
            {
                if (IsInited)
                    return;
                var doc = new HtmlDocument();
                doc.LoadHtml(LoadPage(Link));
                var strCount = doc.DocumentNode.SelectSingleNode("//*[contains(@class,'results-count')]/strong")
                    .InnerText;
                var vacancyCount = int.Parse(strCount);
                var pagesCount = vacancyCount / 100;
                if (vacancyCount % 100 != 0)
                    pagesCount++;
                var threads = new Thread[vacancyCount];
                for (var i = 0; i < pagesCount; i++)
                {
                    var link = new StringBuilder(Link);
                    link.AppendFormat("?pagesize=100&page={0}", i + 1);
                    threads[i] = new Thread(() => ParceVacancyList(link.ToString()));
                    threads[i].Start();
                }
                while (threads.Any(el => el.IsAlive))
                {
                    Thread.Sleep(WaitTime * 3);
                }
                IsInited = true;
            }
            catch (Exception e)
            {
                if (Logger != null)
                    Logger.Error("Another exception {0}\n\n", e);
            }
        }
    }
}
