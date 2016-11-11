﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PactNet.Configuration.Json.Converters;
using PactNet.Matchers;
using PactNet.Mocks.MockHttpService.Matchers;

namespace PactNet.Mocks.MockHttpService.Models
{
    public class ProviderServiceRequest : IHttpMessage
    {
        private bool _bodyWasSet;
        private dynamic _body;

        [JsonProperty(PropertyName = "method")]
        [JsonConverter(typeof(CamelCaseStringEnumConverter))]
        public HttpVerb Method { get; set; }

        [JsonProperty(PropertyName = "path")]
        public string Path { get; set; }

        [JsonProperty(PropertyName = "query")]
        public string Query { get; set; }

        [JsonProperty(PropertyName = "headers")]
        [JsonConverter(typeof(PreserveCasingDictionaryConverter))]
        public IDictionary<string, string> Headers { get; set; }

        [JsonIgnore]
        [JsonProperty(PropertyName = "matchingRules")]
        internal IDictionary<string, IMatcher> MatchingRules { get; private set; }

        [JsonProperty(PropertyName = "body", NullValueHandling = NullValueHandling.Include)]
        public dynamic Body
        {
            get { return _body; }
            set
            {
                _bodyWasSet = true;
                _body = ParseBodyMatchingRules(value);
            }
        }

        // A not so well known feature in JSON.Net to do conditional serialization at runtime
        public bool ShouldSerializeBody() => _bodyWasSet;

        private dynamic ParseBodyMatchingRules(dynamic body)
        {
            MatchingRules = new Dictionary<string, IMatcher>
            {
                { DefaultHttpBodyMatcher.PATH, new DefaultHttpBodyMatcher(false) }
            };

            return body;
        }

        public string PathWithQuery()
        {
            if (string.IsNullOrEmpty(Path) && !string.IsNullOrEmpty(Query))
            {
                throw new InvalidOperationException("Query has been supplied, however Path has not. Please specify as Path.");
            }

            return !string.IsNullOrEmpty(Query) ?
                $"{Path}?{Query}"
                :
                    Path;
        }
    }
}