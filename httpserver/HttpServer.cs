using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;

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
        public static readonly int DefaultPort = Convert.ToInt32(ConfigurationManager.AppSettings["DefaultPort"]);
        
        /// <summary>
        /// The port used for terminating the http-server
        /// </summary>
        public static int KillPort = Convert.ToInt32(ConfigurationManager.AppSettings["KillPort"]);
        
        /// <summary>
        /// Defines where the root of the server files are stored
        /// </summary>
        public static readonly string RootCatalog = ConfigurationManager.AppSettings["RootCatalog"];
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
            EventLogging.WriteToLog("Server has been shutdown", "Information");
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


            try
            {
                string httpStatusLine = streamReader.ReadLine();
                if (httpStatusLine == null)
                {
                    throw new Exception("404 Not found");
                }
                EventLogging.WriteToLog("Server accepted request from client: \n" + httpStatusLine, "Information");
                
                HttpRequestHeader httpRequest = new HttpRequestHeader(httpStatusLine);
                string localFilePath = httpRequest.LocalFilePath;

                if (localFilePath.EndsWith("\\"))
                {
                    ContentCatalog.SendContentCatalog(networkStream, localFilePath);
                }
                else
                {
                    HttpResponse httpResponse = new HttpResponse(localFilePath);
                    httpResponse.Send(networkStream);
                }
                
                streamReader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                HttpResponse.SendError(networkStream, ex.Message);
            }
            finally
            {
                networkStream.Close();
                tcpClient.Close();
            }
        }
        #endregion
    }
}