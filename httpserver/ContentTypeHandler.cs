using System.Collections.Generic;
using System.IO;
using System.Configuration;

namespace httpserver
{
    public class ContentTypeHandler
    {
        //readonly Dictionary<string, string> _contentTypeDictionary = new Dictionary<string, string>(); 
        public static string DefaultContentType = ConfigurationManager.AppSettings["DefaultContentType"];

        /// <summary>
        /// This static method returns the content-type of a provided file.
        /// </summary>
        /// <param name="requestedFile">The file which are going to have its content-type returned</param>
        /// <returns></returns>
        public static string ContentType(string requestedFile)
        {
            string extension = Path.GetExtension(requestedFile);

            if (ConfigurationManager.AppSettings[extension] != "")
            {
                return ConfigurationManager.AppSettings[extension];
            }
            return DefaultContentType;
        }
    }
}
