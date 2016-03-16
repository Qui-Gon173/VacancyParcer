using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VacancyParser.PagesLoader
{
    public interface ILogger
    {
        void Info(string info);
        void Info(string format, params object[] args);
        void Debug(string info);
        void Debug(string format, params object[] args);
        void Error(string info);
        void Error(string format, params object[] args);

        void ForceSave();
    }
}
