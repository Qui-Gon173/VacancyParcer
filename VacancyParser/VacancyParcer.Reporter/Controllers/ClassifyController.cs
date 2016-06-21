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

        public ActionResult MainClassify()
        {
            MainThread.Stop();
            MainThread.Start();
            return View();
        }

        [HttpPost]
        public ActionResult MainClassify(string foo)
        {
            return RedirectToAction("MainFullData");
        }

        public ActionResult MainFullData(ModelFilter filter)
        {
            var data = MainThread.Result.Value.Item1;
            return View(new PaginationList<Vacancy>(data, filter));
        }

        public ActionResult MainDataForStudy(ModelFilter filter)
        {
            var data = MainThread.Result.Value.Item2;
            return View(new PaginationList<Vacancy>(data, filter));
        }

        public ActionResult MainClassifyData(ModelFilter filter)
        {
            var data = MainThread.Result.Value;
            ViewBag.Error = data.Item4;
            return View(new PaginationList<Tuple<string, Vacancy>>(data.Item3, filter));
        }

        public ActionResult TestClassify()
        {
            TestThread.Stop();
            TestThread.Start();
            return View();
        }

        [HttpPost]
        public JsonResult GetStatus(bool isTest)
        {
            if(isTest)
            {
                return Json(new { status = TestThread.Status, progress = TestThread.Progress, finished = TestThread.Finished });
            }else
                return Json(new { status = MainThread.Status, progress = MainThread.Progress, finished = MainThread.Finished });
        }

        [HttpPost]
        public ActionResult TestClassify(string foo)
        {
            return RedirectToAction("TestFullData");
        }

        public ActionResult TestFullData(ModelFilter filter)
        {
            var data = TestThread.Result.Value.Item1;
            return View(new PaginationList<Element>(data, filter));
        }

        public ActionResult TestDataForStudy(ModelFilter filter)
        {
            var data = TestThread.Result.Value.Item2;
            return View(new PaginationList<Element>(data, filter));
        }

        public ActionResult TestClassifyData(ModelFilter filter)
        {
            var data = TestThread.Result.Value;
            ViewBag.Error = data.Item4;
            return View(new PaginationList<Tuple<string, Element>>(data.Item3, filter));
        }

    }
}
