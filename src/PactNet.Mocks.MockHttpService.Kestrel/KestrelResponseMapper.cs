using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Http;
using PactNet.Mappers;
using PactNet.Mocks.MockHttpService.Mappers;
using PactNet.Mocks.MockHttpService.Models;

namespace PactNet.Mocks.MockHttpService.Kestrel
{
    internal class KestrelResponseMapper : IKestrelResponseMapper
    {
        private readonly IHttpBodyContentMapper _httpBodyContentMapper;

        internal KestrelResponseMapper(IHttpBodyContentMapper httpBodyContentMapper)
        {
            _httpBodyContentMapper = httpBodyContentMapper;
        }

        public KestrelResponseMapper() : this(new HttpBodyContentMapper())
        {
        }

        HttpResponse IMapper<ProviderServiceResponse, HttpResponse>.Convert(ProviderServiceResponse from)
        {
            if (from == null)
            {
                return null;
            }

            var to = new HttpResponse
            {
                StatusCode = (HttpStatusCode)from.Status,
                Headers = from.Headers ?? new Dictionary<string, string>()
            };

            if (from.Body != null)
            {
                HttpBodyContent bodyContent = _httpBodyContentMapper.Convert(body: from.Body, headers: from.Headers);
                to.ContentType = bodyContent.ContentType.MediaType;
                to.Contents = s =>
                {
                    byte[] bytes = bodyContent.ContentBytes;
                    s.Write(bytes, 0, bytes.Length);
                    s.Flush();
                };
            }

            return to;
        }
    }
}