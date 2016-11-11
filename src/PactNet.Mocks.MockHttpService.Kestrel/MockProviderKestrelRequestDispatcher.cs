using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using PactNet.Logging;

namespace PactNet.Mocks.MockHttpService.Kestrel
{
    internal class MockProviderKestrelRequestDispatcher : IRequestDispatcher
    {
        private readonly IMockProviderRequestHandler _requestHandler;
        private readonly IMockProviderAdminRequestHandler _adminRequestHandler;
        private readonly ILog _log;
        private readonly PactConfig _pactConfig;

        public MockProviderKestrelRequestDispatcher(
            IMockProviderRequestHandler requestHandler,
            IMockProviderAdminRequestHandler adminRequestHandler,
            ILog log,
            PactConfig pactConfig)
        {
            _requestHandler = requestHandler;
            _adminRequestHandler = adminRequestHandler;
            _log = log;
            _pactConfig = pactConfig;
        }

        public Task<HttpResponse> Dispatch(HttpContext context, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<HttpResponse>();

            if (cancellationToken.IsCancellationRequested)
            {
                tcs.SetException(new OperationCanceledException());
                return tcs.Task;
            }

            if (context == null)
            {
                tcs.SetException(new ArgumentException("context is null"));
                return tcs.Task;
            }

            HttpResponse response;

            try
            {
                response = IsAdminRequest(context.Request) ?
                    _adminRequestHandler.Handle(context) :
                    _requestHandler.Handle(context);
            }
            catch (Exception ex)
            {
                if (ex.GetType() != typeof(PactFailureException))
                {
                    _log.ErrorException("Failed to handle the request", ex);
                }

                string exceptionMessage = $"{JsonConvert.ToString(ex.Message) .Trim('"')} See {(!string.IsNullOrEmpty(_pactConfig.LoggerName) ? LogProvider.CurrentLogProvider.ResolveLogPath(_pactConfig.LoggerName) : "logs")} for details.";

                response = new HttpResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    ReasonPhrase = exceptionMessage,
                    Contents = s =>
                    {
                        var bytes = Encoding.UTF8.GetBytes(exceptionMessage);
                        s.Write(bytes, 0, bytes.Length);
                        s.Flush();
                    }
                };
            }

            context.Response = response;
            tcs.SetResult(context.Response);

            return tcs.Task;
        }

        private static bool IsAdminRequest(HttpRequest request) => request.Headers != null &&
       request.Headers.Any(x => x.Key == Constants.ADMINISTRATIVE_REQUEST_HEADER_KEY);
    }
}