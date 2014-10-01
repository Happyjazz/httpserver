using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace httpserver
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!EventLog.SourceExists(EventLogging.Source))
            {
                EventLog.CreateEventSource(EventLogging.Source, EventLogging.LogName);
            }

            Console.WriteLine("Hello http server");

            HttpServer server = new HttpServer();
            server.StartServer();
        }
    }
}
