using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VacancyParcer.Reporter.Helpers;
using VacancyParcer.Reporter.Models;
using JsonHelper = System.Web.Helpers.Json;


namespace VacancyParcer.Reporter.Controllers
{
    public class AnalisysController : Controller
    {
        //
        // GET: /Analisys/

        public ActionResult Index(int texCount, int cityCount, string tex)
        {
            var group = DataCollector.ConvertedVacancyData.Value.Where(el => el.Salary != 0)
                    .GroupBy(SalaryGroup.SalaryGrouping)
                    .OrderBy(el => el.Key).ToArray();
            var salaryGroups = JsonHelper.Encode(group.Select(el => new
            {
                key = el.Key.SalaryGroupName(),
                data = el.Count()
            }));
            var otherInfo = new Dictionary<string, AnalisisInfo>();
            var otherData = new Dictionary<string, string>();
            var bigClustersInfo = new AnalisisInfo();
            var spaceData = new Dictionary<string, string>();
            var timeData = new Dictionary<string, string>();

            bigClustersInfo.FullCount = DataCollector.ConvertedVacancyData.Value.Count();
            bigClustersInfo.Count = DataCollector.ConvertedVacancyData.Value.Count(el => el.Salary != 0);
            bigClustersInfo.Info = new Dictionary<string, ClasterInfo>();
            Func<Vacancy, double> salary = el => el.Salary;
            foreach (var gr in group)
            {
                var newInfo = new ClasterInfo()
                {
                    Average = Math.Round(gr.Average(salary), 2),
                    Min = gr.Min(salary),
                    Max = gr.Max(salary),
                    Count = gr.Count(),
                    Pers = Math.Round(100 * gr.Count() / (double)bigClustersInfo.Count, 2),
                    FullPers = Math.Round(100 * gr.Count() / (double)bigClustersInfo.FullCount, 2),
                };
                bigClustersInfo.Info.Add(gr.Key.SalaryGroupName(), newInfo);
                var skilsDict = Vacancy.SkillKeyWords.ToDictionary(el => el.Key, el => 0);
                foreach (var el in gr)
                {
                    foreach (var pair in Vacancy.SkillKeyWords)
                    {
                        if (el.SkilsString.Contains(pair.Key))
                            skilsDict[pair.Key]++;
                    }
                }
                var othersubInfo = skilsDict.OrderByDescending(el => el.Value)
                                               .Take(texCount)
                                               .Select(el => new { key = el.Key, data = el.Value })
                                               .ToArray();
                otherData.Add(gr.Key.SalaryGroupName(), JsonHelper.Encode(othersubInfo));

                var clustersInfo = new AnalisisInfo();
                clustersInfo.FullCount = bigClustersInfo.Count;
                clustersInfo.Count = newInfo.Count;
                clustersInfo.Info = othersubInfo.ToDictionary(el => el.key, el => new ClasterInfo()
                {
                    Count = el.data,
                    Pers = Math.Round(100 * el.data / (double)clustersInfo.Count, 2),
                    FullPers = Math.Round(100 * el.data / (double)clustersInfo.FullCount, 2),
                });
                otherInfo.Add(gr.Key.SalaryGroupName(), clustersInfo);

                spaceData.Add(gr.Key.SalaryGroupName(),JsonHelper.Encode(
                    new
                    {
                        name = tex,
                        chartData = gr.Where(el => el.SkilsString.Contains(tex))
                                      .GroupBy(el => el.Location)
                                      .OrderByDescending(el=>el.Count())
                                      .Take(cityCount)
                                      .Select(el => new { key = el.Key, data = el.Count() }).ToArray()
                    }
                    ));


                timeData.Add(gr.Key.SalaryGroupName(),JsonHelper.Encode(
                    new
                    {
                        name = tex,
                        chartData = gr.Where(el => el.SkilsString.Contains(tex) && el.Date < new DateTime(2016, 06, 01))
                                      .GroupBy(el => new DateTime(el.Date.Year, el.Date.Month, 1))
                                      .OrderByDescending(el => el.Count())
                                      .Select(el => new { key = el.Key.ToString("s"), data = el.Count() }).ToArray()
                    }
                    ));
            }

            return View(new AnalisisViewModel
            {
                TotalJsonData = salaryGroups,
                JsonData = otherData,
                SubInfo = otherInfo,
                TotalInfo = bigClustersInfo,
                SpaceClusterData=spaceData,
                TimeClusterData=timeData
            });
        }

        public ActionResult SetData()
        {
            return View(new SelectList(Vacancy.SkillKeyWords.AsEnumerable(), "Key", "Key"));
        }

        [HttpPost]
        public ActionResult SetData(int texCount, int cityCount,string tex)
        {
            return RedirectToAction("Index", new {texCount, cityCount, tex });
        }

    }
}
