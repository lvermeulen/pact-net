using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using PactNet.Mocks.MockHttpService.Http;

namespace PactNet.Mocks.MockHttpService.Kestrel.Http
{
    public class KestrelHttpRequestMessage : IMockHttpRequestMessage
    {
        private static readonly IDictionary<string, HttpMethod> s_methodByString = new Dictionary<string, HttpMethod>
        {
            { "GET", HttpMethod.Get },
            { "POST", HttpMethod.Post },
            { "PUT", HttpMethod.Put },
            { "DELETE", HttpMethod.Delete },
            { "HEAD", HttpMethod.Head },
            { "PATCH", new HttpMethod("PATCH") }
        };

        private static string StringByMethod(HttpMethod httpMethod) => s_methodByString.FirstOrDefault(x => x.Value == httpMethod).Key;

        public static implicit operator HttpRequestMessage(KestrelHttpRequestMessage kestrelHttpRequestMessage) => new HttpRequestMessage(
            s_methodByString[kestrelHttpRequestMessage.Method],
            kestrelHttpRequestMessage.Uri
        );

        public static implicit operator KestrelHttpRequestMessage(HttpRequestMessage httpRequestMessage) => new KestrelHttpRequestMessage(httpRequestMessage);

        private readonly HttpRequestMessage _httpRequestMessage;

        public KestrelHttpRequestMessage(HttpRequestMessage httpRequestMessage)
        {
            _httpRequestMessage = httpRequestMessage;
        }

        public KestrelHttpRequestMessage(HttpMethod method, Uri requestUri)
        {
            _httpRequestMessage = new HttpRequestMessage(method, requestUri);
        }

        public string Method => StringByMethod(_httpRequestMessage.Method);
        public string Path => _httpRequestMessage.RequestUri.AbsolutePath;
        public Uri Uri => _httpRequestMessage.RequestUri;
        public Stream Body => _httpRequestMessage.Content.ReadAsStreamAsync().Result;
        public IDictionary<string, IEnumerable<string>> Headers => _httpRequestMessage.Headers.ToDictionary(x => x.Key, x => x.Value);
    }
}
