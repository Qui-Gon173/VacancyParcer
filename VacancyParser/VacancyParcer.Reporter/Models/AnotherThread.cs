using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using VacancyParcer.ClusterLibs;
using VacancyParcer.Reporter.Helpers;

namespace VacancyParcer.Reporter.Models
{
    public static class MainThread
    {
        private static Thread _context;
        public static double Progress { get; private set; }
        public static string Status { get; private set; }
        public static bool Finished { get; private set; }
        public static Tututple<Vacancy> Result { get; private set; }


        public static void Start()
        {
            ThreadStart action = () =>
                {
                    Progress = 0;
                    Status = "Получение данных";
                    var data = DataCollector.ConvertedVacancyData.Value
                        .Where(el => !el.Job.StartsWith("!") && el.Salary != 0 && el.Experiance != 0)
                        .ToArray();
                    Status = "Формирование обучающей выборки";
                    var countsOfClasses = data.GroupBy(el => el.Job).ToDictionary(el => el.Key, el => el.Count());
                    var dataForStuding = (from d in data
                                          group d by d.Job into dg
                                          from dgp in dg.Take(countsOfClasses[dg.Key] < 200 ? countsOfClasses[dg.Key] / 2 : 100)
                                          select dgp).ToArray();
                    Status = "Формирование выборки для классификации";
                    var dataForClassing = data.Except(dataForStuding).ToArray();
                    var elementStudingData = dataForStuding.Select(el => el.ConvertToElement()).ToArray();
                    var len = (int)elementStudingData.Average(el => el.Coordinates.Length);
                    Status = "Инициализация сети Кохонена";
                    var web = new KohonenWebSecond(15000, 100, 100, len);
                    web.StudyElements = dataForStuding.Select(el => el.ConvertToElement()).ToArray();
                    web.CurrectIteration += i => Progress = Math.Round(100*i/15000.0,2);
                    Status = "Обучение сети Кохонена";
                    web.Train();
                    Status = "Классификация данных";
                    var classifiedData = dataForClassing.Select(el => new Tuple<string, Vacancy>(web.BestInStudyArray(el.ConvertToElement()).ClassType, el))
                        .ToArray();
                    var errorPersent = 100 * classifiedData.Count(el => el.Item1 != el.Item2.Job) / (double)classifiedData.Length;
                    Result=new Tututple<Vacancy>(data, dataForStuding, classifiedData, Math.Round(errorPersent, 2));
                    Status = "Завершено!";
                    Finished = true;
                };
            _context = new Thread(action);
            _context.Start();
        }

        public static void Stop()
        {
            if(_context!=null)
                _context.Abort();
            _context = null;
            Progress = 0;
            Status = "";
            Finished = false;
        }
    }

    public static class TestThread
    {
        private static Thread _context;
        public static double Progress { get; private set; }
        public static string Status { get; private set; }
        public static bool Finished { get; private set; }
        public static Tututple<Element> Result { get; private set; }

        public static void Start()
        {
            ThreadStart action = () =>
            {
                Progress = 0;
                Status = "Получение данных";
                var data = DataCollector.IrisArray.Value;
                Status = "Формирование обучающей выборки";                
                var countsOfClasses = data.GroupBy(el => el.ClassType).ToDictionary(el => el.Key, el => el.Count());
                var dataForStuding = (from d in data
                                      group d by d.ClassType into dg
                                      from dgp in dg.Take(countsOfClasses[dg.Key] / 2)
                                      select dgp).ToArray();
                Status = "Формирование выборки для классификации";                
                var dataForClassing = data.Except(dataForStuding).ToArray();

                Status = "Инициализация сети Кохонена";
                var len = (int)dataForStuding.Average(el => el.Coordinates.Length);
                var web = new KohonenWebSecond(10000, 50, 50, len);
                web.StudyElements = dataForStuding;
                web.CurrectIteration += i => Progress = Math.Round(100*i / 10000.0, 2);
                Status = "Обучение сети Кохонена";
                web.Train();
                Status = "Классификация данных";
                var classifiedData = dataForClassing.Select(el => new Tuple<string, Element>(web.BestInStudyArray(el).ClassType, el))
                    .ToArray();
                var errorPersent = 100 * classifiedData.Count(el => el.Item1 != el.Item2.ClassType) / (double)classifiedData.Length;
                Result=new Tututple<Element>(data, dataForStuding, classifiedData, Math.Round(errorPersent, 2));
                Status = "Завершено";
                Finished = true;
            };
            _context = new Thread(action) { IsBackground=true};
            _context.Start();
        }

        public static void Stop()
        {
            if (_context != null)
                _context.Abort();
            _context = null;
            Progress = 0;
            Status = "";
            Finished = false;
        }
    }
}