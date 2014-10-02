using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace httpserver
{
    class HttpRequestHeader
    {
        private string _function;
        private string _filePath;
        private string _httpVersion;
        private string _localFilePath;

        #region Properties
        /// <summary>
        /// This property contains the funtion of the request.
        /// Validation of the function is done, before assigning it to the backing field.
        /// </summary>
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

        /// <summary>
        /// This property contains the URI of the requested file. 
        /// The property checks if the root URI is provided and redirects it to index.html, if that is the case.
        /// </summary>
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                value = WebUtility.UrlDecode(value);

                if (value == "/")
                {
                    if (File.Exists(GetLocalFilePath("index.html")))
                    {
                        value = "/index.html";
                        
                    } 
                    else if (File.Exists(GetLocalFilePath("index.htm")))
                    {
                        value = "/index.htm";
                    }
                }
                _filePath = value;
                LocalFilePath = value;
            }
        }

        /// <summary>
        /// This property checks if the requested HTTP version is valid, before assigning its value to the backing field.
        /// </summary>
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

        /// <summary>
        /// This property contains the full local path of the requested file.
        /// Before assigning the value to the private backing field, the property checks if the file actually exists.
        /// </summary>
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
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor of the class that takes the request header as a parameter.
        /// First the header validates the request and then proceeds to parse it and assign its values to the class' properties.
        /// </summary>
        /// <param name="headerStatusLine"></param>
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
        #endregion

        #region Private Methods
        /// <summary>
        /// Method that returns the full local path of a file, by combining the URI with the RootCatalog.
        /// </summary>
        /// <param name="message">URI to be converted to local filepath</param>
        /// <returns></returns>
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
        #endregion

    }
}
