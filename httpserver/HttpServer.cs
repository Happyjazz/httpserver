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
        #region Static properties
        /// <summary>
        /// Used in the HTTP header response to define the server version
        /// </summary>
        public static readonly string ServerVersion = "MartPet Server 0.1";

        /// <summary>
        /// DefaultPort defines what port the server will listen on
        /// </summary>
        public static readonly int DefaultPort = 8888;
        
        /// <summary>
        /// The port used for terminating the http-server
        /// </summary>
        public static readonly int KillPort = 9999;
        
        /// <summary>
        /// Defines where the root of the server files are stored
        /// </summary>
        public static readonly string RootCatalog = @"C:\temp\";
        #endregion

        #region Private properties
        /// <summary>
        /// Used to define whether the server is running or not
        /// </summary>
        private bool _serverRunning;

        /// <summary>
        /// List for handling the tasks of incoming connections
        /// </summary>
        private List<Task> serverThreads = new List<Task>();
        #endregion

        #region Public Methods
        /// <summary>
        /// This method starts the HTTP listener.
        /// </summary>
        /// <remarks>The HTTP server starts a TCP listener on the defined DefaultPort.
        /// When a client connects on the DefaultPort a new thread is made to handle the new connection in the NewConnection method</remarks>
        public void StartServer()
        {
            _serverRunning = true;

            TcpListener tcpListener = new TcpListener(IPAddress.Any, DefaultPort);
            tcpListener.Start();
            EventLogging.WriteToLog("Server started succesfully", "Information");

            while (_serverRunning)
            {
                TcpClient tcpClient = tcpListener.AcceptTcpClient();
                Task task = Task.Run(() => NewConnection(tcpClient));
                serverThreads.Add(task);
            }
            tcpListener.Stop();
            EventLogging.WriteToLog("Server has stopped", "Information");
        }

        /// <summary>
        /// Method for shutting down the server.
        /// </summary>
        /// <remarks>This method is designed to be run in a parallel thread, with StartServer().
        /// It open a listener on the port assigned to KillPort and as soon as a client connects to that port, the ShutDown() method is called.</remarks>
        public void StopServer()
        {
            TcpListener killListener = new TcpListener(IPAddress.Any, KillPort);
            killListener.Start();
            killListener.AcceptTcpClient();
            killListener.Stop();

            ShutDown();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Method for cleanly shutting down the server.
        /// </summary>
        /// <remarks>This method waits for all tasks to complete, then changes 
        /// the _serverRunning condition to break the while-loop in StartServer() 
        /// and then connect once more to the http server, to make sure the 
        /// listener shuts down.</remarks>
        private void ShutDown()
        {
            Task.WaitAll(serverThreads.ToArray());
            _serverRunning = false;
            TcpClient lastConnection = new TcpClient("localhost", DefaultPort);
            lastConnection.Close();
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
            ContentTypeHandler contentTypeHandler = new ContentTypeHandler();

            try
            {
                string httpStatusLine = streamReader.ReadLine();
                if (httpStatusLine == null)
                {
                    throw new Exception("404 Not found");
                }
                EventLogging.WriteToLog("Server accepted request from client: \n" + httpStatusLine, "Information");
                
                HttpRequestHeader httpHeader = new HttpRequestHeader(httpStatusLine);
                FileInfo fileInfo = new FileInfo(httpHeader.LocalFilePath);
                
               
                SendHeader(streamWriter, fileInfo.LastWriteTime,fileInfo.Length, contentTypeHandler.ContentType(httpHeader.LocalFilePath));
                SendRequestedFile(httpHeader.LocalFilePath, networkStream);
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
        /// <param name="networkStream">The network stream to be used for transferring the content.</param>
        private void SendRequestedFile(string filePath, Stream networkStream)
        {
            using (FileStream source = File.OpenRead(filePath))
            {
                source.CopyTo(networkStream);
            }
        }

        /// <summary>
        /// This method is used to make the process of sending/writing the HTTP response easier. 
        /// </summary>
        /// <param name="streamWriter">The StreamWriter is used to be able to write to the socket</param>
        /// <param name="lastModifieDateTime">The lastModifiedDateTime is used to get the date the requested file was last modified</param>
        /// <param name="contentLength">contentLength is used to 'calculate' the size of the requested file</param>
        /// <param name="contentType">Contains the Content-type of the file being sent</param>
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
        #endregion
    }
}