using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace httpserver
{
    public class EventLogging
    {
        private const string machineName = "."; // This computer
        public const string Source = "MartPet HTTP Server";
        public const string logName = "Application";
        private const int myId = 1452;

        public static void WriteToLog(string logMessage, string entryType)
        {
            using (EventLog log = new EventLog(logName, machineName, Source))
            {
                if (entryType == "Information")
                {
                    log.WriteEntry(logMessage, EventLogEntryType.Information, myId);
                }
                else if (entryType == "Error")
                {
                    log.WriteEntry(logMessage, EventLogEntryType.Error, myId + 1);
                }
                else if (entryType == "Warning")
                {
                    log.WriteEntry(logMessage, EventLogEntryType.Warning, myId + 2);
                }
            }
        }
    }
}
