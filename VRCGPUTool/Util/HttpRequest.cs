using System;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;

namespace VRCGPUTool.Util
{
    internal class HttpRequest
    {
        public static HttpClient client;

        public void HttpClientUtil()
        {
            client = new HttpClient();
        }
    }
}
