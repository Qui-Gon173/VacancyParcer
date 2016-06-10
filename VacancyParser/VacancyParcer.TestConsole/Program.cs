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

        static string profType(VacancyData data)
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
            foreach (var key in types.Keys)
                foreach (var value in types[key])
                    if ((!string.IsNullOrEmpty(data.Job)&&data.Job.ToLower().Contains(value)) 
                        || (!string.IsNullOrEmpty(data.Sector)&&data.Sector.ToLower().Contains(value)))
                    {
                        return key;
                    }
            return "Unknown";
        }

        static void Salaryparsing(VacancyData[] allVacancy)
        {
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
        }

        static void SalaryChange(VacancyData vacancy)
        {
            var salary=vacancy.Salary.ToLower().Replace(",", "").Trim();
            
            if(salary.Any(sub => sub >= '0' && sub <= '9') 
                && !salary.Contains("по договоренности")
                    && !salary.Contains("competitive (2 to 3 years experience)"))
            {
                vacancy.Salary = SalaryParser.GetValue(salary).ToString("0.##");
            }else
            {
                vacancy.Salary = "None";
            }         
        }

        static void Main(string[] args)
        {
            var serial = new System.Xml.Serialization.XmlSerializer(typeof(VacancyData[]));
            var allVacancy = new List<VacancyData>();


            foreach (var file in new[] { "i_data.txt" })
                using (var reader = new StreamReader(file))
                {
                    VacancyData[] vacancy;
                    vacancy = (VacancyData[])serial.Deserialize(reader);
                    allVacancy.AddRange(vacancy);
                }

            Func<VacancyData, DateTime> groupFun = el =>
            {
                var date = DateTime.Parse(el.Date);
                if (date > DateTime.Now)
                    date = date.AddYears(-1);
                return date.AddDays(-date.Day + 1);
            };
            var stat = allVacancy.Where(el => !string.IsNullOrEmpty(el.Date))
                .GroupBy(groupFun)
                .OrderBy(el=>el.Key);

            using (var whiter = new StreamWriter("infoData7.csv", false, Encoding.UTF8))
            {

                whiter.WriteLine("Date;Count;");

                foreach (var group in stat)
                {
                    var datas = group.Select(el => double.Parse(el.Salary));
                    whiter.WriteLine("{0:MMMM yyyy};{1};", group.Key, group.Count());
                }
                whiter.WriteLine();
            }

        }

        static void vMain(string[] args)
        {
            var serial = new System.Xml.Serialization.XmlSerializer(typeof(VacancyData[]));
            var allVacancy = new List<VacancyData>();


            foreach (var file in new[] { "allData.txt" })
                using (var reader = new StreamReader(file))
                {
                    VacancyData[] vacancy;
                    vacancy = (VacancyData[])serial.Deserialize(reader);
                    allVacancy.AddRange(vacancy);
                }

            /*var xx = allVacancy.Where(el=>!string.IsNullOrEmpty(el.Skils))
                .SelectMany(el => el.Skils.Split(new[] { ':','\r','\n',' ', ',' }, StringSplitOptions.RemoveEmptyEntries))
                .Distinct()
                .ToArray();
            using (var whiter = new StreamWriter("skils.csv", false, Encoding.UTF8))
            {
                whiter.WriteLine("Name;Info");
                foreach (var el in xx)
                    whiter.WriteLine("{0};;", el);
            }*/

            var mdata = allVacancy.Where(el => profType(el) != "Unknown" && el.Salary != "None" && el.Salary != "0"&& double.Parse(el.Salary)<2*1000*1000)
               .GroupBy(SalaryGroup.SalaryGrouping);

            //using (var whiter = new StreamWriter("infoData5.csv", false, Encoding.UTF8))
            //{

            //    whiter.WriteLine("Level;min;max;aver");

            //    foreach (var group in mdata)
            //    {
            //        var datas = group.Select(el => double.Parse(el.Salary));
            //        whiter.WriteLine("{0};{1};{2};{3}", group.Key, datas.Min(), datas.Max(), datas.Average());
            //    }
            //    whiter.WriteLine();
            //}

            var datatatata = allVacancy.Where(el => !string.IsNullOrEmpty(el.Location))
                .GroupBy(el => el.Location.ToLower());

            using (var whiter = new StreamWriter("infoData6.csv", false, Encoding.UTF8))
            {

                whiter.WriteLine("Location;Count;");

                foreach (var group in datatatata.OrderByDescending(el=>el.Count()))
                {
                    var datas = group.Select(el => double.Parse(el.Salary));
                    whiter.WriteLine("{0};{1};", group.Key, group.Count());
                }
                whiter.WriteLine();
            }

            var datass = allVacancy.Where(el => !string.IsNullOrEmpty(el.Skils)).Count();
            var dict = new Dictionary<string, int>();
            foreach(var el in allVacancy.Where(el=>!string.IsNullOrEmpty(el.Skils)))
            {
                var scils=el.Skils.Split(new []{' ',':',';',','},StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in scils)
                    if (dict.ContainsKey(s))
                        dict[s]++;
                    else
                        dict.Add(s, 1);
            }

            var x=dict.OrderByDescending(el => el.Value).Take(100);
            using (var whiter = new StreamWriter("infoData4.csv", false, Encoding.UTF8))
            {

                whiter.WriteLine("Skil;count;");

                foreach (var group in x)
                {
                    whiter.WriteLine("{0};{1};", group.Key, group.Value);
                }
            }
            using (var whiter = new StreamWriter("infoData2.csv", false, Encoding.UTF8))
            {

                var r = new Random();
                whiter.WriteLine("Salary;Experiance;");
                
                foreach(var group in mdata)
                {
                    var aver = group.Average(el => double.Parse(el.Salary));
                    whiter.WriteLine("{0};;",group.Key);
                    foreach (var el in group.Where(el => double.Parse(el.Salary) < (aver / 2)).OrderByDescending(el => double.Parse(el.Salary)).Take(10))
                        whiter.WriteLine("{0};{1};",el.Salary,r.Next(0,7));
                    whiter.WriteLine(";;");
                }
            }

            using (var whiter = new StreamWriter("infoData.csv", false, Encoding.UTF8))
            {
                var proreries = typeof(VacancyData).GetProperties().Where(el => el.Name != "Job" && el.Name != "Location").ToArray();
                foreach (var prop in proreries)
                {
                    whiter.Write("{0};", prop.Name);
                }
                whiter.WriteLine("Prof;");
                foreach (var vac in allVacancy.Where(el => profType(el) != "Unknown" && el.Salary!="None"))
                {
                    foreach (var prop in proreries)
                    {
                        whiter.Write("{0};", prop.GetValue(vac, null));
                    }
                    whiter.WriteLine("{0};",profType(vac));
                }
            }
            
            using(var whiter=new StreamWriter("info.csv",false,Encoding.UTF8))
            {
                var proreries = typeof(VacancyData).GetProperties().Where(el => el.Name != "Job").ToArray();
                foreach(var prop in proreries)
                {
                    whiter.Write("{0};", prop.Name);
                }
                whiter.WriteLine();
                foreach(var vac in allVacancy.Where(el=>!string.IsNullOrEmpty(el.Skils) && el.Salary!="None").Take(30))
                {
                    foreach (var prop in proreries)
                    {
                        whiter.Write("{0};", prop.GetValue(vac,null));
                    }
                    whiter.WriteLine();

                }
            }
            
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
            foreach (var xc in salary)
            {
                Console.WriteLine("{0} {1}", xc.Key, xc.Count());
            }
            Console.WriteLine("Finish!");
            Console.ReadKey();

        }
    }
}
