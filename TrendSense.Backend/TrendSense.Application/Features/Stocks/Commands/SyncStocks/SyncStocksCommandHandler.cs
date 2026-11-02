using MediatR;
using Microsoft.Extensions.Logging;
using System.Data.Entity;
using TrendSense.Application.Common.Caching;
using TrendSense.Application.Features.Stocks.Commands.SyncStocks;
using TrendSense.Application.Interfaces;
using TrendSense.Domain;

namespace TrendSense.Application.Features.Stocks.Commands
{
    public class SyncStocksCommandHandler : IRequestHandler<SyncStocksCommand, Unit>
    {
        private readonly IAppDbContext _dbContext;
        private readonly ICacheService _cache;
        private readonly IStockMarketService _stockMarketService;
        private readonly ILogger<SyncStocksCommandHandler> _logger;

        public SyncStocksCommandHandler(IAppDbContext dbContext, IStockMarketService stockMarketService, ILogger<SyncStocksCommandHandler> logger, ICacheService cache) =>
            (_dbContext, _stockMarketService, _logger, _cache) = (dbContext, stockMarketService, logger, cache);

        public async Task<Unit> Handle(SyncStocksCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting stocks synchronization");

            var marketStocks = await _stockMarketService.GetStocksListAsync(cancellationToken);
            var existingStocks = await _dbContext.Stocks.ToListAsync(cancellationToken);
            var stocksByTicker = existingStocks.ToDictionary(x => x.TickerSymbol);

            foreach (var marketStock in marketStocks)
            {
                if (marketStock is null)
                {
                    continue;
                }

                if (!stocksByTicker.TryGetValue(marketStock.SecId, out var stock))
                {
                    stock = new Stock
                    {
                        Id = Guid.NewGuid(),
                        TickerSymbol = marketStock.SecId,
                        Name = marketStock.ShortName,
                        Isin = marketStock.Isin,
                        Currency = marketStock.CurrencyId,
                        Exchange = "MOEX",
                        LastPrice = marketStock.Last,
                        DayChange = marketStock.Change,
                        DayChangePercent = marketStock.ChangePercent,
                        UpdatedAt = marketStock.Time ?? DateTime.Now
                    };

                    _dbContext.Stocks.Add(stock);

                    stocksByTicker.Add(stock.TickerSymbol, stock);
                }
                else
                {
                    stock.Name = marketStock.ShortName;
                    stock.LastPrice = marketStock.Last;
                    stock.DayChange = marketStock.Change;
                    stock.DayChangePercent = marketStock.ChangePercent;
                    stock.UpdatedAt = marketStock.Time ?? DateTime.Now;
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _cache.SetAsync(CacheKeys.StocksVersion, Guid.NewGuid().ToString("N"), TimeSpan.FromDays(365), cancellationToken);

            _logger.LogInformation("Synchronization completed");

            return Unit.Value;
        }
    }
}
