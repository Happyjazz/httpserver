using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace httpserver
{
    class Program
    {
        static void Main(string[] args)
        {
            //This if-sentence checks if the source-name required for logging, already exists in the windows-log
            if (!EventLog.SourceExists(EventLogging.Source))
            {
                EventLog.CreateEventSource(EventLogging.Source, EventLogging.LogName);
            }

            //Welcome text for the server
            Console.WriteLine("Hello http server");
            
            //Instantiation and start of the http-server
            HttpServer server = new HttpServer();
            Parallel.Invoke(server.StartServer, server.StopServer);
            
        }
    }
}
