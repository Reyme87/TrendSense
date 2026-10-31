using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrendSense.Application.Dtos;
using TrendSense.Application.Features.Stocks.Commands.SyncStocks;
using TrendSense.Application.Features.Stocks.Commands.UpdateStockPrices;
using TrendSense.Application.Features.Stocks.Queries.GetDbStockById;
using TrendSense.Application.Features.Stocks.Queries.GetDbStocks;
using TrendSense.Application.Features.Stocks.Queries.GetPriceHistory;
using TrendSense.Application.Features.Stocks.Queries.GetStock;

namespace TrendSense.WebApi.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class StocksController : BaseController
    {
        public StocksController(IMediator mediator) : base(mediator) { }

        /// <summary>
        /// Gets stocks list
        /// </summary>
        /// <param name="command"></param>
        /// <returns>
        /// Returns StockDto
        /// </returns>
        /// <response code="200">Success</response>
        /// <response code="401">If the user is unauthorized</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PagedResult<StockDto>>> Get([FromQuery] GetDbStocksQuery query, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Gets stock by id
        /// </summary>
        /// <param name="command"></param>
        /// <returns>
        /// Returns StockDto
        /// </returns>
        /// <response code="200">Success</response>
        /// <response code="401">If the user is unauthorized</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<StockDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var stock = await Mediator.Send(new GetDbStockByIdQuery { Id = id }, cancellationToken);

            if (stock is null)
            {
                return NotFound();
            }

            return Ok(stock);
        }

        /// <summary>
        /// Gets stocks list from exchange and updates data in DB
        /// </summary>
        /// <returns>
        /// Returns NoContent
        /// </returns>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <response code="204">Data updated</response>
        [HttpPost("sync")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Sync(SyncStocksCommand command, CancellationToken cancellationToken)
        {
            await Mediator.Send(command, cancellationToken);

            return NoContent();
        }

        [HttpGet("{id:guid}/history")]
        public async Task<ActionResult<IReadOnlyList<PriceHistoryDto>>> GetHistory(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetPriceHistoryQuery
            {
                StockId = id
            };

            var result = await Mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdatePrices(UpdateStockPricesCommand command, CancellationToken cancellationToken)
        {
            await Mediator.Send(command, cancellationToken);

            return NoContent();
        }
    }
}
