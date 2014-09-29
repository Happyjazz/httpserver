using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
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
                try
                {
                    StreamWriter streamWriter = new StreamWriter(networkStream);
                    streamWriter.AutoFlush = true;

                    string message = streamReader.ReadLine();

                    string returnContent = "";

                    if (message != null)
                    {
                        returnContent = String.Format("You requested the file: {0}", GetRequestedFile(message));
                    }

                    streamWriter.Write(
                        "HTTP/1.0 200 OK\r\n" +
                        "Date: {0}\r\n" +
                        "Server: {1}\r\n" +
                        "MIME-version: 1.0\r\n" +
                        "Last-Modified: {2}\r\n" +
                        "Content-Type: text/html\r\n" +
                        "Content-Length: {3}\r\n" +
                        "\r\n{4}",
                        DateTime.Now, _serverVersion, DateTime.Now, returnContent.Length, returnContent);
                    Console.WriteLine(message);
                    streamReader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally 
                {
                    networkStream.Close();
                    tcpClient.Close();
                }
            }
            tcpListener.Stop();
        }

        public void StopServer()
        {
            _serverRunning = false;
        }

        private string GetRequestedFile(string message)
        {
            string[] messageWords = message.Split(' ');
            
            return messageWords[1];
        }
    }
}