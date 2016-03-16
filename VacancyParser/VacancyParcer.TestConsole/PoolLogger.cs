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

        private Queue<InputData> _debugQueue = new Queue<InputData>();
        private Queue<InputData> _infoQueue = new Queue<InputData>();
        private Queue<InputData> _errorQueue = new Queue<InputData>();

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
        
        private void SaveLog(Queue<InputData> queue,string file)
        {
            lock (_saveLock)
            {
                var debugData = queue.Select(el => string.Format("{0:G}|{1}", el.Date, el.Message));
                System.IO.File.AppendAllLines(file, debugData);
            }
        }

        private void _saveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SaveLog(_debugQueue, DebugFile);
            SaveLog(_infoQueue, InfoFile);
            SaveLog(_errorQueue, ErrorFile);
        }

        private void AddEvent(MessageType type,string message)
        {
            Queue<InputData> queue=null;
            switch(type)
            {
                case MessageType.Debug:queue = _debugQueue;break;
                case MessageType.Info: queue = _infoQueue; break;
                case MessageType.Error: queue = _errorQueue; break;
            }
            lock(_saveLock)
                queue.Enqueue(new InputData { Message = message, Date = DateTime.Now });
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
    }
}
