using System;
using PactNet.Extensions;
using PactNet.Logging;

namespace PactNet.Mocks.MockHttpService.Kestrel
{
    internal class KestrelHttpHost : IHttpHost
    {
        private readonly Uri _baseUri;
        private readonly INancyBootstrapper _bootstrapper;
        private readonly ILog _log;
        private readonly PactConfig _config;
        private KestrelHost _host;

        internal KestrelHttpHost(Uri baseUri, PactConfig config, INancyBootstrapper bootstrapper)
        {
            _baseUri = baseUri;
            _bootstrapper = bootstrapper;
            _log = LogProvider.GetLogger(config.LoggerName);
            _config = config;
        }

        internal KestrelHttpHost(Uri baseUri, string providerName, PactConfig config)
        {
            string loggerName = LogProvider.CurrentLogProvider.AddLogger(config.LogDir, providerName.ToLowerSnakeCase(), "{0}_mock_service.log");
            config.LoggerName = loggerName;

            _baseUri = baseUri;
            _bootstrapper = new MockProviderKestrelBootstrapper(config);
            _log = LogProvider.GetLogger(config.LoggerName);
            _config = config;
        }


        public void Start()
        {
            Stop();
            _host = new KestrelHost(_bootstrapper, KestrelConfig.HostConfiguration, _baseUri);
            _host.Start();
            _log.InfoFormat("Started {0}", _baseUri.OriginalString);
        }

        public void Stop()
        {
            if (_host != null)
            {
                _host.Stop();
                Dispose(_host);
                _host = null;
                _log.InfoFormat("Stopped {0}", _baseUri.OriginalString);

                LogProvider.CurrentLogProvider.RemoveLogger(_config.LoggerName);
            }
        }

        private void Dispose(IDisposable disposable)
        {
            disposable?.Dispose();
        }
    }
}