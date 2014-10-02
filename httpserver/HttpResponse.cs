using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace httpserver
{
    class HttpResponse
    {
        public DateTime LastModifiedDate { get; set; }
        public long ContentLength { get; set; }
        public string ContentType { get; set; }
        public FileInfo LocalFileInfo { get; set; }

        public HttpResponse(string file)
        {
            ContentTypeHandler contentTypeHandler = new ContentTypeHandler();

            LocalFileInfo = new FileInfo(file);
            LastModifiedDate = LocalFileInfo.LastWriteTime;
            ContentLength = LocalFileInfo.Length;
            ContentType = contentTypeHandler.ContentType(file);

        }

        public void Send(Stream networkStream)
        {
            StreamWriter streamWriter = new StreamWriter(networkStream);
            
            streamWriter.Write(
            "HTTP/1.0 200 OK\r\n" +
            "Date: {0}\r\n" +
            "Server: {1}\r\n" +
            "Last-Modified: {2}\r\n" +
            "Content-Type: {3}\r\n" +
            "Content-Length: {4}\r\n" +
            "\r\n",
            DateTime.Now, HttpServer.ServerVersion, LastModifiedDate, ContentType, ContentLength);
            
            using (FileStream file = File.OpenRead(LocalFileInfo.FullName))
            {
                file.CopyTo(networkStream);
            }

            EventLogging.WriteToLog("Send response to client", "Information");
        }

        public static void SendError(Stream networkStream, string httpCode)
        {
            StreamWriter streamWriter = new StreamWriter(networkStream);
            streamWriter.Write(
                        "HTTP/1.0 {0}\r\n" +
                        "Date: {1}\r\n" +
                        "Server: {2}\r\n" +
                        "\r\n",
                        httpCode, DateTime.Now, HttpServer.ServerVersion);
            streamWriter.Close();
        }
    }
}
