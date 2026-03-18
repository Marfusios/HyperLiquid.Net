using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.Klines;
using CryptoExchange.Net.Trackers.Trades;
using HyperLiquid.Net.Interfaces;
using HyperLiquid.Net.Interfaces.Clients;
using Microsoft.Extensions.DependencyInjection;
using System;
using HyperLiquid.Net.Clients;
using Microsoft.Extensions.Logging;

namespace HyperLiquid.Net
{
    /// <inheritdoc />
    public class HyperLiquidTrackerFactory : IHyperLiquidTrackerFactory
    {
        private readonly IServiceProvider? _serviceProvider;

        /// <summary>
        /// ctor
        /// </summary>
        public HyperLiquidTrackerFactory()
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="serviceProvider">Service provider for resolving logging and clients</param>
        public HyperLiquidTrackerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public bool CanCreateKlineTracker(SharedSymbol symbol, SharedKlineInterval interval) => false;

        /// <inheritdoc />
        public bool CanCreateTradeTracker(SharedSymbol symbol) => true;

        /// <inheritdoc />
        public IKlineTracker CreateKlineTracker(SharedSymbol symbol, SharedKlineInterval interval, int? limit = null, TimeSpan? period = null)
        {
            throw new NotSupportedException("Kline trackers are not available for the local HyperLiquid client build.");
        }
        /// <inheritdoc />
        public ITradeTracker CreateTradeTracker(SharedSymbol symbol, int? limit = null, TimeSpan? period = null)
        {
            var restClient = _serviceProvider?.GetRequiredService<IHyperLiquidRestClient>() ?? new HyperLiquidRestClient();
            var socketClient = _serviceProvider?.GetRequiredService<IHyperLiquidSocketClient>() ?? new HyperLiquidSocketClient();

            ITradeSocketClient sharedSocketClient;
            if (symbol.TradingMode == TradingMode.Spot)            
                sharedSocketClient = socketClient.SpotApi.SharedClient;            
            else            
                sharedSocketClient = socketClient.FuturesApi.SharedClient;
            
            return new TradeTracker(
                _serviceProvider?.GetRequiredService<ILoggerFactory>().CreateLogger(restClient.Exchange),
                null,
                null,
                sharedSocketClient,
                symbol,
                limit,
                period
                );
        }
    }
}
