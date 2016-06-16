using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VacancyParcer.Reporter.Models
{

    public class ClasterInfo
    {
        public double Min{get;set;}
        public double Max{get;set;}
        public double Average{get;set;}
        public int Count{get;set;}
        public double Pers{get;set;}
        public double FullPers{get;set;}
    }


    public class AnalisisInfo
    {
        public int FullCount { get; set; }
        public int Count { get; set; }
        public Dictionary<string, ClasterInfo> Info { get; set; }
    }
}