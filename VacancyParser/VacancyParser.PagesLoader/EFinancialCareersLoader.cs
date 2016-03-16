using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace VacancyParser.PagesLoader
{
    public class EFinancialCareersLoader : PageLoader
    {
        private static EFinancialCareersLoader _instance;

        public static EFinancialCareersLoader Instance
        {
            get
            {
                return _instance ?? (_instance = new EFinancialCareersLoader());
            }
        }

        public override string Link
        {
            get
            {
                return @"http://www.efinancialcareers.com/jobs-Information_Technology.s019";
            }
        }

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
                    .SelectNodes("//*[contains(@class,'jobPreview')]/h2/a")
                    .Select(el => el.GetAttributeValue("href", ""))
                    .Where(el => !string.IsNullOrEmpty(el))
                    .Select(el => new Thread(() => ParceVacancy(el)))
                    .ToArray();
                foreach (var el in threadArray)
                    el.Start();
                while (threadArray.Any(el => el.IsAlive))
                {
                    Thread.Sleep(WaitTime * 2);
                }
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
                    .SelectSingleNode("//*[contains(@class,'well')]/h1")
                    .InnerText;

                var liContent = doc.DocumentNode
                    .SelectNodes("//ul[contains(@class,'details')]/li");
                var deviders = new[] { ' ', ':', '\t', '\r', '\n' };

                for (var i = 0; i < liContent.Count; i++)
                {
                    var span = liContent[i].SelectSingleNode("span");
                    switch (liContent[i].GetAttributeValue("class", ""))
                    {
                        case "salary": result.Salary = RemoveDeviders(span.InnerText, deviders); break;
                        case "location": result.Location = RemoveDeviders(span.InnerText, deviders); break;
                    }
                }
                lock (_loadedData)
                    _loadedData.Enqueue(result);
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
                var textVacancyCount = doc.DocumentNode.SelectNodes("//*[contains(@class,'pagination')]/*[contains(@class,'pageItem')]")
                    .Last()
                    .InnerText;
                var pageCount = int.Parse(textVacancyCount);
                var threads = new Thread[pageCount];
                for (var i = 0; i < pageCount; i++)
                {
                    var link = new StringBuilder(Link);
                    link.AppendFormat("?page={0}", i + 1);
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
