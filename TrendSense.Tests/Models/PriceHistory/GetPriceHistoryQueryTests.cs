using Moq;
using TrendSense.Application.Features.Stocks.Queries.GetPriceHistory;
using TrendSense.Application.Interfaces;
using TrendSense.Tests.Common;

namespace TrendSense.Tests.Models.PriceHistory
{
    public class GetPriceHistoryQueryTests : TestCrudBase
    {
        private readonly Mock<ICacheService> _cache;
        private readonly GetPriceHistoryQueryHandler _handler;

        public GetPriceHistoryQueryTests()
        {
            _cache = new Mock<ICacheService>();
            _handler = new GetPriceHistoryQueryHandler(Context, _cache.Object);
        }

        [Fact]
        public async Task GetPriceHistory_ReturnsCachedHistory_CacheHit()
        {
            // Arrange
            var stockId = TrendSenseContextFactory.StockSberId;

            var cachedHistory = new List<PriceHistoryDto>
            {
                new()
                {
                    Price = 200,
                    RecoredAt = DateTime.Now
                }
            };

            _cache.Setup(x => x.GetAsync<List<PriceHistoryDto>>($"stocks:{stockId}:history", It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedHistory);

            // Act
            var result = await _handler.Handle(new GetPriceHistoryQuery
            {
                StockId = stockId
            }, CancellationToken.None);

            // Assert
            Assert.Single(result);
            Assert.Equal(200, result[0].Price);

            _cache.Verify(x => x.GetAsync<List<PriceHistoryDto>>($"stocks:{stockId}:history", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetPriceHistory_ReadsFromDBAndCaches_CacheMiss()
        {
            // Arrange
            var stockId = TrendSenseContextFactory.StockSberId;

            Context.History.Add(new Domain.PriceHistory
            {
                Id = Guid.NewGuid(),
                StockId = stockId,
                Price = 200,
                RecordedAt = DateTime.Now
            });

            await Context.SaveChangesAsync();

            _cache.Setup(x => x.GetAsync<List<PriceHistoryDto>>($"stocks:{stockId}:history", It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<PriceHistoryDto>?)null);

            // Act
            var result = await _handler.Handle(new GetPriceHistoryQuery
            {
                StockId = stockId
            }, CancellationToken.None);

            // Assert
            Assert.Single(result);
            Assert.Equal(200, result[0].Price);

            _cache.Verify(x => x.SetAsync($"stocks:{stockId}:history", It.Is<List<PriceHistoryDto>>(x => x.Count == 1 && x[0].Price == 200),
                        It.Is<TimeSpan>(x => x == TimeSpan.FromMinutes(1)), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
