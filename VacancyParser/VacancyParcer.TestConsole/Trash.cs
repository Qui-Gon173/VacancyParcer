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
            if (int.Parse(el.Salary) < 75000)
                return 1;
            if (int.Parse(el.Salary) >= 75000 && int.Parse(el.Salary) < 90000)
                return 2;
            if (int.Parse(el.Salary) >= 90000 && int.Parse(el.Salary) < 105000)
                return 3;
            if (int.Parse(el.Salary) >= 105000 && int.Parse(el.Salary) < 120000)
                return 4;
            return 5;
        }
    }
}
