using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace httpserver
{
    class Program
    {
        static void Main(string[] args)
        {
            //Welcome text for the server
            Console.WriteLine("Hello http server");

            //Instantiation and start of the http-server
            HttpServer server = new HttpServer();
            Parallel.Invoke(server.StartServer, server.StopServerListener);
        }
    }
}
