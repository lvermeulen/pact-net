using PactNet.Mappers;
using PactNet.Mocks.MockHttpService.Http;
using PactNet.Mocks.MockHttpService.Models;

namespace PactNet.Mocks.MockHttpService.Mappers
{
    public interface IProviderServiceRequestMapper : IMapper<IMockHttpRequestMessage, ProviderServiceRequest>
    {
    }
}