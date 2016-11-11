﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using PactNet.Comparers;
using PactNet.Configuration.Json;
using PactNet.Logging;
using PactNet.Mocks.MockHttpService.Models;
using PactNet.Models;

namespace PactNet.Mocks.MockHttpService.Kestrel
{
    internal class MockProviderAdminRequestHandler : IMockProviderAdminRequestHandler
    {
        private readonly IMockProviderRepository _mockProviderRepository;
        private readonly IFileSystem _fileSystem;
        private readonly PactConfig _pactConfig;
        private readonly ILog _log;

        public MockProviderAdminRequestHandler(
            IMockProviderRepository mockProviderRepository,
            IFileSystem fileSystem,
            PactConfig pactConfig,
            ILog log)
        {
            _mockProviderRepository = mockProviderRepository;
            _fileSystem = fileSystem;
            _pactConfig = pactConfig;
            _log = log;
        }

        public HttpResponse Handle(HttpContext context)
        {
            //The first admin request with test context, we should log the context
            if (string.IsNullOrEmpty(_mockProviderRepository.TestContext) &&
                context.Request.Headers != null &&
                context.Request.Headers.Any(x => x.Key == Constants.ADMINISTRATIVE_REQUEST_TEST_CONTEXT_HEADER_KEY))
            {
                _mockProviderRepository.TestContext = context.Request.Headers.Single(x => x.Key == Constants.ADMINISTRATIVE_REQUEST_TEST_CONTEXT_HEADER_KEY).Value.Single();
                _log.InfoFormat("Test context {0}", _mockProviderRepository.TestContext);
            }

            if (context.Request.Method.Equals("DELETE", StringComparison.OrdinalIgnoreCase) && context.Request.Path == Constants.INTERACTIONS_PATH)
            {
                return HandleDeleteInteractionsRequest();
            }

            if (context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) && context.Request.Path == Constants.INTERACTIONS_PATH)
            {
                return HandlePostInteractionsRequest(context);
            }

            if (context.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase) && context.Request.Path == Constants.INTERACTIONS_VERIFICATION_PATH)
            {
                return HandleGetInteractionsVerificationRequest();
            }

