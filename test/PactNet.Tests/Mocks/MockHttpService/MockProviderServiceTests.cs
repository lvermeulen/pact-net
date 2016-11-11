using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using NSubstitute;
using PactNet.Configuration.Json;
using PactNet.Mocks.MockHttpService;
using PactNet.Mocks.MockHttpService.Mappers;
using PactNet.Mocks.MockHttpService.Models;
using PactNet.Tests.Fakes;
using Xunit;

namespace PactNet.Tests.Mocks.MockHttpService
{
    public class MockProviderServiceTests
    {
        private IHttpHost _mockHttpHost;
        private FakeHttpMessageHandler _fakeHttpMessageHandler;
        private int _mockHttpHostFactoryCallCount;

        private IMockProviderService GetSubject(int port = 1234, bool enableSsl = false)
        {
            _mockHttpHost = Substitute.For<IHttpHost>();
            _fakeHttpMessageHandler = new FakeHttpMessageHandler();
            _mockHttpHostFactoryCallCount = 0;

            return new MockProviderService(
                baseUri =>
                {
                    _mockHttpHostFactoryCallCount++;
                    return _mockHttpHost;
                },
                port,
                enableSsl,
                baseUri => new HttpClient(_fakeHttpMessageHandler) { BaseAddress = new Uri(baseUri) },
                new HttpMethodMapper());
        }

        [Fact]
        public void Ctor_WhenCalledWithPort_SetsBaseUri()
        {
            const int port = 999;
            string expectedBaseUri = $"http://localhost:{port}";
            IMockProviderService mockService = GetSubject(port);

            Assert.Equal(expectedBaseUri, ((MockProviderService)mockService).BaseUri);
        }

        [Fact]
        public void Ctor_WhenCalledWithEnableSslFalse_SetsBaseUriWithHttpScheme()
        {
            IMockProviderService mockService = GetSubject(enableSsl: false);

            Assert.True(((MockProviderService)mockService).BaseUri.StartsWith("http", StringComparison.OrdinalIgnoreCase), "BaseUri has a http scheme");
        }

        [Fact]
        public void Ctor_WhenCalledWithEnableSslTrue_SetsBaseUriWithHttpsScheme()
        {
            IMockProviderService mockService = GetSubject(enableSsl: true);

            Assert.True(((MockProviderService)mockService).BaseUri.StartsWith("https"), "BaseUri has a https scheme");
        }

        [Fact]
        public void Given_WithProviderState_SetsProviderState()
        {
            const string providerState = "My provider state";
            IMockProviderService mockService = GetSubject();
            mockService.Start();

            mockService
                .Given(providerState)
                .UponReceiving("My description")
                .With(new ProviderServiceRequest { Method = HttpVerb.Get })
                .WillRespondWith(new ProviderServiceResponse { Status = (int)HttpStatusCode.OK });

            ProviderServiceInteraction interaction = Deserialise<ProviderServiceInteraction>(_fakeHttpMessageHandler.RequestContentReceived.Single());

            Assert.Equal(providerState, interaction.ProviderState);
        }

        [Fact]
        public void Given_WithNullProviderState_ThrowsArgumentException()
        {
            IMockProviderService mockService = GetSubject();

            Assert.Throws<ArgumentException>(() => mockService.Given(null));
        }

        [Fact]
        public void Given_WithEmptyProviderState_ThrowsArgumentException()
        {
            IMockProviderService mockService = GetSubject();

            Assert.Throws<ArgumentException>(() => mockService.Given(string.Empty));
        }

        [Fact]
        public void UponReceiving_WithDescription_SetsDescription()
        {
            const string description = "My description";
            IMockProviderService mockService = GetSubject();
            mockService.Start();

            mockService.UponReceiving(description)
                .With(new ProviderServiceRequest { Method = HttpVerb.Get })
                .WillRespondWith(new ProviderServiceResponse { Status = (int)HttpStatusCode.OK });

            ProviderServiceInteraction interaction = Deserialise<ProviderServiceInteraction>(_fakeHttpMessageHandler.RequestContentReceived.Single());

            Assert.Equal(description, interaction.Description);
        }

        [Fact]
        public void UponReceiving_WithNullDescription_ThrowsArgumentException()
        {
            IMockProviderService mockService = GetSubject();

            Assert.Throws<ArgumentException>(() => mockService.UponReceiving(null));
        }

