namespace PactNet
{
    public class PactConfig
    {
        public string PactDir { get; set; }
        public string LogDir { get; set; }

        public string LoggerName;

        public PactConfig()
        {
            PactDir = Constants.DEFAULT_PACT_DIR;
            LogDir = Constants.DEFAULT_LOG_DIR;
        }
    }
}