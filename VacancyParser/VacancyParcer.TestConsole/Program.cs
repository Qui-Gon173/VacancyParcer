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
            
        }
    }
}
