using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.RateLimiting.Interfaces;
using HyperLiquid.Net.Objects;

namespace HyperLiquid.Net.Clients.BaseApi
{
    internal abstract partial class HyperLiquidRestClientApi
    {
        protected override ValueTask<Error?> RateLimitAsync(string host, int requestId, RequestDefinition definition,
            int weight, CancellationToken cancellationToken, int? weightSingleLimiter = null, string? rateLimitKeySuffix = null)
        {
            var timing = HyperLiquidRequestTiming.Current;
            return timing == null
                ? base.RateLimitAsync(host, requestId, definition, weight, cancellationToken, weightSingleLimiter, rateLimitKeySuffix)
                : MeasureRateLimitAsync(timing, host, requestId, definition, weight, cancellationToken, weightSingleLimiter, rateLimitKeySuffix);
        }

        private async ValueTask<Error?> MeasureRateLimitAsync(HyperLiquidRequestTiming timing, string host,
            int requestId, RequestDefinition definition, int weight, CancellationToken ct, int? weightSingleLimiter, string? rateLimitKeySuffix)
        {
            var started = Stopwatch.GetTimestamp();
            try
            {
                return await base.RateLimitAsync(host, requestId, definition, weight, ct, weightSingleLimiter, rateLimitKeySuffix).ConfigureAwait(false);
            }
            finally
            {
                timing.RateLimitTicks += Stopwatch.GetTimestamp() - started;
            }
        }

        protected override Task<WebCallResult<T>> GetResponseAsync2<T>(RequestDefinition definition, IRequest request,
            IRateLimitGate? gate, CancellationToken cancellationToken)
        {
            var timing = HyperLiquidRequestTiming.Current;
            return timing == null
                ? base.GetResponseAsync2<T>(definition, request, gate, cancellationToken)
                : MeasureResponseAsync<T>(definition, new TimedRequest(request, timing), gate, cancellationToken);
        }

        private async Task<WebCallResult<T>> MeasureResponseAsync<T>(RequestDefinition definition, TimedRequest request,
            IRateLimitGate? gate, CancellationToken ct)
        {
            try
            {
                return await base.GetResponseAsync2<T>(definition, request, gate, ct).ConfigureAwait(false);
            }
            finally
            {
                if (request.HeadersTimestamp > 0)
                    request.Timing.ResponseProcessingTicks += Stopwatch.GetTimestamp() - request.HeadersTimestamp;
            }
        }

        private sealed class TimedRequest(IRequest inner, HyperLiquidRequestTiming timing) : IRequest
        {
            internal HyperLiquidRequestTiming Timing => timing;
            internal long HeadersTimestamp { get; private set; }
            public MediaTypeWithQualityHeaderValue Accept { set => inner.Accept = value; }
            public string? Content => inner.Content;
            public HttpMethod Method { get => inner.Method; set => inner.Method = value; }
            public Uri Uri => inner.Uri;
            public Version HttpVersion => inner.HttpVersion;
            public int RequestId => inner.RequestId;
            public void SetContent(byte[] data) => inner.SetContent(data);
            public void SetContent(string data, Encoding? encoding, string contentType) => inner.SetContent(data, encoding, contentType);
            public void AddHeader(string key, string value) => inner.AddHeader(key, value);
            public HttpRequestHeaders GetHeaders() => inner.GetHeaders();

            public async Task<IResponse> GetResponseAsync(CancellationToken ct)
            {
                var started = Stopwatch.GetTimestamp();
                timing.RecordHttpStart(started);
                try
                {
                    var response = await inner.GetResponseAsync(ct).ConfigureAwait(false);
                    HeadersTimestamp = timing.ResponseHeadersReceivedTimestamp = Stopwatch.GetTimestamp();
                    return response;
                }
                finally
                {
                    timing.HttpTicks += (HeadersTimestamp > 0 ? HeadersTimestamp : Stopwatch.GetTimestamp()) - started;
                }
            }
        }
    }
}
