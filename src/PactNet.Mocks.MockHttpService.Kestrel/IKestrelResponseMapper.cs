using Microsoft.AspNetCore.Http;
using PactNet.Mappers;
using PactNet.Mocks.MockHttpService.Models;

namespace PactNet.Mocks.MockHttpService.Kestrel
{
    internal interface IKestrelResponseMapper : IMapper<ProviderServiceResponse, HttpResponse>
    {
    }
}