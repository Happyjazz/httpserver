using System;
using System.IO;
using System.Net;

namespace httpserver
{
    class HtmlContentCatalog
    {
        /// <summary>
        /// This method creates a content catalog to show the contents of a web-directory, in case there are no welcome-file.
        /// </summary>
        /// <param name="networkStream">Is the stream that is being used to send the data to the browser.</param>
        /// <param name="directory">The URI that are being asked for.</param>
        public static void SendContentCatalog(Stream networkStream, string directory)
        {
            string fullDirectory = Path.Combine(HttpServer.RootCatalog, directory);
            string[] files = Directory.GetFiles(fullDirectory);
            string[] folders = Directory.GetDirectories(fullDirectory);

            StreamWriter streamWriter = new StreamWriter(networkStream);

            streamWriter.Write(
            "HTTP/1.0 200 OK\r\n" +
            "Date: {0}\r\n" +
            "Server: {1}\r\n" +
            "Content-Type: text/html\r\n" +
            "\r\n",
            DateTime.Now, HttpServer.ServerVersion);

            streamWriter.Write("<!DOCTYPE html PUBLIC \"-//IETF//DTD HTML 2.0//EN\">" +
                               "<HTML><HEAD><TITLE>Content of /{0}</TITLE></HEAD>" +
                               "<BODY><H1>Hi</H1><P>This is the content " +
                               "of the directory</P><UL>", Path.GetFileName(directory));

            foreach (string folder in folders)
            {
                streamWriter.Write("<LI>Folder: <A HREF=\"./{0}\">{0}</A></LI>", WebUtility.HtmlEncode(Path.GetFileName(folder)));
            }

            streamWriter.Write("<BR>");

            foreach (string file in files)
            {
                streamWriter.Write("<LI>File: <A HREF=\"./{0}\">{0}</A></LI>", WebUtility.HtmlEncode(Path.GetFileName(file)));
            }

            streamWriter.Write("</UL></BODY></HTML>");

            streamWriter.Close();

        }
    }
}
