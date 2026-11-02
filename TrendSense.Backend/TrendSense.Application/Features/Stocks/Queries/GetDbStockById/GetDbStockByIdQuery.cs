using MediatR;
using TrendSense.Application.Features.Stocks.Queries.GetDbStocks;

namespace TrendSense.Application.Features.Stocks.Queries.GetDbStockById
{
    public class GetDbStockByIdQuery : IRequest<StockDto?>
    {
        public Guid Id;
    }
}