            if (context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) && context.Request.Path == Constants.PACT_PATH)
            {
                return HandlePostPactRequest(context);
            }

            return GenerateResponse(HttpStatusCode.NotFound,
                $"The {context.Request.Method} request for path {context.Request.Path}, does not have a matching mock provider admin action.");
        }

        private HttpResponse HandleDeleteInteractionsRequest()
        {
            _mockProviderRepository.ClearTestScopedState();

            _log.Info("Cleared interactions");
            
            return GenerateResponse(HttpStatusCode.OK, "Deleted interactions");
        }

        private HttpResponse HandlePostInteractionsRequest(HttpContext context)
        {
            string interactionJson = ReadContent(context.Request.Body);
            var interaction = JsonConvert.DeserializeObject<ProviderServiceInteraction>(interactionJson);
            _mockProviderRepository.AddInteraction(interaction);

            _log.InfoFormat("Registered expected interaction {0} {1}", interaction.Request.Method.ToString().ToUpperInvariant(), interaction.Request.Path);
            _log.Debug(JsonConvert.SerializeObject(interaction, JsonConfig.PactFileSerializerSettings));

            return GenerateResponse(HttpStatusCode.OK, "Added interaction");
        }

        private HttpResponse HandleGetInteractionsVerificationRequest()
        {
            var registeredInteractions = _mockProviderRepository.TestScopedInteractions;

            var comparisonResult = new ComparisonResult();

            //Check all registered interactions have been used once and only once
            if (registeredInteractions.Any())
            {
                foreach (var registeredInteraction in registeredInteractions)
                {
                    var interactionUsages = _mockProviderRepository.HandledRequests.Where(x => x.MatchedInteraction != null && x.MatchedInteraction == registeredInteraction).ToList();

                    if (!interactionUsages.Any())
                    {
                        comparisonResult.RecordFailure(new MissingInteractionComparisonFailure(registeredInteraction));
                    }
                    else if (interactionUsages.Count > 1)
                    {
                        comparisonResult.RecordFailure(new ErrorMessageComparisonFailure(
                            $"The interaction with description '{registeredInteraction.Description}' and provider state '{registeredInteraction.ProviderState}', was used {interactionUsages.Count} time/s by the test."));
                    }
                }
            }

            //Have we seen any request that has not be registered by the test?
            if (_mockProviderRepository.HandledRequests != null && _mockProviderRepository.HandledRequests.Any(x => x.MatchedInteraction == null))
            {
                foreach (var handledRequest in _mockProviderRepository.HandledRequests.Where(x => x.MatchedInteraction == null))
                {
                    comparisonResult.RecordFailure(new UnexpectedRequestComparisonFailure(handledRequest.ActualRequest));
                }
            }

            //Have we seen any requests when no interactions were registered by the test?
            if (!registeredInteractions.Any() && 
                _mockProviderRepository.HandledRequests != null && 
                _mockProviderRepository.HandledRequests.Any())
            {
                comparisonResult.RecordFailure(new ErrorMessageComparisonFailure("No interactions were registered, however the mock provider service was called."));
            }

            if (!comparisonResult.HasFailure)
            {
                _log.Info("Verifying - interactions matched");

                return GenerateResponse(HttpStatusCode.OK, "Interactions matched");
            }

            _log.Error("Verifying - actual interactions do not match expected interactions");

            if (comparisonResult.Failures.Any(x => x is MissingInteractionComparisonFailure))
            {
                _log.Error("Missing requests: " + string.Join(", ", 
                    comparisonResult.Failures
                        .Where(x => x is MissingInteractionComparisonFailure)
                        .Cast<MissingInteractionComparisonFailure>()
                        .Select(x => x.RequestDescription)));
            }

            if (comparisonResult.Failures.Any(x => x is UnexpectedRequestComparisonFailure))
            {
                _log.Error("Unexpected requests: " + string.Join(", ", 
                    comparisonResult.Failures
                        .Where(x => x is UnexpectedRequestComparisonFailure)
                        .Cast<UnexpectedRequestComparisonFailure>()
                        .Select(x => x.RequestDescription)));
            }

            foreach (var failureResult in comparisonResult.Failures.Where(failureResult => !(failureResult is MissingInteractionComparisonFailure) && !(failureResult is UnexpectedRequestComparisonFailure)))
            {
                _log.Error(failureResult.Result);
            }

            var failure = comparisonResult.Failures.First();
            throw new PactFailureException(failure.Result);
        }

        private HttpResponse HandlePostPactRequest(HttpContext context)
        {
            string pactDetailsJson = ReadContent(context.Request.Body);
            var pactDetails = JsonConvert.DeserializeObject<PactDetails>(pactDetailsJson);
            string pactFilePath = Path.Combine(_pactConfig.PactDir, pactDetails.GeneratePactFileName());

            var pactFile = new ProviderServicePactFile
            {
                Provider = pactDetails.Provider,
                Consumer = pactDetails.Consumer,
                Interactions = _mockProviderRepository.Interactions
            };

            string pactFileJson = JsonConvert.SerializeObject(pactFile, JsonConfig.PactFileSerializerSettings);

            try
            {
                _fileSystem.File.WriteAllText(pactFilePath, pactFileJson);
            }
            catch (DirectoryNotFoundException)
            {
                _fileSystem.Directory.CreateDirectory(_pactConfig.PactDir);
                _fileSystem.File.WriteAllText(pactFilePath, pactFileJson);
            }

            return GenerateResponse(HttpStatusCode.OK, pactFileJson, "application/json");
        }

        private HttpResponse GenerateResponse(HttpStatusCode statusCode, string message, string contentType = "text/plain") => new HttpResponse
        {
            StatusCode = statusCode,
            Headers = new Dictionary<string, string> { { "Content-Type", contentType } },
            Contents = s => SetContent(message, s)
        };

        private void SetContent(string content, Stream stream)
        {
            var contentBytes = Encoding.UTF8.GetBytes(content);
            stream.Write(contentBytes, 0, contentBytes.Length);
            stream.Flush();
        }

        private string ReadContent(Stream stream)
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}