        [Fact]
        public void UponReceiving_WithEmptyDescription_ThrowsArgumentException()
        {
            IMockProviderService mockService = GetSubject();

            Assert.Throws<ArgumentException>(() => mockService.UponReceiving(string.Empty));
        }

        [Fact]
        public void With_WithRequest_SetsRequest()
        {
            string description = "My description";
            ProviderServiceRequest request = new ProviderServiceRequest
            {
                Method = HttpVerb.Head,
                Path = "/tester/testing/1"
            };
            ProviderServiceResponse response = new ProviderServiceResponse
            {
                Status = (int)HttpStatusCode.ProxyAuthenticationRequired
            };

            ProviderServiceInteraction expectedInteraction = new ProviderServiceInteraction
            {
                Description = description,
                Request = request,
                Response = response
            };
            string expectedInteractionJson = expectedInteraction.AsJsonString();

            IMockProviderService mockService = GetSubject();
            mockService.Start();

            mockService.UponReceiving(description)
                .With(request)
                .WillRespondWith(response);

            string actualInteractionJson = _fakeHttpMessageHandler.RequestContentReceived.Single();

            Assert.Equal(expectedInteractionJson, actualInteractionJson);
        }

        [Fact]
        public void With_WithNullRequest_ThrowsArgumentException()
        {
            IMockProviderService mockService = GetSubject();

            Assert.Throws<ArgumentException>(() => mockService.With(null));
        }

        [Fact]
        public void With_WithRequestThatDoesNotHaveARequestMethod_ThrowsArgumentException()
        {
            string description = "My description";
            ProviderServiceRequest request = new ProviderServiceRequest
            {
                Path = "/tester/testing/1"
            };

            IMockProviderService mockService = GetSubject();
            mockService.Start();

            mockService.UponReceiving(description);

            Assert.Throws<ArgumentException>(() => mockService.With(request));
        }

        [Fact]
        public void With_WithRequestThatContainsABodyAndNoContentType_ThrowsArgumentException()
        {
            string description = "My description";
            ProviderServiceRequest request = new ProviderServiceRequest
            {
                Method = HttpVerb.Head,
                Path = "/tester/testing/1",
                Body = new
                {
                    tester = 1
                }
            };

            IMockProviderService mockService = GetSubject();
            mockService.Start();

            mockService.UponReceiving(description);

            Assert.Throws<ArgumentException>(() => mockService.With(request));
        }

        [Fact]
        public void WillRespondWith_WithNullResponse_ThrowsArgumentException()
        {
            IMockProviderService mockService = GetSubject();

            Assert.Throws<ArgumentException>(() => mockService.WillRespondWith(null));
        }

        [Fact]
        public void WillRespondWith_WithNullDescription_ThrowsInvalidOperationException()
        {
            IMockProviderService mockService = GetSubject();

            mockService
                .With(new ProviderServiceRequest { Method = HttpVerb.Get });

            Assert.Throws<InvalidOperationException>(() => mockService.WillRespondWith(new ProviderServiceResponse { Status = (int)HttpStatusCode.OK }));
        }

        [Fact]
        public void WillRespondWith_WithNullRequest_ThrowsInvalidOperationException()
        {
            IMockProviderService mockService = GetSubject();

            mockService
                .UponReceiving("My description");

            Assert.Throws<InvalidOperationException>(() => mockService.WillRespondWith(new ProviderServiceResponse { Status = (int)HttpStatusCode.OK }));
        }

        [Fact]
        public void WillRespondWith_WithResponseThatContainsABodyAndNoContentType_ThrowsArgumentException()
        {
            string providerState = "My provider state";
            string description = "My description";
            ProviderServiceRequest request = new ProviderServiceRequest { Method = HttpVerb.Get };
            ProviderServiceResponse response = new ProviderServiceResponse
            {
                Status = (int)HttpStatusCode.OK,
                Body = new
                {
                    tester = 1
                }
            };

            IMockProviderService mockService = GetSubject();

            mockService
                .Given(providerState)
                .UponReceiving(description)
                .With(request);

            Assert.Throws<ArgumentException>(() => mockService.WillRespondWith(response));
        }

