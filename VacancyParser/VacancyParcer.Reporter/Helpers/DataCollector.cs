using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using VacancyParcer.Reporter.Models;
using VacancyParser.PagesLoader;

namespace VacancyParcer.Reporter.Helpers
{
    public static class DataCollector
    {
        static DataCollector()
        {
            VacancyData = new Lazy<IEnumerable<VacancyData>>(GetData);
            ConvertedVacancyData = new Lazy<IEnumerable<Vacancy>>(() =>
            {
                return VacancyData.Value.Select(Vacancy.FromVacancyData);
            });
        }

        public static Lazy<IEnumerable<VacancyData>> VacancyData { get; private set; }
        public static Lazy<IEnumerable<Vacancy>> ConvertedVacancyData { get; private set; }

        private static VacancyData[] GetData()
        {
            VacancyData[] data;
            using (var reader = new StreamReader(HttpContext.Current.Request.MapPath(@"~\App_Data\mainData.xml")))
            {
                var serial = new XmlSerializer(typeof(VacancyData[]));
                data = (VacancyData[])serial.Deserialize(reader);
            }
            return data;
        }

    }
}