using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VacancyParcer.TestConsole
{
    public static class SalaryParser
    {
        private static double Curency(string salary)
        {
            if (salary.Contains("£") || salary.Contains("gbp"))
                return 95.79;
            if (salary.Contains("€") || salary.Contains("eur"))
                return 75.63;
            if (salary.Contains("yen") || salary.Contains("jpy"))
                return 0.60;
            if (salary.Contains("chf"))
                return 69.40;
            if (salary.Contains("w"))
                return 0.08;
            if (salary.Contains("aud"))
                return 51.37;
            if (salary.Contains("dkk"))
                return 10.14;
            if (salary.Contains("hk"))
                return 8.69;
            if (salary.Contains("nok"))
                return 8.01;
            if (salary.Contains("sek"))
                return 8.19;
            if (salary.Contains("sgd"))
                return 49.5;
            if (salary.Contains("руб"))
                return 1;
            if (salary.Contains("rm"))
                return 16.90;
            return 67.38;
        }


        private static string RemoveTags(string salary)
        {
            while (salary.IndexOfAny(new[] { '<', '>' }) != -1)
            {
                var startIndex = salary.IndexOf('<');
                var endIndex = salary.IndexOf('>');
                if (startIndex == -1 || endIndex == -1)
                    break;
                salary = salary.Remove(startIndex, endIndex - startIndex + 1);
            }
            return salary.Trim();
        }

        private static double Coeff(string salary)
        {
            var index = salary.IndexOf("k");
            if (index > 0 && (salary[index - 1] >= '0' && salary[index - 1] <= '9'))
                return 1000;
            index = salary.IndexOf("m");

            if ((index > 0 && (salary[index - 1] >= '0' && salary[index - 1] <= '9')))
                return 1000000;

            return 1;
        }

        private static double PeriodCoeff(string salary)
        {
            if (salary.Contains("day") || salary.Contains("p/d"))
                return 21.0;
            if (salary.Contains("month") || salary.Contains("руб"))
                return 1.0;
            return 1.0 / 12.0;
        }

        private static bool isValueelement(char c)
        {
            return (c >= '0' && c <= '9') || c == 'k' || c == 'm' || c == '.';
        }

        private static double NumericValue(string value)
        {
            var array = value.Where(c => (c >= '0' && c <= '9') || c == '.').ToArray();
            return double.Parse(new string(array).Replace('.', ','));
        }

        private static bool isnumericCoef(string value, int index)
        {
            if ( value[index] != 'k' && value[index] != 'm')
                return true;
            return ((index == 0 || (value[index - 1] >= '0' && value[index - 1] <= '9'))
                || (index == value.Length - 1 || (value[index + 1] >= '0' && value[index + 1] <= '9')))
                && (value.Length > 1);
        }

        private static bool isnumericDot(string value,int index)
        {
            if (value[index] != '.')
                return true;
            return (index == 0 || (value[index - 1] >= '0' && value[index - 1] <= '9'))
                && (index == value.Length - 1 || (value[index + 1] >= '0' && value[index + 1] <= '9'))
                && (value.Length > 1);
        }

        public static double GetValue(string salary)
        {
            var replaceTo = RemoveTags(salary).Replace("—", "-").Replace(" to ", "-").Replace("/", "-");
            var nums = new List<string>();

            int index = 0;
            while (index < replaceTo.Length)
            {
                var builder = new StringBuilder();
                while (index < replaceTo.Length && (!isValueelement(replaceTo[index]) || !(isnumericDot(replaceTo, index) && isnumericCoef(replaceTo, index))))
                    index++;
                while (index < replaceTo.Length && isValueelement(replaceTo[index]))
                {
                    builder.Append(replaceTo[index]);
                    index++;
                    if (index + 1 < replaceTo.Length && replaceTo[index] == ' ' 
                        && isValueelement(replaceTo[index+1]))
                        index++;
                }
                nums.Add(builder.ToString());
                if (replaceTo.Substring(index).Contains("-") && nums.Count <= 1)
                {
                    index = replaceTo.IndexOf('-', index) + 1;
                }
                else
                    break;
            }
            nums.RemoveAll(string.IsNullOrEmpty);

            double result = 0;
            if (nums.Count == 2)
            {
                var firstCurency = Coeff(nums[0]);
                var secondCurency = Coeff(nums[1]);
                if (firstCurency != 1 && secondCurency != 1)
                    result = (NumericValue(nums[0]) * firstCurency + NumericValue(nums[1]) * secondCurency) / 2;
                else if (!(firstCurency == 1 && secondCurency == 1) && (firstCurency == 1 || secondCurency == 1))
                {
                    var coef = firstCurency != 1 ? firstCurency : secondCurency;
                    result = (NumericValue(nums[0]) * coef + NumericValue(nums[1]) * coef) / 2;
                }
                else
                    result = (NumericValue(nums[0]) + NumericValue(nums[1])) / 2;
            }
            else
                result = Coeff(nums[0]) * NumericValue(nums[0]);

            var krotch = salary.Contains("million") ? 1000000.0 : 1.0;
            result *= Curency(salary) * PeriodCoeff(salary) * krotch;
            return result;
        }
    }
}
