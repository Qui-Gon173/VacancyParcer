using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VacancyParcer.ClusterLibs;
using VacancyParcer.Reporter.Helpers;
using VacancyParcer.Reporter.Models;

namespace VacancyParcer.Reporter.Controllers
{
    public class ClassifyController : Controller
    {
        //
        // GET: /Classify/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MainClassify()
        {
            var data = DataCollector.ConvertedVacancyData
                .Value
                .Where(el=>!el.Job.StartsWith("!") && el.Salary!=0&&el.Experiance!=0)
                .ToArray();
            var classCodeDictionary = Vacancy.Professions.ToDictionary(el => el.Value, el => el.Key);
            var classCount = data.GroupBy(el => el.Job)
                .ToDictionary(el => el.Key, el => el.Count());
            var dataForStudy=from d in data
                             group d by d.Job into gd
                             from gdp in gd.Take(classCount[gd.Key]/2)
                             select gdp;
            var web = new KohonenWeb(9, 7);
            foreach(var el in dataForStudy)
            {
                var elementData = el.ConvertToPoint().Coordinates;
                web.StudyNeiron(elementData, classCodeDictionary[el.Job]);
            }
            var dataForClassing = data.Where(el => !dataForStudy.Contains(el)).ToArray();
            var queue = new Queue<dynamic>();
            foreach (var el in dataForClassing)
            {
                var elementData = el.ConvertToPoint().Coordinates;
                queue.Enqueue(new {elType=web.ClassifyObject(elementData),element=el});
            }
            return View();
        }

        public ActionResult TestClassify()
        {
            return View();
        }

    }
}
