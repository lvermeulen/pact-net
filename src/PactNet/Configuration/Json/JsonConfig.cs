using Newtonsoft.Json;

namespace PactNet.Configuration.Json
{
    public static class JsonConfig
    {
        private static JsonSerializerSettings s_serializerSettings;
        public static JsonSerializerSettings PactFileSerializerSettings 
        {
            get
            {
                s_serializerSettings = s_serializerSettings ?? new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                };
                return s_serializerSettings;
            }
        }

        private static JsonSerializerSettings s_apiRequestSerializerSettings;
        public static JsonSerializerSettings ApiSerializerSettings
        {
            get
            {
                s_apiRequestSerializerSettings = s_apiRequestSerializerSettings ?? new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.None
                };
                return s_apiRequestSerializerSettings;
            }
            set { s_apiRequestSerializerSettings = value; }
        }
    }
}
