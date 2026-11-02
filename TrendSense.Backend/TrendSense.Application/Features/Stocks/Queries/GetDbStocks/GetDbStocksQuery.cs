using MediatR;
using TrendSense.Application.Dtos;
using TrendSense.Domain;

namespace TrendSense.Application.Features.Stocks.Queries.GetDbStocks
{
    public class GetDbStocksQuery : IRequest<PagedResult<StockDto>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
