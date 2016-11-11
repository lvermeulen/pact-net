using System.IO.Abstractions;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using NSubstitute;
using PactNet.Mocks.MockHttpService.Nancy;

namespace PactNet.Tests.Mocks.MockHttpService.Nancy
{
    internal class IntegrationTestingMockProviderNancyBootstrapper : MockProviderNancyBootstrapper
    {
        public IntegrationTestingMockProviderNancyBootstrapper(PactConfig config)
            : base(config)
        {
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            container.Register(typeof(IFileSystem), Substitute.For<IFileSystem>());
        }
    }
}