using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VacancyParcer.Reporter.Models
{
    public class AnalisisViewModel
    {
        public string TotalJsonData { get; set; }
        public AnalisisInfo TotalInfo { get; set; }
        public Dictionary<string, string> JsonData { get; set; }
        public Dictionary<string, AnalisisInfo> SubInfo { get; set; }
        public Dictionary<string, string> SpaceClusterData { get; set; }
        public Dictionary<string, string> TimeClusterData { get; set; }
    }
}