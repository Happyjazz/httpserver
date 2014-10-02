using System.Collections.Generic;
using System.IO;
using System.Configuration;

namespace httpserver
{
    public class ContentTypeHandler
    {
        readonly Dictionary<string, string> _contentTypeDictionary = new Dictionary<string, string>(); 
        public static string DefaultContentType = ConfigurationManager.AppSettings["DefaultContentType"];

        /// <summary>
        /// This method returns the content-type of a provided file.
        /// </summary>
        /// <param name="requestedFile">The file which are going to have its content-type returned</param>
        /// <returns></returns>
        public string ContentType(string requestedFile)
        {
            _contentTypeDictionary.Add(".html", "text/html");
            _contentTypeDictionary.Add(".htm", "text/html");
            _contentTypeDictionary.Add(".gif", "image/gif");
            _contentTypeDictionary.Add(".jpg", "image/jpeg");
            _contentTypeDictionary.Add(".jpeg", "image/jpeg");
            _contentTypeDictionary.Add(".doc", "application/msword");
            _contentTypeDictionary.Add(".docx", "application/msword");
            _contentTypeDictionary.Add(".pdf", "application/pdf");
            _contentTypeDictionary.Add(".css", "text/css");
            _contentTypeDictionary.Add(".xml", "text/xml");
            _contentTypeDictionary.Add(".jar", "application/x-java-archive");
            _contentTypeDictionary.Add("octet-stream", "application/octet-stream");

            string extension = Path.GetExtension(requestedFile);

            if (!_contentTypeDictionary.ContainsKey(extension))
            {
                return _contentTypeDictionary[DefaultContentType];
            }
             
            return _contentTypeDictionary[extension];
        }
    }
}
