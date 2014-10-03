using System.Diagnostics;

namespace httpserver
{
    public class EventLogging
    {
        private const string MachineName = "."; // This computer
        public const string Source = "MartPet HTTP Server";
        public const string LogName = "Application";
        private const int MyId = 1452;
        

        /// <summary>
        /// This static method is used for writing entries in the Windows Event Log.
        /// </summary>
        /// <param name="logMessage">The info-message of the log-entry</param>
        /// <param name="entryType">The type of log entry (Warning, Error or Information)</param>
        public static void WriteToLog(string logMessage, string entryType)
        {
            //This if-sentence checks if the source-name required for logging, already exists in the windows-log
            if (!EventLog.SourceExists(EventLogging.Source))
            {
                EventLog.CreateEventSource(EventLogging.Source, EventLogging.LogName);
            }

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
