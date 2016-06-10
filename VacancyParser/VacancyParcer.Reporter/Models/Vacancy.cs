using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using VacancyParcer.ClusterLibs;
using VacancyParser.PagesLoader;

namespace VacancyParcer.Reporter.Models
{
    public class Vacancy
    {
        private static readonly Dictionary<string, int> SkillKeyWords;
        private static readonly int[] ScillsTypes;

        public string Job { get; set; }
        public string Location { get; set; }
        public DateTime Date { get; set; }
        public double Salary { get; set; }
        public double Experiance { get; set; }
        public Dictionary<int, int> Skils { get; set; }

        static Vacancy()
        {
            SkillKeyWords = new Dictionary<string, int>();
            using(var reader=new System.IO.StreamReader(@"App_Data\skils.csv"))
            {
                while(!reader.EndOfStream)
                {
                    var strs = reader.ReadLine().Split(';');
                    SkillKeyWords.Add(strs[0].ToLower(), int.Parse(strs[1]));
                }
            }
            ScillsTypes = SkillKeyWords.Select(el => el.Value)
                .Distinct()
                .OrderBy(el => el)
                .ToArray();
        }

        public static Vacancy FromVacancyData(VacancyData data)
        {
            var result = new Vacancy();
            result.Job = data.Job;

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
                if (ch >= '0' && ch <= '9' || ch == '—')
                    builder.Append(ch);
            }
            if (builder.Length != 0)
            {
                result.Experiance = builder.ToString()
                    .Split('—')
                    .Select(double.Parse)
                    .Average();
                builder.Clear();
            }

            result.Skils = ScillsTypes.ToDictionary(el => el, el => 0);

            foreach (var pair in SkillKeyWords)
            {
                if (data.Experiance.Contains(pair.Key))
                    result.Skils[pair.Value]++;
            }
            return result;
        }

        public Point ConvertToPoint()
        {
            var coord=new double[9];
            coord[0] = Salary;
            coord[1] = Experiance;

            var weight=Skils.Aggregate(0.0,(acc,el)=>acc+=(el.Value*el.Value));
            if(weight!=0)
                weight=Math.Sqrt(1/weight);
            for(var i=2;i<9;i++)
            {
                if (weight != 0)
                    coord[i] = Skils[i - 1] * weight;
                else
                    coord[i] = 0;
            }

            return new Point(coord);
        }
    }
}