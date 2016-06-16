using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VacancyParcer.ClusterLibs;
using VacancyParcer.Reporter.Helpers;
using VacancyParcer.Reporter.Models;
using JsonHelper = System.Web.Helpers.Json;

namespace VacancyParcer.Reporter.Controllers
{
    public class ClusteringController : Controller
    {
        public ActionResult Index()
        {
            var data = DataCollector.ConvertedVacancyData.Value;

            var clusterData=data
                .Where(el=>el.Salary!=0 && el.Experiance!=0)
                .Select(el=>new Point(new double[]{el.Salary,el.Experiance}))
                .ToArray();
            var maxSalary = clusterData.Max(el => el.Coordinates[0]);
            var maxExperiance = clusterData.Max(el => el.Coordinates[1]);

            var kMeans = new KMeans()
            {
                ClustersCount=5,
                MaxExperianse=maxExperiance,
                MaxSalary=maxSalary,
                VectorLength=2
            };
            var clustRes = kMeans.Clustrize(clusterData)
                .GroupBy(el => el.Cluster)
                .ToArray();
            while(clustRes.Count(el=>el.Count()!=0)!=5)
                clustRes = kMeans.Clustrize(clusterData)
                    .GroupBy(el => el.Cluster)
                    .ToArray();

            var chart1Data=clustRes
                .Select((el, i) => 
                    new { 
                        key = "Cluster" + i, 
                        data = el.Select(sub => sub.Object.Coordinates).ToArray()
                        } 
                    );

            var objGroups = data.Where(el => !el.Job.StartsWith("!"))
                .GroupBy(el => el.Location)
                .OrderByDescending(el=>el.Count())
                .Take(10)
                .Select(el => new { key = el.Key, data = el.Count() });

            var tempGroups = data
                .Where(el=>el.Date<new DateTime(DateTime.Now.Year,DateTime.Now.Month,1))
                .GroupBy(el => new DateTime(el.Date.Year, el.Date.Month,1))
                .Select(el => new { key = el.Key.ToString("s"), data = el.Count() });

            return View(new ClusterViewModel { 
                Attributes = JsonHelper.Encode(chart1Data),
                Objects = JsonHelper.Encode(objGroups),
                Time = JsonHelper.Encode(tempGroups)
            });
        }
    }
}
