using System;
using System.Diagnostics;
using System.Threading;

namespace HyperLiquid.Net.Objects
{
    /// <summary>
    /// Opt-in timing for requests in the current async flow. Contains no request or credential data.
    /// Durations accumulate across SDK retries; preparation is measured only up to the first dispatch.
    /// </summary>
    public sealed class HyperLiquidRequestTiming : IDisposable
    {
        private static readonly AsyncLocal<HyperLiquidRequestTiming?> _current = new();
        private readonly HyperLiquidRequestTiming? _previous;
        internal static HyperLiquidRequestTiming? Current => _current.Value;
        internal long RateLimitTicks;
        internal long AuthenticationPreparationTicks;
        internal long SigningTicks;
        internal long HttpTicks;
        internal long ResponseProcessingTicks;
        private long _preparationTicks;

        /// <summary>Monotonic timestamp at scope entry.</summary>
        public long StartedTimestamp { get; }
        /// <summary>First IRequest.GetResponseAsync entry, before HttpClient.SendAsync; zero if never entered.</summary>
        public long HttpSendStartedTimestamp { get; private set; }
        /// <summary>Most recent response headers received; zero if no attempt returned headers.</summary>
        public long ResponseHeadersReceivedTimestamp { get; internal set; }
        /// <summary>Number of attempted HTTP requests, including retries and transport failures.</summary>
        public int HttpAttempts { get; private set; }
        /// <summary>Entry to first dispatch, excluding measured rate checks/waits and signing.</summary>
        public double PreparationMilliseconds => Milliseconds(_preparationTicks);
        /// <summary>Nonce generation, action hashing and EIP712 encoding, excluding signing.</summary>
        public double AuthenticationPreparationMilliseconds => Milliseconds(AuthenticationPreparationTicks);
        /// <summary>Existing recoverable signature function, including key creation and recovery.</summary>
        public double SigningMilliseconds => Milliseconds(SigningTicks);
        /// <summary>SDK rate-limit checks and waits.</summary>
        public double RateLimitMilliseconds => Milliseconds(RateLimitTicks);
        /// <summary>HTTP dispatch to response headers or transport failure; includes connection acquisition.</summary>
        public double HttpMilliseconds => Milliseconds(HttpTicks);
        /// <summary>Response headers to completion of SDK body reading and response validation.</summary>
        public double ResponseProcessingMilliseconds => Milliseconds(ResponseProcessingTicks);

        private HyperLiquidRequestTiming()
        {
            StartedTimestamp = Stopwatch.GetTimestamp();
            _previous = _current.Value;
            _current.Value = this;
        }

        /// <summary>Start a timing scope; dispose in the same async flow after the request completes.</summary>
        public static HyperLiquidRequestTiming Start() => new();

        internal void RecordHttpStart(long timestamp)
        {
            if (HttpAttempts++ == 0)
            {
                HttpSendStartedTimestamp = timestamp;
                _preparationTicks = Math.Max(0, timestamp - StartedTimestamp - RateLimitTicks - SigningTicks);
            }
        }

        private static double Milliseconds(long ticks) => ticks * 1000d / Stopwatch.Frequency;

        /// <inheritdoc />
        public void Dispose() => _current.Value = _previous;
    }
}
