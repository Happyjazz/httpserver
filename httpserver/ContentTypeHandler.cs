using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace httpserver
{
    public class ContentTypeHandler
    {
        Dictionary<string, string> contentTypeDictionary = new Dictionary<string, string>(); 

        /// <summary>
        /// This method returns the content-type of a provided file.
        /// </summary>
        /// <param name="requestedFile">The file which are going to have its content-type returned</param>
        /// <returns></returns>
        public string ContentType(string requestedFile)
        {
            contentTypeDictionary.Add(".html", "text/html");
            contentTypeDictionary.Add(".htm", "text/html");
            contentTypeDictionary.Add(".gif", "image/gif");
            contentTypeDictionary.Add(".jpg", "image/jpeg");
            contentTypeDictionary.Add(".jpeg", "image/jpeg");
            contentTypeDictionary.Add(".doc", "application/msword");
            contentTypeDictionary.Add(".docx", "application/msword");
            contentTypeDictionary.Add(".pdf", "application/pdf");
            contentTypeDictionary.Add(".css", "text/css");
            contentTypeDictionary.Add(".xml", "text/xml");
            contentTypeDictionary.Add(".jar", "application/x-java-archive");
            contentTypeDictionary.Add("octet-stream", "application/octet-stream");

            string extension = Path.GetExtension(requestedFile);

            if (!contentTypeDictionary.ContainsKey(extension))
            {
                return contentTypeDictionary["octet-stream"];
            }
             
            return contentTypeDictionary[extension];
        }
    }
}
