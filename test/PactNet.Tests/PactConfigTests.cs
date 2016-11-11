using PactNet.Mocks.MockHttpService;
using Xunit;

namespace PactNet.Tests
{
    public class PactConfigTests
    {
        [Fact]
        public void Ctor_WithDefaults_UsesDefaultPactDir()
        {
            var options = new PactConfig();

            Assert.Equal(Constants.DEFAULT_PACT_DIR, options.PactDir);
        }

        [Fact]
        public void Ctor_WithDefaults_UsesDefaultLogDir()
        {
            var options = new PactConfig();

            Assert.Equal(Constants.DEFAULT_LOG_DIR, options.LogDir);
        }
    }
}
