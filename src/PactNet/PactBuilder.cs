﻿using System;
using Newtonsoft.Json;
using PactNet.Configuration.Json;
using PactNet.Mocks.MockHttpService;
using PactNet.Mocks.MockHttpService.Models;
using PactNet.Models;

namespace PactNet
{
    public class PactBuilder : IPactBuilder
    {
        public string ConsumerName { get; private set; }
        public string ProviderName { get; private set; }
        private readonly Func<int, bool, string, IMockProviderService> _mockProviderServiceFactory;
        private IMockProviderService _mockProviderService;

        public PactBuilder(Func<int, bool, string, IMockProviderService> mockProviderServiceFactory)
        {
            _mockProviderServiceFactory = mockProviderServiceFactory;
        }

        public PactBuilder()
            : this(new PactConfig())
        {
        }

        public PactBuilder(PactConfig config)
            : this((port, enableSsl, providerName) => new MockProviderService(port, enableSsl, providerName, config))
        {
        }

        public IPactBuilder ServiceConsumer(string consumerName)
        {
            if (string.IsNullOrEmpty(consumerName))
            {
                throw new ArgumentException("Please supply a non null or empty consumerName");
            }

            ConsumerName = consumerName;

            return this;
        }

        public IPactBuilder HasPactWith(string providerName)
        {
            if (string.IsNullOrEmpty(providerName))
            {
                throw new ArgumentException("Please supply a non null or empty providerName");
            }

            ProviderName = providerName;

            return this;
        }

        public IMockProviderService MockService(int port, bool enableSsl = false) => MockService(port, jsonSerializerSettings: null, enableSsl: enableSsl);

        public IMockProviderService MockService(int port, JsonSerializerSettings jsonSerializerSettings, bool enableSsl = false)
        {
            _mockProviderService?.Stop();

            if (jsonSerializerSettings != null)
            {
                JsonConfig.ApiSerializerSettings = jsonSerializerSettings;
            }

            _mockProviderService = _mockProviderServiceFactory(port, enableSsl, ProviderName);

            _mockProviderService.Start();

            return _mockProviderService;
        }

        public void Build()
        {
            if (_mockProviderService == null)
            {
                throw new InvalidOperationException("The Pact file could not be saved because the mock provider service is not initialised. Please initialise by calling the MockService() method.");
            }

            PersistPactFile();
            _mockProviderService.Stop();
        }

        private void PersistPactFile()
        {
            if (string.IsNullOrEmpty(ConsumerName))
            {
                throw new InvalidOperationException("ConsumerName has not been set, please supply a consumer name using the ServiceConsumer method.");
            }

            if (string.IsNullOrEmpty(ProviderName))
            {
                throw new InvalidOperationException("ProviderName has not been set, please supply a provider name using the HasPactWith method.");
            }

            var pactDetails = new PactDetails
            {
                Provider = new Pacticipant { Name = ProviderName },
                Consumer = new Pacticipant { Name = ConsumerName }
            };

            _mockProviderService.SendAdminHttpRequest(HttpVerb.Post, Constants.PACT_PATH, pactDetails);
        }
    }
}
