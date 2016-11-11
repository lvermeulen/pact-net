using System.Net.Http;
using PactNet.Mocks.MockHttpService.Models;

namespace PactNet.Mappers
{
    public interface IHttpMethodMapper
    {
        HttpMethod Convert(HttpVerb from);
    }
}