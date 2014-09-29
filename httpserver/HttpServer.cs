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
                string answer = "";

                streamWriter.WriteLine("HTTP/1.0 200 OK");
                message = streamReader.ReadLine();
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
