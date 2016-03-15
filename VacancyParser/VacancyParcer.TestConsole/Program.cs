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
        static TimeWaiter waiter = new TimeWaiter();

        static void load(object link,int _try=0)
        {
            if (_try > 5)
                return;
            try
            {
                waiter.Wait();
                using (var client = new System.Net.WebClient())
                {
                    var data = client.DownloadString((string)link);
                    Console.WriteLine("{0}:{1}",link,data.Length);
                }
            }catch(Exception e)
            {
                Console.WriteLine("{0}:{1}", link, e);
                load(link,_try++);
            }
        }

        static void Main(string[] args)
        {
            Console.SetOut(new System.IO.StreamWriter("log.txt"));
            waiter.TimeToWaitInMs = 600;
            var threadList = new List<Thread>();
            for(var i=0;i<112;i++)
            {
                threadList.Add(new Thread(l=>load(l)) { Name=i.ToString()});
            }
            for (var i = 0; i < 112; i++)
            {
                threadList[i].Start(@"http://itmozg.ru/search/vacancy/?page=" + (i + 1));
            }
        
            while(threadList.Any(el=>el.IsAlive))
                Thread.Sleep(1000);
            Console.WriteLine("It's all!");
            Console.ReadKey();
        }
    }
}
