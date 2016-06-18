using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VacancyParcer.Reporter.Models
{
    public class IrisData
    {
        private static int _lastId=1;
        public static Dictionary<int, string> ClassIdDictionary { get; private set; }

        public int ClassIndex { get; set; }
        public string ClassName
        {
            get
            {
                return ClassIdDictionary[ClassIndex];
            }
        }
        public double[] Data { get; private set; }

        static IrisData()
        {
            ClassIdDictionary = new Dictionary<int, string>();
        }

        public IrisData(double[] data,string dataType)
        {
            int id;
            if(ClassIdDictionary.ContainsValue(dataType))
            {
                id = ClassIdDictionary.First(el => el.Value.Equals(dataType)).Key;
            }
            else
            {
                id = _lastId;
                ClassIdDictionary.Add(_lastId, dataType);
                _lastId++;
            }
            Data = data;
            ClassIndex = id;
        }
    }
}