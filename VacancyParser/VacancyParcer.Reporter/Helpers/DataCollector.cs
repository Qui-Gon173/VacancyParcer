using System;
using System.Collections.Generic;
using System.Globalization;
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
                VacancyData.Value.Select(Vacancy.FromVacancyData)
            );
            IrisArray = new Lazy<IrisData[]>(GetIrisData);
        }

        public static Lazy<IEnumerable<VacancyData>> VacancyData { get; private set; }
        public static Lazy<IEnumerable<Vacancy>> ConvertedVacancyData { get; private set; }
        public static Lazy<IrisData[]> IrisArray { get; private set; }

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

        private static IrisData[] GetIrisData()
        {
            var queue = new Queue<IrisData>();
            using (var reader = new StreamReader(HttpContext.Current.Request.MapPath(@"~\App_Data\iris.data")))
            {
                while(!reader.EndOfStream)
                {
                    var elems=reader.ReadLine().Split(',');
                    var data=elems.Take(elems.Length-1)
                        .Select(el=>double.Parse(el, CultureInfo.GetCultureInfo("en-US")))
                        .ToArray();
                    var classType=elems.Last();
                    queue.Enqueue(new IrisData(data,classType));
                }
            }
            return queue.ToArray();
        }



    }
}