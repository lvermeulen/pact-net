using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using PactNet.Logging;
using PactNet.Mocks.MockHttpService.Comparers;
using PactNet.Mocks.MockHttpService.Mappers;

namespace PactNet.Mocks.MockHttpService.Kestrel
{
    public class MockProviderKestrelBootstrapper //: DefaultNancyBootstrapper
    {
        private readonly PactConfig _config;

        public MockProviderKestrelBootstrapper(PactConfig config)
        {
            _config = config;
        }

        protected override IEnumerable<ModuleRegistration> Modules => new List<ModuleRegistration>();

        protected override Func<ITypeCatalog, NancyInternalConfiguration> InternalConfiguration => NancyInternalConfiguration.WithOverrides(c =>
                                                                                                                 {
                                                                                                                     c.RequestDispatcher = typeof(MockProviderKestrelRequestDispatcher);
                                                                                                                     c.StatusCodeHandlers.Clear();
                                                                                                                 });

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            RegisterDependenciesWithNancyContainer(container);

            DiagnosticsHook.Disable(pipelines);
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
        }

        private void RegisterDependenciesWithNancyContainer(TinyIoCContainer container)
        {
            container.Register(typeof(PactConfig), (c, o) => _config);
            container.Register<IProviderServiceRequestMapper, ProviderServiceRequestMapper>().AsMultiInstance();
            container.Register<IProviderServiceRequestComparer, ProviderServiceRequestComparer>().AsMultiInstance();
            container.Register<IKestrelResponseMapper, KestrelResponseMapper>().AsMultiInstance();
            container.Register<IMockProviderRequestHandler, MockProviderRequestHandler>().AsMultiInstance();
            container.Register<IMockProviderAdminRequestHandler, MockProviderAdminRequestHandler>().AsMultiInstance();
            container.Register<IMockProviderRepository, MockProviderRepository>().AsSingleton();
            container.Register<IFileSystem, FileSystem>().AsMultiInstance();
            container.Register(typeof(ILog), (c, o) => LogProvider.GetLogger(_config.LoggerName));
        }
    }
}