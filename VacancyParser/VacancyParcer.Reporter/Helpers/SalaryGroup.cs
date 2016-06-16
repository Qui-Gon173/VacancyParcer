using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VacancyParcer.Reporter.Helpers
{
    public static class SalaryGroup 
    {
        public static int SalaryGrouping(this VacancyParser.PagesLoader.VacancyData el)
        {
            if (double.Parse(el.Salary) < 35000)
                return 1;
            if (double.Parse(el.Salary) >= 35000 && double.Parse(el.Salary) < 55000)
                return 2;
            if (double.Parse(el.Salary) >= 55000 && double.Parse(el.Salary) < 90000)
                return 3;
            if (double.Parse(el.Salary) >= 90000 && double.Parse(el.Salary) < 125000)
                return 4;
            return 5;
        }

        public static int SalaryGrouping(this VacancyParcer.Reporter.Models.Vacancy el)
        {
            if (el.Salary < 35000)
                return 1;
            if (el.Salary >= 35000 && el.Salary < 55000)
                return 2;
            if (el.Salary >= 55000 && el.Salary < 90000)
                return 3;
            if (el.Salary >= 90000 && el.Salary < 125000)
                return 4;
            return 5;
        }

        public static string SalaryGroupName(this int group)
        {
            switch(group)
            {
                case 1: return "Низкий";
                case 2: return "Ниже среднего";
                case 3: return "Средний";
                case 4: return "Выше среднего";
                case 5: return "Высокий";
            }
            throw new Exception("Это залет!");
        }
    }
}
