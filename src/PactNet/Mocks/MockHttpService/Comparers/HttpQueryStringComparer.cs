using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.WebUtilities;
using PactNet.Comparers;

namespace PactNet.Mocks.MockHttpService.Comparers
{
    internal class HttpQueryStringComparer : IHttpQueryStringComparer
    {
        public ComparisonResult Compare(string expected, string actual)
        {
            if (string.IsNullOrEmpty(expected) && string.IsNullOrEmpty(actual))
            {
                return new ComparisonResult("has no query strings");
            }

            string normalisedExpectedQuery = NormaliseUrlEncodingAndTrimTrailingAmpersand(expected);
            string normalisedActualQuery = NormaliseUrlEncodingAndTrimTrailingAmpersand(actual);
            var result = new ComparisonResult("has query {0}", normalisedExpectedQuery ?? "null");

            if (expected == null || actual == null)
            {
                result.RecordFailure(new DiffComparisonFailure(expected, actual));
                return result;
            }

            var expectedQueryItems = QueryHelpers.ParseQuery(normalisedExpectedQuery);
            var actualQueryItems = QueryHelpers.ParseQuery(normalisedActualQuery);

            if (expectedQueryItems.Count != actualQueryItems.Count)
            {
                result.RecordFailure(new DiffComparisonFailure(normalisedExpectedQuery, normalisedActualQuery));
                return result;
            }

            foreach (string expectedKey in expectedQueryItems.Keys)
            {
                if (!actualQueryItems.Keys.Contains(expectedKey))
                {
                    result.RecordFailure(new DiffComparisonFailure(normalisedExpectedQuery, normalisedActualQuery));
                    return result;
                }

                string expectedValue = expectedQueryItems[expectedKey];
                string actualValue = actualQueryItems[expectedKey];

                if (expectedValue != actualValue)
                {
                    result.RecordFailure(new DiffComparisonFailure(normalisedExpectedQuery, normalisedActualQuery));
                    return result;
                }
            }

            return result;
        }

        private string NormaliseUrlEncodingAndTrimTrailingAmpersand(string query) => query != null
            ? Regex.Replace(query, "(%[0-9a-f][0-9a-f])", c => c.Value.ToUpperInvariant()).TrimEnd('&')
            : null;
    }
}