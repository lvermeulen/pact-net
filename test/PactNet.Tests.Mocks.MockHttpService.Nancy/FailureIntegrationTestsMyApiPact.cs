using System;
using System.Net.Http;
using PactNet.Mocks.MockHttpService;
using PactNet.Mocks.MockHttpService.Mappers;
using PactNet.Mocks.MockHttpService.Nancy;

namespace PactNet.Tests.Mocks.MockHttpService.Nancy
{
    public class FailureIntegrationTestsMyApiPact : IDisposable
    {
        public IPactBuilder PactBuilder { get; private set; }
        public IMockProviderService MockProviderService { get; private set; }

        public int MockServerPort => 4321;
        public string MockProviderServiceBaseUri => $"http://localhost:{MockServerPort}";

        public FailureIntegrationTestsMyApiPact()
        {
            var pactConfig = new PactConfig { LoggerName = "my_api" };

            PactBuilder = new PactBuilder((port, enableSsl, providerName) => 
                    new MockProviderService(
                        baseUri => new NancyHttpHost(baseUri, pactConfig, new IntegrationTestingMockProviderNancyBootstrapper(pactConfig)), 
                        port, enableSsl, 
                        baseUri => new HttpClient { BaseAddress = new Uri(baseUri) },
                        new HttpMethodMapper()))
                .ServiceConsumer("FailureIntegrationTests")
                .HasPactWith("MyApi");

            MockProviderService = PactBuilder.MockService(MockServerPort);
        }

        public void Dispose()
        {
            PactBuilder.Build();
        }
    }
}