        [Fact]
        public void WillRespondWith_WithResponseThatDoesNotHaveAResponseStatusSet_ThrowsArgumentException()
        {
            string providerState = "My provider state";
            string description = "My description";
            ProviderServiceRequest request = new ProviderServiceRequest { Method = HttpVerb.Get };
            ProviderServiceResponse response = new ProviderServiceResponse();

            IMockProviderService mockService = GetSubject();

            mockService
                .Given(providerState)
                .UponReceiving(description)
                .With(request);

            Assert.Throws<ArgumentException>(() => mockService.WillRespondWith(response));
        }

        [Fact]
        public void WillRespondWith_WhenHostIsNull_ThrowsInvalidOperationException()
        {
            string providerState = "My provider state";
            string description = "My description";
            ProviderServiceRequest request = new ProviderServiceRequest { Method = HttpVerb.Get };
            ProviderServiceResponse response = new ProviderServiceResponse { Status = (int)HttpStatusCode.OK };

            IMockProviderService mockService = GetSubject();

            mockService
                .Given(providerState)
                .UponReceiving(description)
                .With(request);

            mockService.Stop();

            Assert.Throws<InvalidOperationException>(() => mockService.WillRespondWith(response));
            Assert.Equal(0, _fakeHttpMessageHandler.RequestsReceived.Count());
        }

        [Fact]
        public void WillRespondWith_WithValidInteraction_PerformsAdminInteractionsPostRequestWithInteraction()
        {
            string providerState = "My provider state";
            string description = "My description";
            ProviderServiceRequest request = new ProviderServiceRequest
            {
                Method = HttpVerb.Head,
                Path = "/tester/testing/1"
            };
            ProviderServiceResponse response = new ProviderServiceResponse
            {
                Status = (int)HttpStatusCode.ProxyAuthenticationRequired
            };

            ProviderServiceInteraction expectedInteraction = new ProviderServiceInteraction
            {
                ProviderState = providerState,
                Description = description,
                Request = request,
                Response = response
            };
            string expectedInteractionJson = expectedInteraction.AsJsonString();

            IMockProviderService mockService = GetSubject();
            mockService.Start();

            mockService
                .Given(providerState)
                .UponReceiving(description)
                .With(request)
                .WillRespondWith(response);

            HttpRequestMessage actualRequest = _fakeHttpMessageHandler.RequestsReceived.Single();
            string actualInteractionJson = _fakeHttpMessageHandler.RequestContentReceived.Single();

            Assert.Equal(HttpMethod.Post, actualRequest.Method);
            Assert.Equal("http://localhost:1234/interactions", actualRequest.RequestUri.OriginalString);
            Assert.True(actualRequest.Headers.Contains(Constants.ADMINISTRATIVE_REQUEST_HEADER_KEY));

            Assert.Equal(expectedInteractionJson, actualInteractionJson);
        }

        [Fact]
        public void WillRespondWith_WithValidInteraction_PerformsAdminInteractionsPostRequestWithTestContext()
        {
            string providerState = "My provider state";
            string description = "My description";
            ProviderServiceRequest request = new ProviderServiceRequest
            {
                Method = HttpVerb.Head,
                Path = "/tester/testing/1"
            };
            ProviderServiceResponse response = new ProviderServiceResponse
            {
                Status = (int)HttpStatusCode.ProxyAuthenticationRequired
            };

            IMockProviderService mockService = GetSubject();
            mockService.Start();

            mockService
                .Given(providerState)
                .UponReceiving(description)
                .With(request)
                .WillRespondWith(response);

            HttpRequestMessage actualRequest = _fakeHttpMessageHandler.RequestsReceived.Single();

            Assert.Equal("MockProviderServiceTests.WillRespondWith_WithValidInteraction_PerformsAdminInteractionsPostRequestWithTestContext", actualRequest.Headers.Single(x => x.Key == Constants.ADMINISTRATIVE_REQUEST_TEST_CONTEXT_HEADER_KEY).Value.Single());
        }

        [Fact]
        public void WillRespondWith_WhenResponseFromHostIsNotOk_ThrowsPactFailureException()
        {
            string providerState = "My provider state";
            string description = "My description";
            ProviderServiceRequest request = new ProviderServiceRequest { Method = HttpVerb.Get };
            ProviderServiceResponse response = new ProviderServiceResponse { Status = (int)HttpStatusCode.OK };

            IMockProviderService mockService = GetSubject();

            _fakeHttpMessageHandler.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            mockService
                .Given(providerState)
                .UponReceiving(description)
                .With(request);

            mockService.Start();

            Assert.Throws<PactFailureException>(() => mockService.WillRespondWith(response));
        }

