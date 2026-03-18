using CryptoExchange.Net.Interfaces.Clients;
using System;

namespace HyperLiquid.Net.Interfaces.Clients.FuturesApi
{
    /// <summary>
    /// HyperLiquid futures API endpoints
    /// </summary>
    public interface IHyperLiquidRestClientFuturesApi : IRestApiClient, IDisposable
    {
        /// <summary>
        /// Endpoints related to account settings, info or actions
        /// </summary>
        /// <see cref="IHyperLiquidRestClientFuturesApiAccount"/>
        public IHyperLiquidRestClientFuturesApiAccount Account { get; }

        /// <summary>
        /// Endpoints related to retrieving market and system data
        /// </summary>
        /// <see cref="IHyperLiquidRestClientFuturesApiExchangeData"/>
        public IHyperLiquidRestClientFuturesApiExchangeData ExchangeData { get; }

        /// <summary>
        /// Endpoints related to orders and trades
        /// </summary>
        /// <see cref="IHyperLiquidRestClientFuturesApiTrading"/>
        public IHyperLiquidRestClientFuturesApiTrading Trading { get; }

    }
}
