namespace PactNet.Mocks.MockHttpService.Kestrel
{
    internal static class KestrelConfig
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
