using System;
using System.Collections.Generic;
using System.IO;

namespace PactNet.Mocks.MockHttpService.Http
{
    public interface IMockHttpRequestMessage
    {
        string Method { get; }
        string Path { get; }
        Uri Uri { get; }
        Stream Body { get; }
        IDictionary<string, IEnumerable<string>> Headers { get; }
    }
}
