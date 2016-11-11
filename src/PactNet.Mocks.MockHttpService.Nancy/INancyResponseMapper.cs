using Nancy;
using PactNet.Mappers;
using PactNet.Mocks.MockHttpService.Models;

namespace PactNet.Mocks.MockHttpService.Nancy
{
    public interface INancyResponseMapper : IMapper<ProviderServiceResponse, Response>
    {
    }
}