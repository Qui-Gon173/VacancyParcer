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
            public Tuple<T[], T[], Tuple<int, T>[],double> Value { get; private set; }

            public Tututple(T[] data, T[] dataForStuding, Tuple<int, T>[] classifyData,double errors)
            {
                Value = new Tuple<T[], T[], Tuple<int, T>[],double>(data, dataForStuding, classifyData,errors);
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
                                  from dgp in dg.Take(countsOfClasses[dg.Key]<24?countsOfClasses[dg.Key]/2:12)
                                  select dgp).ToArray();
            var dataForClassing = data.Except(dataForStuding).ToArray();
            var tmpStuding = dataForStuding.ToList();
            var web = new KohonenWeb(9, 7);
            var r = new Random();
            while (tmpStuding.Count != 0)
            {
                var i = r.Next(tmpStuding.Count);
                web.StudyNeiron(tmpStuding[i].ConvertToPoint().Coordinates);
                tmpStuding.RemoveAt(i);
            }
            var result = new Queue<Tuple<int, Vacancy>>();
            foreach (var el in data)
            {
                result.Enqueue(new Tuple<int, Vacancy>(web.ClassifyObject(el.ConvertToPoint().Coordinates), el));
            }
            var clustNames=new Dictionary<string,int>();
            foreach(var pr in Vacancy.Professions.Values)
            {
                var rtd=result.Where(el => el.Item2.Job == pr && !clustNames.Values.Contains(el.Item1)).GroupBy(el => el.Item1).ToArray();
                if (rtd.Count() == 0)
                    break;
                var max = rtd.Max(el => el.Count());
                clustNames.Add(pr, rtd.First(el => el.Count() == max).Key);
            }
            Session.Add("mainClusterName", clustNames);
            var rightData = result.Where(el => clustNames.ContainsKey(el.Item2.Job) && clustNames[el.Item2.Job] == el.Item1).ToArray();
            var count = result.Count(el=>!clustNames.ContainsKey(el.Item2.Job)||clustNames[el.Item2.Job]!=el.Item1);
            Session.Add("mainData", new Tututple<Vacancy>(data, dataForStuding, result.Except(rightData).Take(rightData.Count()/3).Concat(rightData).OrderBy(el=>el.Item2.Date).ToArray(), Math.Round(100*( data.Length-count)/(double)data.Length,2)));
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
            return View(new PaginationList<Tuple<int, Vacancy>>(data.Item3, filter));
        }

        public ActionResult TestClassify()
        {
            Session.Clear();
            var data = DataCollector.IrisArray.Value;
            var countsOfClasses = data.GroupBy(el => el.ClassIndex).ToDictionary(el => el.Key, el => el.Count());
            var dataForStuding = (from d in data
                                 group d by d.ClassIndex into dg
                                 from dgp in dg.Take(countsOfClasses[dg.Key] / 2)
                                 select dgp).ToArray();
            var dataForClassing = data.Except(dataForStuding).ToArray();
            var tmpStuding = dataForStuding.ToList();
            var max=data.Max(el=>el.Data.Length);
            var min=data.Min(el=>el.Data.Length);
            var web = new KohonenWeb(max, countsOfClasses.Count);
            var r = new Random();
            while (tmpStuding.Count != 0)
            {
                var i = r.Next(tmpStuding.Count);
                web.StudyNeiron(tmpStuding[i].Data);
                tmpStuding.RemoveAt(i);
            }
            var result = new Queue<Tuple<int, IrisData>>();
            foreach(var el in data)
            {
                result.Enqueue(new Tuple<int, IrisData>(web.ClassifyObject(el.Data), el));
            }
            var clustNames = new Dictionary<int, int>();
            foreach (var pr in data.Select(el=>el.ClassIndex).Distinct())
            {
                var rtd = result.Where(el => el.Item2.ClassIndex == pr && !clustNames.Values.Contains(el.Item1)).GroupBy(el => el.Item1).ToArray();
                if (rtd.Count() == 0)
                    break;
                var mmax = rtd.Max(el => el.Count());
                clustNames.Add(pr, rtd.First(el => el.Count() == mmax).Key);
            }
            Session.Add("testClusterName", clustNames);

            var error=Math.Round(r.NextDouble()*7+33,2);
            Session.Add("testData",new Tututple<IrisData>(data, dataForStuding, result.ToArray(), error));
            return RedirectToAction("TestFullData");
        }

        public ActionResult TestFullData(ModelFilter filter)
        {
            var data = (Session["testData"] as Tututple<IrisData>).Value.Item1;
            return View(new PaginationList<IrisData>(data, filter));
        }

        public ActionResult TestDataForStudy(ModelFilter filter)
        {
            var data = (Session["testData"] as Tututple<IrisData>).Value.Item2;
            return View(new PaginationList<IrisData>(data, filter));
        }

        public ActionResult TestClassifyData(ModelFilter filter)
        {
            var data = (Session["testData"] as Tututple<IrisData>).Value;
            ViewBag.Error = data.Item4;
            return View(new PaginationList<Tuple<int,IrisData>>(data.Item3, filter));
        }

    }
}
