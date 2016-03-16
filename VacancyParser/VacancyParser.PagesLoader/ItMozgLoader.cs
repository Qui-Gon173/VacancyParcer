using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace VacancyParser.PagesLoader
{
    public class ItMozgLoader : PageLoader
    {

        private static DarwinRecruitmentLoader _instance;

        public static DarwinRecruitmentLoader Instance
        {
            get
            {
                return _instance ?? (_instance = new DarwinRecruitmentLoader());
            }
        }

        public override string Link
        {
            get
            {
                return @"http://itmozg.ru/search/vacancy/";
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
                    .SelectNodes("//*[contains(@class,'vac')]/a[contains(@class,'vacancy')]")
                    .Select(el => new
                    {
                        href = el.GetAttributeValue("href", ""),
                        title = el.SelectSingleNode("span").InnerText.Trim()
                    })
                    .Where(el => !string.IsNullOrEmpty(el.href) && !string.IsNullOrEmpty(el.title))
                    .Select(el => new Thread(() => ParceVacancy(el.href, el.title)))
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

        private void ParceVacancy(string link, string title, int _try = 0)
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
                result.Job = title;

                var spesiality = "Специализация";
                var experiance = "Описание работы";

                var text = doc.DocumentNode
                    .SelectSingleNode("//*[contains(@class,'listing-body')]/*[contains(@class,'fleft')]")
                    .InnerText;
                var start = text.IndexOf(spesiality) + spesiality.Length;
                result.Skils = text.Substring(start, text.IndexOf(experiance) - start).Trim(':', '\t', '\n', '\r');

                var tableContent = doc.DocumentNode
                    .SelectSingleNode("//*[contains(@class,'listing-summary')]");
                var header = tableContent.SelectNodes("thead/tr/td");
                var tbody = tableContent.SelectNodes("tbody/tr/td");
                if (header.Count != tbody.Count)
                    return;
                var deviders = new[] { ' ', ':', '\t' };
                for (var i = 0; i < header.Count; i++)
                {
                    switch (RemoveDeviders(header[i].InnerText, deviders).Trim())
                    {
                        case "Доход": result.Salary = RemoveDeviders(tbody[i].InnerText, deviders); break;
                        case "Город": result.Location = RemoveDeviders(tbody[i].InnerText, deviders); break;
                        case "Требуемый опыт работы": result.Experiance = RemoveDeviders(tbody[i].InnerText, deviders); break;
                    }
                }
                lock (_loadedData)
                    _loadedData.Enqueue(result);
            }
            catch (WebException e)
            {
                if (Logger != null)
                    Logger.Error("{0}){1}:\n{2}\n\n", _try, link, e);
                ParceVacancy(link, title, _try++);
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
                var strCount = doc.DocumentNode
                    .SelectSingleNode("//h2[contains(@class,'title_vacancy')]")
                    .SelectSingleNode("//*[contains(@class,'context')]/*[contains(@class,'context')]")
                    .InnerText;
                var vacancyCount = int.Parse(strCount);
                var pagesCount = vacancyCount / 20;
                if (vacancyCount % 20 != 0)
                    pagesCount++;
                var threads = new Thread[vacancyCount];
                for (var i = 0; i < pagesCount; i++)
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
