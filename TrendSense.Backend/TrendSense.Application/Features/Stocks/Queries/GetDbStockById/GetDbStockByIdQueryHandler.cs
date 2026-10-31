using MediatR;
using System.Data.Entity;
using TrendSense.Application.Features.Stocks.Queries.GetDbStocks;
using TrendSense.Application.Interfaces;

namespace TrendSense.Application.Features.Stocks.Queries.GetDbStockById
{
    public class GetDbStockByIdQueryHandler : IRequestHandler<GetDbStockByIdQuery, StockDto?>
    {
        private readonly IAppDbContext _dbContext;

        public GetDbStockByIdQueryHandler(IAppDbContext dbContext) => _dbContext = dbContext;

        public async Task<StockDto?> Handle(GetDbStockByIdQuery request, CancellationToken cancellationToken)
        {
            return await _dbContext.Stocks.AsNoTracking()
                                          .Where(stock => stock.Id == request.Id)
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
                                          .SingleOrDefaultAsync(cancellationToken);
        }
    }
}
