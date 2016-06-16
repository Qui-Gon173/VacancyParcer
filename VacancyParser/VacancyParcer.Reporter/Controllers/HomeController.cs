using SelectPdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;
using VacancyParcer.Reporter.Models;
using VacancyParser.PagesLoader;
using VacancyParcer.Reporter.Helpers;

namespace VacancyParcer.Reporter.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(ModelFilter filter)
        {
            return RedirectToAction("SetData", "Analisys");
            return View(new PaginationList<VacancyData>(DataCollector.VacancyData.Value, filter));
        }

        public ActionResult ChangedData(ModelFilter filter)
        {
            return View(new PaginationList<Vacancy>(DataCollector.ConvertedVacancyData.Value, filter));
        }

        public FileResult DownloadPage(string page)
        {
            HtmlToPdf converter = new HtmlToPdf();
            var file=converter.ConvertUrl(page);
            return File(file.Save(),"application/pdf","report.pdf");
        }

    }
}
