﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web;
using VacancyParcer.ClusterLibs;
using VacancyParser.PagesLoader;

namespace VacancyParcer.Reporter.Models
{
    public class Vacancy
    {
        public static readonly Dictionary<string, int> SkillKeyWords;
        public static readonly Dictionary<int,string> Professions = new Dictionary<int,string>
        {
            {1,"Разработчик"},
            {2,"QA-специалист"},
            {3,"Аналитик"},
            {4,"Дизайнер"},
            {5,"Администратор"},
            {6,"SEO-специалист"},
            {7,"Менеджер"}
        };

        [Display(Name="Класс")]
        public string Job { get; set; }
        [Display(Name = "Расположение(Не учавствует в классификации)")]
        public string Location { get; set; }
        [Display(Name = "Дата(Не учавствует в классификации)")]
        public DateTime Date { get; set; }
        [Display(Name="Зарплата")]
        public double Salary { get; set; }
        [Display(Name="Опыт работы")]
        public double Experiance { get; set; }
        [Display(Name="Навыки")]
        public Dictionary<int, int> Skils { get; set; }
        public string SkilsString { get; set; }

        static Vacancy()
        {
            SkillKeyWords = new Dictionary<string, int>();
            using (var reader = new System.IO.StreamReader(@"D:\Work\Custom\VacancyParcer\VacancyParser\VacancyParcer.Reporter\App_Data\skils.csv"))
            {
                while (!reader.EndOfStream)
                {
                    var strs = reader.ReadLine().Split(';');
                    SkillKeyWords.Add(strs[0].ToLower(), int.Parse(strs[1]));
                }
            }
        }

        public static Vacancy FromVacancyData(VacancyData data)
        {
            var result = new Vacancy();
            result.Job = (from sc in SkillKeyWords
                         join n in Professions on sc.Value equals n.Key
                         where data.Job.ToLower().Contains(sc.Key)
                         select n.Value)
                         .FirstOrDefault();
            if (string.IsNullOrEmpty(result.Job))
                result.Job = "!" + data.Job;
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

            result.Skils = Professions.ToDictionary(el => el.Key, el => 0);

            var sckils=data.Skils.ToLower();
            result.SkilsString = sckils;
            foreach (var pair in SkillKeyWords)
            {
                if (sckils.Contains(pair.Key))
                    result.Skils[pair.Value]++;
            }
            return result;
        }

        public Element ConvertToElement()
        {
            var coord = new double[9];
            coord[0] = Salary;
            coord[1] = Experiance;

            var weight = Skils.Aggregate(0.0, (acc, el) => acc += (el.Value * el.Value));
            if (weight != 0)
                weight = Math.Sqrt(1 / weight);
            for (var i = 2; i < 9; i++)
            {
                if (weight != 0)
                    coord[i] = Skils[i - 1] * weight;
                else
                    coord[i] = 0;
            }

            return new Element {ClassType=Job,Coordinates=coord };
        }
    }
}