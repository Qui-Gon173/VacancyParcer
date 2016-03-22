using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using VacancyParser.PagesLoader;

namespace VacancyParcer.TestConsole
{
    class Program
    {
        static void LoadData(PageLoader loader, string dataFile)
        {
            loader.Init();
            var result = loader.GetVacancy();
            var serial = new System.Xml.Serialization.XmlSerializer(typeof(VacancyData[]));
            using (var writer = new System.IO.StreamWriter(dataFile))
            {
                serial.Serialize(writer, result);
            }
        }

        static void LoadVacancy()
        {
            if (!Directory.Exists("d/"))
                Directory.CreateDirectory("d/");
            if (!Directory.Exists("e/"))
                Directory.CreateDirectory("e/");
            if (!Directory.Exists("i/"))
                Directory.CreateDirectory("i/");
            PageLoader dloader = DarwinRecruitmentLoader.Instance;
            PageLoader eloader = EFinancialCareersLoader.Instance;
            PageLoader iloader = ItMozgLoader.Instance;
            dloader.Logger = new PoolLogger("d/d_debug.txt", "d/d_info.txt", "d/d_error.txt");
            eloader.Logger = new PoolLogger("e/e_debug.txt", "e/e_info.txt", "e/e_error.txt");
            iloader.Logger = new PoolLogger("i/i_debug.txt", "i/i_info.txt", "i/i_error.txt");

            dloader.WaitTime = 400;
            eloader.WaitTime = 300;
            iloader.WaitTime = 350;

            var threadAr = new System.Threading.Thread[]{
                new Thread(()=>LoadData(dloader,"d_data.txt")),
                new Thread(()=>LoadData(eloader,"e_data.txt")),
                new Thread(()=>LoadData(iloader,"i_data.txt"))
            };

            foreach (var el in threadAr)
                el.Start();

            while (threadAr.Any(el => el.IsAlive)) {
                int t1, t2;
                System.Threading.Thread.Sleep(10000);
        }
            dloader.Logger.ForceSave();
            eloader.Logger.ForceSave();
            iloader.Logger.ForceSave();
        }

        static bool ContainsAny(string value, params string[] array)
        {
            foreach (var el in array)
                if (value.Contains(el))
                    return true;
            return false;
        }



        static Dictionary<string, int> Els(VacancyData[] data)
        {
            var types = new Dictionary<string, string[]>{
            {"analitics", new []{ "analyst", "risk", "аналитик" }},
            {"testers", new[]{ "тест", "delivery", "qa", "test", "validat", "тестировщик" }},
            {"develop",new []{ "программист", "develop", "сопровожден", "бэкенд", "engineer", "java", "technology", "software", "разработчик" }},
            {"designer",new [] { "model", "художник", "иллюстратор", "аниматор", "арт", "дизайнер" }},
            {"admin",new []{ "электромонтажник", "сервис", "техни", "network", "админ" }},
            {"manager",new []{ "manager", "support", "консультант", " маркетолог", "копирайтер", "наборщик", "consultant", "менеджер" }},
            {"seo",new []{ "seo", "smm" }}
            };
            var result = new Dictionary<string, int>();
            foreach (var key in types.Keys)
                foreach (var value in types[key])
                    foreach (var el in data)
                        if (el.Job.Contains(value))
                            if (!result.ContainsKey(key))
                                result.Add(key, 1);
                            else
                                result[key]++;
            return result;
        }

        static void Main(string[] args)
        {
            var serial = new System.Xml.Serialization.XmlSerializer(typeof(VacancyData[]));
            var allVacancy = new List<VacancyData>();

            foreach (var file in new[] { "e_data.txt", "i_data.txt", "d_data.txt" })
                using (var reader = new StreamReader(file))
                {
                    VacancyData[] vacancy;
                    vacancy = (VacancyData[])serial.Deserialize(reader);
                    allVacancy.AddRange(vacancy);
                }
            var salaries=allVacancy
                .Select(el => el.Salary.ToLower().Replace(",", "").Trim())
                .Where(el => el.Any(sub => sub >= '0' && sub <= '9') && !el.Contains("по договоренности")
                    && !el.Contains("competitive (2 to 3 years experience)"))
                .OrderBy(el => el)
                .ToArray();

            File.Delete("salariesDebug.txt");
            for (var i = 0; i < salaries.Length;i++ )
            {
                Console.WriteLine("{2}.{0}\n {1}\n\n",salaries[i], SalaryParser.GetValue(salaries[i]),i);
                File.AppendAllText("salariesDebug.txt", string.Format("{0}\n {1}\n\n", salaries[i], SalaryParser.GetValue(salaries[i])));
            }
            /*
            var kazanVacancy = Els(allVacancy
                .Where(el => el.Location.ToLower().Contains("казань") || el.Location.ToLower().Contains("kazan"))
                .ToArray());
            
            var highlevelSalary = Els(allVacancy
                .Where(el => int.Parse(el.Salary) > 110000)
                .ToArray());

            var moscowAndhighLevelSalary = Els(allVacancy
                .Where(el => (el.Location.ToLower().Contains("москва")
                    || el.Location.ToLower().Contains("moscow"))
                    && int.Parse(el.Salary) > 90000)
                .ToArray());

            var exp2SqlCsharp = Els(allVacancy
                .Where(el => el.Skils != null && (el.Skils.Contains("C#") &&
                     el.Skils.ToLower().Contains("sql"))
                    && int.Parse(el.Experiance) > 2)
                .ToArray());

            var php1c = Els(allVacancy
                .Where(el => el.Skils != null && (el.Skils.Contains("1С") ||
                     el.Skils.ToLower().Contains("PHP"))
                    && int.Parse(el.Salary) > 110000)
                .ToArray());

            var salary = allVacancy.GroupBy(el=>el.Location).OrderByDescending(el=>el.Count()).ToArray();
            foreach (var x in salary)
            {
                Console.WriteLine("{0} {1}", x.Key, x.Count());
            }*/
            Console.WriteLine("Finish!");
            Console.ReadKey();

        }
    }
}
