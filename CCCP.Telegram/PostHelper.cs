using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace CCCP.Telegram
{
    public class PostHelper
    {
        public static void PostCount(string postUrl, int counter)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(postUrl);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                var json = JObject.FromObject(new { Counter = counter});
                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using var streamReader = new StreamReader(httpResponse.GetResponseStream());
            streamReader.ReadToEnd();
        }
    }
}
