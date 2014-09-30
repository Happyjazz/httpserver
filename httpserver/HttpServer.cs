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
        private const string ServerVersion = "MartPet Server 0.1";
        private const string RootCatalog = @"C:\temp\";

        /// <summary>
        /// This method starts the HTTP listener.
        /// </summary>
        /// <remarks>The HTTP server starts a TCP listener on the defined DefaultPort.
        /// When a client connects on the DefaultPort, the received text stream is read with the StreamReader class, 
        /// parsed and a proper response is sent back</remarks>
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
                    StreamWriter streamWriter = new StreamWriter(networkStream) {AutoFlush = true};

                    string message = streamReader.ReadLine();
                    string fullFilePath = Path.Combine(RootCatalog, GetRequestedFilePath(message));

                    if (!File.Exists(fullFilePath))
                    {
                        streamWriter.Write("HTTP/1.0 404 Not Found\r\n");
                    }
                    else
                    {
                        FileInfo fileInfo = new FileInfo(fullFilePath);

                        if (message != null)
                        {
                            SendHeader(streamWriter, fileInfo.LastWriteTime, fileInfo.Length);
                            SendRequestedFile(fullFilePath, streamWriter);
                        }

                        Console.WriteLine(message);
                    }
                    
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

        private string GetRequestedFilePath(string message)
        {
            string[] messageWords = message.Split(' ');
            string result = messageWords[1];
            
            result = result.Replace('/', '\\');
            result = result.Trim('\\');

            return result;
        }
        /// <summary>
        /// This method is used for transferring the content of a requested HTML file to a web client.
        /// </summary>
        /// <param name="filePath">The full local path of the file to be transferred.</param>
        /// <param name="streamWriter">The StreamWriter object used for sending the file.</param>
        private void SendRequestedFile(string filePath, StreamWriter streamWriter)
        {
            using (FileStream source = File.OpenRead(filePath))
            {
                byte[] bytes = new byte[1024];
                UTF8Encoding temp = new UTF8Encoding(true);
                while (source.Read(bytes,0,bytes.Length)>0)
                {
                    streamWriter.Write(temp.GetString(bytes));
                    Console.WriteLine(temp.GetString(bytes));
                }
            }
        }

        private void SendHeader(StreamWriter streamWriter, DateTime lastModifieDateTime, long contentLength )
        {
            streamWriter.Write(
                            "HTTP/1.0 200 OK\r\n" +
                            "Date: {0}\r\n" +
                            "Server: {1}\r\n" +
                            "Last-Modified: {2}\r\n" +
                            "Content-Type: text/html\r\n" +
                            "Content-Length: {3}\r\n" +
                            "\r\n",
                            DateTime.Now, ServerVersion, lastModifieDateTime, contentLength);
        }


    }
}