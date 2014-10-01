using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Net;
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
            EventLogging.WriteToLog("Server started succesfully", "Information");

            List<Task> serverThreads = new List<Task>();

            while (_serverRunning)
            {
                TcpClient tcpClient = tcpListener.AcceptTcpClient();
                Task task = Task.Run(() => NewConnection(tcpClient));
                serverThreads.Add(task);
            }
            tcpListener.Stop();
        }

        /// <summary>
        /// Stops the server by setting the value of the field '_serverRunning' to false
        /// </summary>
        public void StopServer()
        {
            _serverRunning = false;
            EventLogging.WriteToLog("Server has stopped", "Information");
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
                EventLogging.WriteToLog("Server accepted request from client: \n" + httpStatusLine, "Information");
                
                HttpRequestHeader httpHeader = new HttpRequestHeader(httpStatusLine);

                FileInfo fileInfo = new FileInfo(httpHeader.LocalFilePath);

                ContentTypeHandler contentTypeHandler = new ContentTypeHandler();
               
                SendHeader(streamWriter, fileInfo.LastWriteTime,fileInfo.Length, contentTypeHandler.ContentType(httpHeader.LocalFilePath));
                SendRequestedFile(httpHeader.LocalFilePath, streamWriter);
                EventLogging.WriteToLog("Send response to client", "Information");
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
        /// <param name="lastModifieDateTime">The lastModifiedDateTime is used to get the date the requested file was last modified</param>
        /// <param name="contentLength">contentLength is used to 'calculate' the size of the requested file</param>
        /// <param name="contentTypeHandler"></param>
        /// <param name="contentType"></param>
        private void SendHeader(StreamWriter streamWriter, DateTime lastModifieDateTime, long contentLength, string contentType)
        {
            streamWriter.Write(
                            "HTTP/1.0 200 OK\r\n" +
                            "Date: {0}\r\n" +
                            "Server: {1}\r\n" +
                            "Last-Modified: {2}\r\n" +
                            "Content-Type: {4}\r\n" +
                            "Content-Length: {3}\r\n" +
                            "\r\n",
                            DateTime.Now, ServerVersion, lastModifieDateTime, contentLength, contentType);
        }

        /// <summary>
        /// Overload of the SendHeader method, for sending error codes.
        /// </summary>
        /// <param name="streamWriter">The StreamWriter is used to be able to write to the socket</param>
        /// <param name="httpCode">Is used to send the HTTP status code i.e. 200 OK</param>
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