        [Fact]
        public void VerifyInteractions_WhenHostIsNull_ThrowsInvalidOperationException()
        {
            IMockProviderService mockService = GetSubject();

            mockService.Stop();

            Assert.Throws<InvalidOperationException>(() => mockService.VerifyInteractions());
            Assert.Equal(0, _fakeHttpMessageHandler.RequestsReceived.Count());
        }

        [Fact]
        public void VerifyInteractions_WhenHostIsNotNull_PerformsAdminInteractionsVerificationGetRequest()
        {
            IMockProviderService mockService = GetSubject();

            mockService.Start();

            mockService.VerifyInteractions();

            Assert.Equal(1, _fakeHttpMessageHandler.RequestsReceived.Count());
            Assert.Equal(HttpMethod.Get, _fakeHttpMessageHandler.RequestsReceived.First().Method);
            Assert.Equal("http://localhost:1234/interactions/verification", _fakeHttpMessageHandler.RequestsReceived.First().RequestUri.ToString());
        }

        [Fact]
        public void VerifyInteractions_WhenResponseFromHostIsNotOk_ThrowsPactFailureException()
        {
            IMockProviderService mockService = GetSubject();

            _fakeHttpMessageHandler.Response = new HttpResponseMessage(HttpStatusCode.Forbidden);

            mockService.Start();

            Assert.Throws<PactFailureException>(() => mockService.VerifyInteractions());
        }

        [Fact]
        public void ClearInteractions_WhenHostIsNull_DoesNotPerformAdminInteractionsDeleteRequest()
        {
            IMockProviderService mockService = GetSubject();
            mockService.Stop();

            mockService.ClearInteractions();

            Assert.Equal(0, _fakeHttpMessageHandler.RequestsReceived.Count());
        }

        [Fact]
        public void ClearInteractions_WhenHostIsNotNull_PerformsAdminInteractionsDeleteRequest()
        {
            IMockProviderService mockService = GetSubject();

            mockService.Start();

            mockService.ClearInteractions();

            Assert.Equal(1, _fakeHttpMessageHandler.RequestsReceived.Count());
            Assert.Equal(HttpMethod.Delete, _fakeHttpMessageHandler.RequestsReceived.First().Method);
            Assert.Equal("http://localhost:1234/interactions", _fakeHttpMessageHandler.RequestsReceived.First().RequestUri.ToString());
        }

        [Fact]
        public void ClearInteractions_WhenResponseFromHostIsNotOk_ThrowsPactFailureException()
        {
            IMockProviderService mockService = GetSubject();

            _fakeHttpMessageHandler.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            mockService.Start();

            Assert.Throws<PactFailureException>(() => mockService.ClearInteractions());
        }

        [Fact]
        public void Stop_WithNullHost_DoesNotThrow()
        {
            IMockProviderService mockService = GetSubject();

            mockService.Stop();
        }

        [Fact]
        public void Stop_WithNonNullHost_StopIsCalledOnHttpHost()
        {
            IMockProviderService mockService = GetSubject();

            mockService.Start();

            mockService.Stop();

            _mockHttpHost.Received(1).Stop();
        }

        [Fact]
        public void Start_WithNonNullHost_StopIsCalledOnHttpHost()
        {
            IMockProviderService mockService = GetSubject();

            mockService.Start();

            mockService.Start();

            _mockHttpHost.Received(1).Stop();
        }

        [Fact]
        public void Start_WithNullHost_DoesNotThrow()
        {
            IMockProviderService mockService = GetSubject();

            mockService.Start();
        }

        [Fact]
        public void Start_WhenCalled_CallsHostFactory()
        {
            IMockProviderService mockService = GetSubject();

            mockService.Start();

            Assert.Equal(1, _mockHttpHostFactoryCallCount);
        }

        [Fact]
        public void Start_WhenCalled_CallsStartOnHttpHost()
        {
            IMockProviderService mockService = GetSubject();

            mockService.Start();

            _mockHttpHost.Received(1).Start();
        }

        private T Deserialise<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, JsonConfig.ApiSerializerSettings);
        }
    }
}