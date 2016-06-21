using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using VacancyParcer.ClusterLibs;
using VacancyParcer.Reporter.Models;
using VacancyParser.PagesLoader;

namespace VacancyParcer.Reporter.Helpers
{
    public static class DataCollector
    {
        static DataCollector()
        {
            VacancyData = new Lazy<VacancyData[]>(GetData);
            ConvertedVacancyData = new Lazy<Vacancy[]>(() =>
                VacancyData.Value.Select(Vacancy.FromVacancyData).ToArray()
            );
            IrisArray = new Lazy<Element[]>(GetIrisData);
        }

        public static Lazy<VacancyData[]> VacancyData { get; private set; }
        public static Lazy<Vacancy[]> ConvertedVacancyData { get; private set; }
        public static Lazy<Element[]> IrisArray { get; private set; }

        private static VacancyData[] GetData()
        {
            VacancyData[] data;
            using (var reader = new StreamReader(@"D:\Work\Custom\VacancyParcer\VacancyParser\VacancyParcer.Reporter\App_Data\mainData.xml"))
            {
                var serial = new XmlSerializer(typeof(VacancyData[]));
                data = (VacancyData[])serial.Deserialize(reader);
            }
            return data;
        }

        private static Element[] GetIrisData()
        {
            var queue = new Queue<Element>();
            using (var reader = new StreamReader(@"D:\Work\Custom\VacancyParcer\VacancyParser\VacancyParcer.Reporter\App_Data\iris.data"))
            {
                while(!reader.EndOfStream)
                {
                    var elems=reader.ReadLine().Split(',');
                    var data=elems.Take(elems.Length-1)
                        .Select(el=>double.Parse(el, CultureInfo.GetCultureInfo("en-US")))
                        .ToArray();
                    var classType=elems.Last();
                    queue.Enqueue(new Element{ ClassType=classType,Coordinates=data});
                }
            }
            return queue.ToArray();
        }



    }
}