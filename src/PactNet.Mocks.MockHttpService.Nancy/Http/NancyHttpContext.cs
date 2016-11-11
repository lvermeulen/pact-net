using Nancy;
using PactNet.Mocks.MockHttpService.Http;

namespace PactNet.Mocks.MockHttpService.Nancy.Http
{
    public class NancyHttpContext : IMockHttpContext
    {
        private readonly NancyContext _context;

        public static implicit operator NancyContext(NancyHttpContext nancyHttpContext) => new NancyContext
        {
            Request = nancyHttpContext.RequestMessage as Request
        };

        public static implicit operator NancyHttpContext(NancyContext nancyContext) => new NancyHttpContext(nancyContext);

        public NancyHttpContext(NancyContext nancyContext)
        {
            _context = nancyContext;
        }

        public IMockHttpRequestMessage RequestMessage
        {
            get { return (NancyHttpRequestMessage)_context.Request; }
            set { _context.Request = value as Request; }
        }
    }
}
