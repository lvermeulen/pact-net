using System;
using Nancy;
using Newtonsoft.Json;
using PactNet.Configuration.Json;
using PactNet.Logging;
using PactNet.Mocks.MockHttpService.Mappers;
using PactNet.Mocks.MockHttpService.Models;
using PactNet.Mocks.MockHttpService.Nancy.Http;

namespace PactNet.Mocks.MockHttpService.Nancy
{
    public class MockProviderRequestHandler : IMockProviderRequestHandler
    {
        private readonly INancyResponseMapper _responseMapper;
        private readonly IProviderServiceRequestMapper _requestMapper;
        private readonly IMockProviderRepository _mockProviderRepository;
        private readonly ILog _log;

        public MockProviderRequestHandler(
            IProviderServiceRequestMapper requestMapper,
            INancyResponseMapper responseMapper,
            IMockProviderRepository mockProviderRepository,
            ILog log)
        {
            _requestMapper = requestMapper;
            _responseMapper = responseMapper;
            _mockProviderRepository = mockProviderRepository;
            _log = log;
        }

        public Response Handle(NancyContext context) => HandlePactRequest(context);

        private Response HandlePactRequest(NancyContext context)
        {
            var actualRequest = _requestMapper.Convert((NancyHttpRequestMessage)context.Request);
            string actualRequestMethod = actualRequest.Method.ToString().ToUpperInvariant();
            string actualRequestPath = actualRequest.Path;

            _log.InfoFormat("Received request {0} {1}", actualRequestMethod, actualRequestPath);
            _log.Debug(JsonConvert.SerializeObject(actualRequest, JsonConfig.PactFileSerializerSettings));

            ProviderServiceInteraction matchingInteraction;

            try
            {
                matchingInteraction = _mockProviderRepository.GetMatchingTestScopedInteraction(actualRequest);
                _mockProviderRepository.AddHandledRequest(new HandledRequest(actualRequest, matchingInteraction));

                _log.InfoFormat("Found matching response for {0} {1}", actualRequestMethod, actualRequestPath);
                _log.Debug(JsonConvert.SerializeObject(matchingInteraction.Response, JsonConfig.PactFileSerializerSettings));
            }
            catch (Exception)
            {
                _log.ErrorFormat("No matching interaction found for {0} {1}", actualRequestMethod, actualRequestPath);
                _mockProviderRepository.AddHandledRequest(new HandledRequest(actualRequest, null));
                throw;
            }

            return _responseMapper.Convert(matchingInteraction.Response);
        }
    }
}