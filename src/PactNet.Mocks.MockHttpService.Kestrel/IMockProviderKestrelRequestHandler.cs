using Microsoft.AspNetCore.Http;

namespace PactNet.Mocks.MockHttpService.Kestrel
{
    internal interface IMockProviderKestrelRequestHandler
    {
        HttpResponse Handle(HttpContext context);
    }
}