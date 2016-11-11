using System.Net.Http;
using Microsoft.AspNetCore.Http;
using PactNet.Mocks.MockHttpService.Http;

namespace PactNet.Mocks.MockHttpService.Kestrel.Http
{
    public class KestrelHttpContext : IMockHttpContext
    {
        private HttpRequestMessage _httpRequestMessage;

        // XXX
        //public static implicit operator HttpContext(KestrelHttpContext kestrelHttpContext) => new HttpContext
        //{
            
        //    //Request = kestrelHttpContext.RequestMessage as Request
        //};

        public static implicit operator KestrelHttpContext(HttpContext httpContext) => new KestrelHttpContext();

        public KestrelHttpContext()
        {
            _httpRequestMessage = new HttpRequestMessage();

        }

        public IMockHttpRequestMessage RequestMessage
        {
            get { return (KestrelHttpRequestMessage)_httpRequestMessage; }
            set { _httpRequestMessage = value as HttpRequestMessage; }
        }
    }
}
