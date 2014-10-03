using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using httpserver;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace httpserverTest
{
    [TestClass]
    public class HttpServerTest
    {
        private const string CrLf = "\r\n";
        private static HttpServer _server;

        [ClassInitialize]
        public static void StartServer(TestContext context)
        {
            _server = new HttpServer();
            Task.Factory.StartNew(_server.StartServer);
        }

        [TestMethod]
        public void TestGet()
        {
            String line = GetFirstLine("GET /index.html HTTP/1.0");
            Assert.AreEqual("HTTP/1.0 200 OK", line);

            line = GetFirstLine("GET /fileDoesNotExist.txt HTTP/1.0");
            Assert.AreEqual("HTTP/1.0 404 Not Found", line);
        }

        [TestMethod]
        public void TestGetIllegalRequest()
        {
            String line = GetFirstLine("GET /index.html HTTP 1.0");
            Assert.AreEqual("HTTP/1.0 400 Illegal request", line);
        }

        [TestMethod]
        public void TestGetIllegalMethodName()
        {
            String line = GetFirstLine("PLET /index.html HTTP/1.0");
            Assert.AreEqual("HTTP/1.0 400 Illegal request", line);
        }

        [TestMethod]
        public void TestGetIllegalProtocol()
        {
            String line = GetFirstLine("GET /index.html HTTP/1.2");
            Assert.AreEqual("HTTP/1.0 400 Illegal protocol", line);
        }

        [ClassCleanup]
        public static void StopServer()
        {
            _server.StopServer();
        }

        /// <summary>
        /// Private helper method
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static String GetFirstLine(String request)
        {
            TcpClient client = new TcpClient("localhost", HttpServer.DefaultPort);
            NetworkStream networkStream = client.GetStream();

            StreamWriter toServer = new StreamWriter(networkStream, Encoding.UTF8);
            toServer.Write(request + CrLf);
            toServer.Write(CrLf);
            toServer.Flush();

            StreamReader fromServer = new StreamReader(networkStream);
            String firstline = fromServer.ReadLine();
            toServer.Close();
            fromServer.Close();
            client.Close();
            return firstline;
        }
    }
}
