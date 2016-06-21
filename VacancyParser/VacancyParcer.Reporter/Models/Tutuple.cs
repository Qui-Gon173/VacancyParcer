using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VacancyParcer.Reporter.Models
{
    public class Tututple<T>
    {
        public Tuple<T[], T[], Tuple<string, T>[], double> Value { get; private set; }

        public Tututple(T[] data, T[] dataForStuding, Tuple<string, T>[] classifyData, double errors)
        {
            Value = new Tuple<T[], T[], Tuple<string, T>[], double>(data, dataForStuding, classifyData, errors);
        }
    }
}