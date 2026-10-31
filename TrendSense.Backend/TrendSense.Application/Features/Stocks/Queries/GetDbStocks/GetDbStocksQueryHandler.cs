using MediatR;
using System.Data.Entity;
using TrendSense.Application.Common.Caching;
using TrendSense.Application.Dtos;
using TrendSense.Application.Interfaces;

namespace TrendSense.Application.Features.Stocks.Queries.GetDbStocks
{
    public class GetDbStocksQueryHandler : IRequestHandler<GetDbStocksQuery, PagedResult<StockDto>>
    {
        private readonly IAppDbContext _dbContext;
        private readonly ICacheService _cache;

        public GetDbStocksQueryHandler(IAppDbContext dbContext, ICacheService cache) => (_dbContext, _cache) = (dbContext, cache);

        public async Task<PagedResult<StockDto>> Handle(
        GetDbStocksQuery request,
        CancellationToken cancellationToken)
        {
            var version = await _cache.GetAsync<string>(CacheKeys.StocksVersion, cancellationToken);

            if (string.IsNullOrWhiteSpace(version))
            {
                version = Guid.NewGuid().ToString("N");

                await _cache.SetAsync(CacheKeys.StocksVersion, version, TimeSpan.FromDays(365), cancellationToken);
            }

            var cacheKey = CacheKeys.StockPage(version, request.Page, request.PageSize);

            var cached = await _cache.GetAsync<PagedResult<StockDto>>(cacheKey, cancellationToken);

            if (cached is not null)
            {
                return cached;
            }

            var stocksQuery = _dbContext.Stocks.AsNoTracking();

            var totalCount = await stocksQuery.CountAsync(
                cancellationToken);

            var items = await stocksQuery
                .OrderBy(stock => stock.TickerSymbol)
                .ThenBy(stock => stock.Id)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(stock => new StockDto
                {
                    Id = stock.Id,
                    TickerSymbol = stock.TickerSymbol,
                    Name = stock.Name,
                    Exchange = stock.Exchange,
                    LastPrice = stock.LastPrice,
                    DayChange = stock.DayChange,
                    DayChangePercent = stock.DayChangePercent,
                    UpdatedAt = stock.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<StockDto>(
                items,
                request.Page,
                request.PageSize,
                totalCount,
                (int)Math.Ceiling(
                    totalCount / (double)request.PageSize));

            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(1), cancellationToken);

            return result;
        }
    }
}
