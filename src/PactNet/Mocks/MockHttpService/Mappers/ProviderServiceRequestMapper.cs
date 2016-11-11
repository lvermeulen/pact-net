using System.IO;
using System.Linq;
using PactNet.Mocks.MockHttpService.Http;
using PactNet.Mocks.MockHttpService.Models;

namespace PactNet.Mocks.MockHttpService.Mappers
{
    public class ProviderServiceRequestMapper : IProviderServiceRequestMapper
    {
        private readonly IHttpVerbMapper _httpVerbMapper;
        private readonly IHttpBodyContentMapper _httpBodyContentMapper;

        public ProviderServiceRequestMapper(
            IHttpVerbMapper httpVerbMapper,
            IHttpBodyContentMapper httpBodyContentMapper)
        {
            _httpVerbMapper = httpVerbMapper;
            _httpBodyContentMapper = httpBodyContentMapper;
        }

        public ProviderServiceRequestMapper() : this(
            new HttpVerbMapper(),
            new HttpBodyContentMapper())
        {
        }

        public ProviderServiceRequest Convert(IMockHttpRequestMessage from)
        {
            if (from == null)
            {
                return null;
            }
                
            var httpVerb = _httpVerbMapper.Convert(from.Method.ToUpperInvariant());

            var to = new ProviderServiceRequest
            {
                Method = httpVerb,
                Path = from.Path,
                Query = !string.IsNullOrEmpty(from.Uri.Query) ? from.Uri.Query.TrimStart('?') : null
            };

            if (from.Headers != null && from.Headers.Any())
            {
                var fromHeaders = from.Headers.ToDictionary(x => x.Key, x => string.Join(", ", x.Value));
                to.Headers = fromHeaders;
            }

            if (from.Body != null && from.Body.Length > 0)
            {
                var httpBodyContent = _httpBodyContentMapper.Convert(content: ConvertStreamToBytes(from.Body), headers: to.Headers);

                to.Body = httpBodyContent.Body;
            }

            return to;
        }

        private static byte[] ConvertStreamToBytes(Stream content)
        {
            using (var memoryStream = new MemoryStream())
            {
                content.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}