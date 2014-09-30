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
        /// <summary>
        /// DefaultPort defines what port the server will listen on
        /// </summary>
        public static readonly int DefaultPort = 8888;
        /// <summary>
        /// Used to define whether the server is running or not
        /// </summary>
        private bool _serverRunning;
        /// <summary>
        /// Used in the HTTP header response to define the server version
        /// </summary>
        private static readonly string _serverVersion = "MartPet Server 0.1";
        /// <summary>
        /// Defines where the root of the server files are stored
        /// </summary>
        private static readonly string _rootCatalog = @"C:\temp\";

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
                    string fullFilePath = Path.Combine(_rootCatalog, GetRequestedFilePath(message));

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

        /// <summary>
        /// Stops the server by setting the value of the field '_serverRunning' to false
        /// </summary>
        public void StopServer()
        {
            _serverRunning = false;
        }

        /// <summary>
        /// This method is used to split the HTTP GET request, so that we only get the URI of the requested file, 
        /// and combine the result with the _rootCatalog field when it's used.
        /// </summary>
        /// <param name="message">The 'message' parameter is basically HTTP GET request send from the client</param>
        /// <returns>Returns the HTTP GET request URI, where it's been splitted and certain characters has been replaced</returns>
        private string GetRequestedFilePath(string message)
        {
            string[] messageWords = message.Split(' ');
            string result = messageWords[1];

            result = result.Replace('/', '\\');
            result = result.Trim('\\');

            return result;
        }

        private void SendRequestedFile(string filePath, StreamWriter streamWriter)
        {
            using (FileStream source = File.OpenRead(filePath))
            {
                byte[] bytes = new byte[1024];
                UTF8Encoding temp = new UTF8Encoding(true);
                while (source.Read(bytes, 0, bytes.Length) > 0)
                {
                    streamWriter.Write(temp.GetString(bytes));
                    Console.WriteLine(temp.GetString(bytes));
                }
            }
        }

        /// <summary>
        /// This method is used to make the process of sending/writing the HTTP response easier. 
        /// </summary>
        /// <param name="streamWriter">The StreamWriter is used to be able to write to the socket</param>
        /// <param name="lastModifieDateTime">The lastModifiedDateTime is used to get the date the requested file was last modified</param>
        /// <param name="contentLength">contentLength is used to 'calculate' the size of the requested file</param>
        private void SendHeader(StreamWriter streamWriter, DateTime lastModifieDateTime, long contentLength)
        {
            streamWriter.Write(
                            "HTTP/1.0 200 OK\r\n" +
                            "Date: {0}\r\n" +
                            "Server: {1}\r\n" +
                            "Last-Modified: {2}\r\n" +
                            "Content-Type: text/html\r\n" +
                            "Content-Length: {3}\r\n" +
                            "\r\n",
                            DateTime.Now, _serverVersion, lastModifieDateTime, contentLength);
        }
    }
}