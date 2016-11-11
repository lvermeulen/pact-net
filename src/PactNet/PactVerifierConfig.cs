using System.Collections.Generic;
using PactNet.Reporters.Outputters;

namespace PactNet
{
    public class PactVerifierConfig
    {
        public string LogDir { get; set; }

        public IList<IReportOutputter> ReportOutputters { get; private set; }

        internal string LoggerName;

        public PactVerifierConfig()
        {
            LogDir = Constants.DEFAULT_LOG_DIR;
            ReportOutputters = new List<IReportOutputter>
            {
                new ConsoleReportOutputter(),
                new FileReportOutputter(() => LoggerName)
            };
        }
    }
}