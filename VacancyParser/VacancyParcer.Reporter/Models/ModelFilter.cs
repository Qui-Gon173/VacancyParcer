using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VacancyParcer.Reporter.Models
{
    public class ModelFilter
    {
        public int page { get; set; }
        public int size { get; set; }

        public ModelFilter()
        {
            page = 1;
            size = 25;
        }
    }
}