using System.Diagnostics;

namespace httpserver
{
    public class EventLogging
    {
        private const string MachineName = "."; // This computer
        public const string Source = "MartPet HTTP Server";
        public const string LogName = "Application";
        private const int MyId = 1452;

        public static void WriteToLog(string logMessage, string entryType)
        {
            using (EventLog log = new EventLog(LogName, MachineName, Source))
            {
                if (entryType == "Information")
                {
                    log.WriteEntry(logMessage, EventLogEntryType.Information, MyId);
                }
                else if (entryType == "Error")
                {
                    log.WriteEntry(logMessage, EventLogEntryType.Error, MyId + 1);
                }
                else if (entryType == "Warning")
                {
                    log.WriteEntry(logMessage, EventLogEntryType.Warning, MyId + 2);
                }
            }
        }
    }
}
