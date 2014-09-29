using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace httpserver
{
    public class HttpServer
    {
        public static readonly int DefaultPort = 8888;
        private bool _serverRunning;
        private static string _serverVersion = "MartPet Server 0.1";

        public void StartServer()
        {
            _serverRunning = true;
            
            TcpListener tcpListener = new TcpListener(DefaultPort);
            tcpListener.Start();

            while (_serverRunning)
            {
                TcpClient tcpClient = tcpListener.AcceptTcpClient();
                Console.WriteLine("Server activated");
                Stream networkStream = tcpClient.GetStream();

                StreamReader streamReader = new StreamReader(networkStream);
                StreamWriter streamWriter = new StreamWriter(networkStream);
                streamWriter.AutoFlush = true;

                string message = streamReader.ReadLine();

                streamWriter.Write("HTTP/1.0 200 OK\r\nDate: {0}\r\nServer: {1}\r\nMIME-version: 1.0\r\nLast-Modified: {2}\r\nContent-Type: text/html\r\nContent-Length: 12\r\n\r\nHello World!", DateTime.Now, _serverVersion, DateTime.Now);
                Console.WriteLine(message);
                

                networkStream.Close();
                tcpClient.Close();
            }

            tcpListener.Stop();
        }

        public void StopServer()
        {
            _serverRunning = false;
        }

    }
}
