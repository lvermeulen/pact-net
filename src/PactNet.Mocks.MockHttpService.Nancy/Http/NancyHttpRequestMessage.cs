using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Nancy;
using Nancy.IO;
using PactNet.Mocks.MockHttpService.Http;

namespace PactNet.Mocks.MockHttpService.Nancy.Http
{
    public class NancyHttpRequestMessage : IMockHttpRequestMessage
    {
        private readonly Request _request;

        public static implicit operator Request(NancyHttpRequestMessage nancyHttpRequestMessage) => new Request(
            nancyHttpRequestMessage.Method,
            nancyHttpRequestMessage.Uri,
            new RequestStream(nancyHttpRequestMessage.Body, nancyHttpRequestMessage.Body.Length, disableStreamSwitching: false),
            nancyHttpRequestMessage.Headers
        );

        public static implicit operator NancyHttpRequestMessage(Request nancyRequest) => new NancyHttpRequestMessage(nancyRequest);

        public NancyHttpRequestMessage(Request nancyRequest)
            : this(nancyRequest.Method,
                nancyRequest.Url,
                nancyRequest.Body,
                nancyRequest.Headers as IDictionary<string, IEnumerable<string>>,
                nancyRequest.UserHostAddress,
                nancyRequest.ClientCertificate,
                nancyRequest.ProtocolVersion)
        { }

        public NancyHttpRequestMessage(string method, string path, string scheme)
        {
            _request = new Request(method, path, scheme);
        }

        public NancyHttpRequestMessage(string method, Url url, RequestStream body = null, IDictionary<string, IEnumerable<string>> headers = null, string ip = null, X509Certificate certificate = null, string protocolVersion = null)
        {
            _request = new Request(method, url, body, headers, ip, certificate, protocolVersion);
        }

        public string Method => _request.Method;

        public string Path => _request.Path;

        public Uri Uri => _request.Url;

        public Stream Body => _request.Body;

        public IDictionary<string, IEnumerable<string>> Headers => _request.Headers as IDictionary<string, IEnumerable<string>>;
    }
}
