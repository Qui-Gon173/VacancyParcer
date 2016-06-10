using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VacancyParser.PagesLoader;
using System.Timers;

namespace VacancyParcer.TestConsole
{
    public class PoolLogger : ILogger
    {
        private enum MessageType{
            Debug,
            Info,
            Error
            }

        private struct InputData
        {
            public string Message { get; set; }
            public DateTime Date { get; set; }
        }

        public string DebugFile { get; private set; }
        public string InfoFile { get; private set; }
        public string ErrorFile { get; private set; }

        private List<InputData> _debugList = new List<InputData>();
        private List<InputData> _infoList = new List<InputData>();
        private List<InputData> _errorList = new List<InputData>();

        private Timer _saveTimer = new Timer(10000);

        private object _saveLock = new object();

        public PoolLogger(string debugFile,string infoFile,string errorFile)
        {
            DebugFile = debugFile;
            InfoFile = infoFile;
            ErrorFile = errorFile;
            _saveTimer.Elapsed += _saveTimer_Elapsed;
            _saveTimer.Start();
        }

        private void SaveLog(List<InputData> logList, string file)
        {
            lock (_saveLock)
            {
                var debugData = logList.Select(el => string.Format("{0:G}|{1}", el.Date, el.Message));
                System.IO.File.AppendAllLines(file, debugData);
                logList.Clear();
            }
        }

        private void _saveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SaveLog(_debugList, DebugFile);
            SaveLog(_infoList, InfoFile);
            SaveLog(_errorList, ErrorFile);
        }

        private void AddEvent(MessageType type,string message)
        {
            List<InputData> logList = null;
            switch(type)
            {
                case MessageType.Debug:logList = _debugList;break;
                case MessageType.Info: logList = _infoList; break;
                case MessageType.Error: logList = _errorList; break;
            }
            lock(_saveLock)
                logList.Add(new InputData { Message = message, Date = DateTime.Now });
            Console.WriteLine("{2}|{0:G}|{1}", DateTime.Now, message,type);
        }

        public void Debug(string info)
        {
            AddEvent(MessageType.Debug, info);
        }

        public void Debug(string format, params object[] args)
        {
            AddEvent(MessageType.Debug, string.Format(format,args));
        }

        public void Error(string info)
        {
            AddEvent(MessageType.Error, info);
        }

        public void Error(string format, params object[] args)
        {
            AddEvent(MessageType.Error, string.Format(format, args));
        }

        public void Info(string info)
        {
            AddEvent(MessageType.Info, info);
        }

        public void Info(string format, params object[] args)
        {
            AddEvent(MessageType.Info, string.Format(format, args));
        }


        public void ForceSave()
        {
            _saveTimer_Elapsed(null, null);
        }
    }
}
