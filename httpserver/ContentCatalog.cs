using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace httpserver
{
    class ContentCatalog
    {
        public static void SendContentCatalog(Stream networkStream, string directory)
        {
            string fullDirectory = Path.Combine(HttpServer.RootCatalog, directory);
            string[] files = Directory.GetFiles(fullDirectory);

            StreamWriter streamWriter = new StreamWriter(networkStream);

            streamWriter.Write(
            "HTTP/1.0 200 OK\r\n" +
            "Date: {0}\r\n" +
            "Server: {1}\r\n" +
            "Content-Type: text/html\r\n" +
            "\r\n",
            DateTime.Now, HttpServer.ServerVersion);

            streamWriter.Write("<!DOCTYPE html PUBLIC \"-//IETF//DTD HTML 2.0//EN\">" +
                               "<HTML><HEAD><TITLE>Content of Directory </TITLE></HEAD>" +
                               "<BODY><H1>Hi</H1><P>This is the content " +
                               "of the directory</P><UL>");

            foreach (string file in files)
            {
                streamWriter.Write("<LI><A HREF=\"./{0}\">{0}</A></LI>", WebUtility.HtmlEncode(Path.GetFileName(file)));
            }

            streamWriter.Write("</UL></BODY></HTML>");

            streamWriter.Close();

        }
    }
}
