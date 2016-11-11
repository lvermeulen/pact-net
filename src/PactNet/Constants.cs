namespace PactNet
{
    public static class Constants
    {
        public const string ADMINISTRATIVE_REQUEST_HEADER_KEY = "X-Pact-Mock-Service";
        public const string ADMINISTRATIVE_REQUEST_TEST_CONTEXT_HEADER_KEY = "X-Test-Context";
        public const string INTERACTIONS_PATH = "/interactions";
        public const string INTERACTIONS_VERIFICATION_PATH = "/interactions/verification";
        public const string PACT_PATH = "/pact";
        public const string DEFAULT_PACT_DIR = @"..\..\pacts\";
        public const string DEFAULT_LOG_DIR = @"..\..\logs\";
    }
}