using FluentValidation;

namespace TrendSense.Application.Features.Stocks.Queries.GetDbStocks
{
    public class GetDbStocksQueryValidator : AbstractValidator<GetDbStocksQuery>
    {
        public GetDbStocksQueryValidator() 
        {
            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Number of the page must be greater than or equal to 1");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page size must be between 1 and 100");
        }
    }
}
