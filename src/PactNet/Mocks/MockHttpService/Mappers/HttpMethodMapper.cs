using System;
using System.Collections.Generic;
using System.Net.Http;
using PactNet.Mocks.MockHttpService.Models;

namespace PactNet.Mocks.MockHttpService.Mappers
{
    public class HttpMethodMapper : IHttpMethodMapper
    {
        private static readonly IDictionary<HttpVerb, HttpMethod> s_map = new Dictionary<HttpVerb, HttpMethod>
        {
            { HttpVerb.Get, HttpMethod.Get },
            { HttpVerb.Post, HttpMethod.Post },
            { HttpVerb.Put, HttpMethod.Put },
            { HttpVerb.Delete, HttpMethod.Delete },
            { HttpVerb.Head, HttpMethod.Head },
            { HttpVerb.Patch, new HttpMethod("PATCH") }
        };

        public HttpMethod Convert(HttpVerb from)
        {
            if (!s_map.ContainsKey(from))
            {
                throw new ArgumentException($"Cannot map HttpVerb.{from} to a HttpMethod, no matching item has been registered.");
            }

            return s_map[from];
        }
    }
}
