using Nancy.Hosting.Self;

namespace PactNet.Mocks.MockHttpService.Nancy
{
    internal static class NancyConfig
    {
        private static HostConfiguration s_hostConfiguration;
        public static HostConfiguration HostConfiguration
        {
            get
            {
                s_hostConfiguration = s_hostConfiguration ?? new HostConfiguration
                {
                    UrlReservations =
                    {
                        CreateAutomatically = true
                    },
                    AllowChunkedEncoding = false
                };
                return s_hostConfiguration;
            }
        }
    }
}
