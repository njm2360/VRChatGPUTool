using System.Net.Http;

namespace VRCGPUTool.Util
{
    internal class HttpRequest
    {
        public static readonly HttpClient client;

        static HttpRequest()
        {
            client = new HttpClient();
        }
    }
}
