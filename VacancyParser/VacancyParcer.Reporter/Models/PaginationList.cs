using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VacancyParcer.Reporter.Models
{
    public class PaginationList<T> : IEnumerable<T>
    {
        private IEnumerable<T> _list;
        public static int[] Perpages
        {
            get
            {
                return new[] { 25, 50, 100, 250, 500, 1000 };
            }
        }

        public int SelectedPage { get; private set; }
        public int SelectedPerpage { get; private set; }
        public int Totalpages{get;private set;}
        

        public PaginationList(IEnumerable<T> sequence, ModelFilter filter)
        {
            _list = sequence.Skip((filter.page - 1) * filter.size).Take(filter.size);
            SelectedPage = filter.page;
            SelectedPerpage = filter.size;
            Totalpages = (int)Math.Ceiling((decimal)sequence.Count() / SelectedPerpage);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (_list as System.Collections.IEnumerable).GetEnumerator();
        }
    }
}