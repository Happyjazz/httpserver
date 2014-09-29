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

        public void StartServer()
        {
            TcpListener tcpListener = new TcpListener(DefaultPort);
            tcpListener.Start();

            TcpClient tcpClient = tcpListener.AcceptTcpClient();
            Console.WriteLine("Server activated");

            Stream networkStream = tcpClient.GetStream();

            StreamReader streamReader = new StreamReader(networkStream);
            StreamWriter streamWriter = new StreamWriter(networkStream);
            streamWriter.AutoFlush = true;

            string message = streamReader.ReadLine();
            string answer = "";

            while (message != null && message != "")
            {
                Console.WriteLine("Client: " + message);
                //answer = "Message received: \"" + message + "\"";
                answer = "Hello World";

                streamWriter.WriteLine(answer);
                message = streamReader.ReadLine();

            }

            networkStream.Close();
            tcpClient.Close();
            tcpListener.Stop();

        }

    }
}
