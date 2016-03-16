using HtmlAgilityPack;
using System;
using System.Collections.Generic;
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
            PageLoader dloader = DarwinRecruitmentLoader.Instance;
            PageLoader eloader = EFinancialCareersLoader.Instance;
            PageLoader iloader = ItMozgLoader.Instance;
            dloader.Logger = new PoolLogger("d_debug.txt", "d_info.txt", "d_error.txt");
            eloader.Logger = new PoolLogger("e_debug.txt", "e_info.txt", "e_error.txt");
            iloader.Logger = new PoolLogger("i_debug.txt", "i_info.txt", "i_error.txt");

            dloader.WaitTime = 400;
            eloader.WaitTime = 450;
            iloader.WaitTime = 350;

            var threadAr = new System.Threading.Thread[]{
                new Thread(()=>LoadData(dloader,"d_data.txt")),
                new Thread(()=>LoadData(eloader,"e_data.txt")),
                new Thread(()=>LoadData(iloader,"i_data.txt"))
            };

            foreach (var el in threadAr)
                el.Start();

            while (threadAr.Any(el => el.IsAlive))
                System.Threading.Thread.Sleep(10000);
            dloader.Logger.ForceSave();
            eloader.Logger.ForceSave();
            iloader.Logger.ForceSave();
        }

        static void Main(string[] args)
        {
            var serial = new System.Xml.Serialization.XmlSerializer(typeof(VacancyData[]));
            VacancyData[] vacancy;
            var deviders = new[] { '\t', '\r', '\n' };
            using (var reader = new System.IO.StreamReader("i_data.txt"))
            {
                vacancy = (VacancyData[])serial.Deserialize(reader);
            }
            foreach (var x in vacancy)
            {
                if (x.Job != null)
                    x.Job = PageLoader.RemoveDeviders(x.Job, deviders).Trim();
                if (x.Location != null)
                    x.Location = PageLoader.RemoveDeviders(x.Location, deviders).Trim();
                if (x.Salary != null)
                    x.Salary = PageLoader.RemoveDeviders(x.Salary, deviders).Trim();
                if (x.Sector != null)
                    x.Sector = PageLoader.RemoveDeviders(x.Sector,deviders).Trim();
                if (x.Skils != null)
                    x.Skils = PageLoader.RemoveDeviders(x.Skils, deviders).Trim();
                if (x.Experiance != null)
                    x.Experiance = PageLoader.RemoveDeviders(x.Experiance, deviders).Trim();
            }
            using (var writer = new System.IO.StreamWriter("i_data_fix.txt"))
            {
                serial.Serialize(writer, vacancy);
            }

        }
    }
}
