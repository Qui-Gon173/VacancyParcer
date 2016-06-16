using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VacancyParcer.TestConsole
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

        public static int SalaryGrouping(this Vacancy el)
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
    }
}
