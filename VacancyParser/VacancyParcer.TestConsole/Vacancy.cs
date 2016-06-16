using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VacancyParcer.ClusterLibs;
using VacancyParser.PagesLoader;

namespace VacancyParcer.TestConsole
{
    public class Vacancy
    {
        
        public string Job { get; set; }
        public string Location { get; set; }
        public DateTime Date { get; set; }
        public double Salary { get; set; }
        public double Experiance { get; set; }
        public string Skils { get; set; }

        public static Vacancy FromVacancyData(VacancyData data)
        {
            var result = new Vacancy();
            result.Job = data.Job;
            result.Skils = data.Skils;
            result.Location = data.Location;

            var date = DateTime.Parse(data.Date);
            result.Date = date > DateTime.Now ? date.AddYears(-1) : date;

            var builder = new StringBuilder();
            foreach (var ch in data.Salary)
            {
                if (ch >= '0' && ch <= '9' || ch == '—')
                    builder.Append(ch);
            }
            if (builder.Length != 0)
            {
                result.Salary = builder.ToString()
                    .Split('—')
                    .Select(double.Parse)
                    .Average();
                builder.Clear();
            }

            foreach (var ch in data.Experiance)
            {
                if (ch >= '0' && ch <= '9' || ch == '-')
                    builder.Append(ch);
            }
            if (builder.Length != 0)
            {
                result.Experiance = builder.ToString()
                    .Split('-')
                    .Select(double.Parse)
                    .Average();
                builder.Clear();
            }

            
            return result;
        }

    }
}