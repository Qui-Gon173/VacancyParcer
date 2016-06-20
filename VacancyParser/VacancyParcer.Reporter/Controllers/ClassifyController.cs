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

        public class Tututple<T>
        {
            public Tuple<T[], T[], Tuple<string, T>[], double> Value { get; private set; }

            public Tututple(T[] data, T[] dataForStuding, Tuple<string, T>[] classifyData, double errors)
            {
                Value = new Tuple<T[], T[], Tuple<string, T>[], double>(data, dataForStuding, classifyData, errors);
            }
        }

        public ActionResult MainClassify()
        {
            Session.Clear();
            var data = DataCollector.ConvertedVacancyData.Value
                .Where(el => !el.Job.StartsWith("!") && el.Salary != 0 && el.Experiance != 0)
                .ToArray();
            var countsOfClasses = data.GroupBy(el => el.Job).ToDictionary(el => el.Key, el => el.Count());
            var dataForStuding = (from d in data
                                  group d by d.Job into dg
                                  from dgp in dg.Take(countsOfClasses[dg.Key] < 200 ? countsOfClasses[dg.Key] / 2 : 100)
                                  select dgp).ToArray();
            var dataForClassing = data.Except(dataForStuding).ToArray();
            var elementStudingData=dataForStuding.Select(el => el.ConvertToElement()).ToArray();
            var len = (int)elementStudingData.Average(el => el.Coordinates.Length);
            var web = new KohonenWebSecond(10000, 150, 150, len);
            web.StudyElements = dataForStuding.Select(el => el.ConvertToElement()).ToArray();
            web.Train();
            var classifiedData = dataForClassing.Select(el => new Tuple<string, Vacancy>(web.BestInStudyArray(el.ConvertToElement()).ClassType, el))
                .ToArray();
            var errorPersent = 100 * classifiedData.Count(el => el.Item1 != el.Item2.Job) / (double)classifiedData.Length;
            Session.Add("mainData", new Tututple<Vacancy>(data, dataForStuding, classifiedData, Math.Round(errorPersent, 2)));
            return RedirectToAction("MainFullData");
        }

        public ActionResult MainFullData(ModelFilter filter)
        {
            var data = (Session["mainData"] as Tututple<Vacancy>).Value.Item1;
            return View(new PaginationList<Vacancy>(data, filter));
        }

        public ActionResult MainDataForStudy(ModelFilter filter)
        {
            var data = (Session["mainData"] as Tututple<Vacancy>).Value.Item2;
            return View(new PaginationList<Vacancy>(data, filter));
        }

        public ActionResult MainClassifyData(ModelFilter filter)
        {
            var data = (Session["mainData"] as Tututple<Vacancy>).Value;
            ViewBag.Error = data.Item4;
            return View(new PaginationList<Tuple<string, Vacancy>>(data.Item3, filter));
        }

        public ActionResult TestClassify()
        {
            Session.Clear();
            var data = DataCollector.IrisArray.Value;
            var countsOfClasses = data.GroupBy(el => el.ClassType).ToDictionary(el => el.Key, el => el.Count());
            var dataForStuding = (from d in data
                                  group d by d.ClassType into dg
                                  from dgp in dg.Take(countsOfClasses[dg.Key] / 2)
                                  select dgp).ToArray();
            var dataForClassing = data.Except(dataForStuding).ToArray();

            var len = (int)dataForStuding.Average(el => el.Coordinates.Length);
            var web = new KohonenWebSecond(10000, 50, 50, len);
            web.StudyElements = dataForStuding;
            web.Train();
            var classifiedData = dataForClassing.Select(el => new Tuple<string, Element>(web.BestInStudyArray(el).ClassType, el))
                .ToArray();
            var errorPersent = 100 * classifiedData.Count(el=>el.Item1!=el.Item2.ClassType) / (double)classifiedData.Length;

            Session.Add("testData", new Tututple<Element>(data, dataForStuding, classifiedData, Math.Round(errorPersent, 2)));
            return RedirectToAction("TestFullData");
        }

        public ActionResult TestFullData(ModelFilter filter)
        {
            var data = (Session["testData"] as Tututple<Element>).Value.Item1;
            return View(new PaginationList<Element>(data, filter));
        }

        public ActionResult TestDataForStudy(ModelFilter filter)
        {
            var data = (Session["testData"] as Tututple<Element>).Value.Item2;
            return View(new PaginationList<Element>(data, filter));
        }

        public ActionResult TestClassifyData(ModelFilter filter)
        {
            var data = (Session["testData"] as Tututple<Element>).Value;
            ViewBag.Error = data.Item4;
            return View(new PaginationList<Tuple<string, Element>>(data.Item3, filter));
        }

    }
}
