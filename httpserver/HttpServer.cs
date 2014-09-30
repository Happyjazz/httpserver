using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
        private const string ServerVersion = "MartPet Server 0.1";

        /// <summary>
        /// Defines where the root of the server files are stored
        /// </summary>
        public static readonly string RootCatalog = @"C:\temp\";

        /// <summary>
        /// This method starts the HTTP listener.
        /// </summary>
        /// <remarks>The HTTP server starts a TCP listener on the defined DefaultPort.
        /// When a client connects on the DefaultPort a new thread is made to handle the new connection in the NewConnection method</remarks>
        public void StartServer()
        {
            _serverRunning = true;

            TcpListener tcpListener = new TcpListener(DefaultPort);
            tcpListener.Start();

            List<Task> ServerThreads = new List<Task>();

            while (_serverRunning)
            {
                TcpClient tcpClient = tcpListener.AcceptTcpClient();
                Task task = Task.Run(() => NewConnection(tcpClient));
                ServerThreads.Add(task);
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
        /// This methods handles the incoming connections on the default port.
        /// </summary>
        /// <param name="tcpClient"></param>
        private void NewConnection(TcpClient tcpClient)
        {
            
            Console.WriteLine("Client connected on thread " + Thread.CurrentThread.GetHashCode());
            Stream networkStream = tcpClient.GetStream();
            StreamReader streamReader = new StreamReader(networkStream);
            StreamWriter streamWriter = new StreamWriter(networkStream) {AutoFlush = true};

            try
            {
                

                string httpStatusLine = streamReader.ReadLine();
                HTTPHeader httpHeader = new HTTPHeader(httpStatusLine);

                FileInfo fileInfo = new FileInfo(httpHeader.LocalFilePath);
               
                SendHeader(streamWriter, fileInfo.LastWriteTime, fileInfo.Length);
                SendRequestedFile(httpHeader.LocalFilePath, streamWriter);

                streamReader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                SendHeader(streamWriter, ex.Message);
            }
            finally
            {
                networkStream.Close();
                tcpClient.Close();
            }

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
                while (source.Read(bytes, 0, bytes.Length) > 0)
                {
                    streamWriter.Write(temp.GetString(bytes));
                }
            }
        }

        /// <summary>
        /// This method is used to make the process of sending/writing the HTTP response easier. 
        /// </summary>
        /// <param name="streamWriter">The StreamWriter is used to be able to write to the socket</param>
        /// <param name="httpCode">Is used to send the HTTP status code i.e. 200 OK</param>
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
                            DateTime.Now, ServerVersion, lastModifieDateTime, contentLength);
        }
        private void SendHeader(StreamWriter streamWriter, string httpCode)
        {
            streamWriter.Write(
                            "HTTP/1.0 {0}\r\n" +
                            "Date: {1}\r\n" +
                            "Server: {2}\r\n" +
                            "\r\n",
                            httpCode, DateTime.Now, ServerVersion);
        }


    }
}