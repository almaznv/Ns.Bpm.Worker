using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Diagnostics;

namespace Ns.BpmOnline.Worker
{
    public static class Logger
    {
        private static string logJournalName = "Application";
        private static string _workerName = ConfigurationManager.AppSettings["workerName"];

        public static void Log(string message)
        {
            using (EventLog eventLog = new EventLog(logJournalName))
            {
                eventLog.Source = _workerName;
                eventLog.WriteEntry(message, EventLogEntryType.Information, 0, 1);
            }

        }
    }
}
