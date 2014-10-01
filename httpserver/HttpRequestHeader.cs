using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace httpserver
{
    class HttpRequestHeader
    {
        private string _function;
        private string _filePath;
        private string _httpVersion;
        private string _localFilePath;

        public string Function
        {
            get { return _function; }
            set
            {
                if (value == "GET" || value == "HEAD" || value == "POST")
                {
                    _function = value;
                }
                else
                {
                    throw new Exception("400 Illegal request");
                }
            }
        }
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                if (value == "/")
                {
                    value = "/index.html";
                }
                _filePath = value;
                LocalFilePath = value;
            }
        }
        public string HttpVersion
        {
            get { return _httpVersion; }
            set
            {
                if (value == "HTTP/1.0" || value == "HTTP/1.1")
                {
                    _httpVersion = value;
                }
                else
                {
                    throw new Exception("400 Illegal protocol");
                }
            }
        }

        public string LocalFilePath
        {
            get { return _localFilePath; }
            set
            {
                value = GetLocalFilePath(value);
                if (File.Exists(value))
                {
                    _localFilePath = value;
                }
                else
                {
                    throw new Exception("404 Not Found");
                }
            }
        }

        public HttpRequestHeader(string headerStatusLine)
        {
            if (!ValidRequest(headerStatusLine))
            {
                throw new Exception("400 Illegal request");
            }

            string[] headerContents = headerStatusLine.Split(' ');
            Function = headerContents[0];
            FilePath = headerContents[1];
            HttpVersion = headerContents[2];
        }

        private string GetLocalFilePath(string message)
        {
            message = message.Replace('/', '\\');
            message = message.Trim('\\');

            message = Path.Combine(HttpServer.RootCatalog, message);

            return message;
        }

        /// <summary>
        /// Method that validates the format of the HTTP request header and returns a true or false.
        /// </summary>
        /// <param name="requestHeader">The request header to be validated</param>
        /// <returns></returns>
        private bool ValidRequest(string requestHeader)
        {
            return Regex.IsMatch(requestHeader, @"^[A-Z]*\s\/(.)*\sHTTP/\d.\d");
        }


    }
}
