using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TrendSense.Application.Common.Mappings;
using TrendSense.Application.Features.WatchLists.Queries.GetWatchLists;
using TrendSense.Application.Interfaces;
using TrendSense.Tests.Common;

namespace TrendSense.Tests.Models.WatchList
{
    public class GetWatchListsQueryTests : TestCrudBase
    {
        private readonly IMapper _mapper;
        private readonly Mock<ICurrentUserService> _currentUser;
        private readonly Mock<ICacheService> _cache;
        private readonly GetWatchListsQueryHandler _handler;

        public GetWatchListsQueryTests()
        {
            var applicationAssembly = typeof(GetWatchListsQueryHandler).Assembly;

            var myProfileInstance = new AssemblyMappingProfile(applicationAssembly);

            var configurationProvider = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(myProfileInstance);
            }, NullLoggerFactory.Instance);

            _mapper = configurationProvider.CreateMapper();
            _currentUser = new Mock<ICurrentUserService>();
            _cache = new Mock<ICacheService>();

            _handler = new GetWatchListsQueryHandler(Context, _mapper, _currentUser.Object, _cache.Object);
        }

        [Fact]
        public async Task GetWatchLists_ReturnsCurrentUserLists()
        {
            // Arrange
            _currentUser.Setup(x => x.UserId)
                .Returns(TrendSenseContextFactory.UserAId);

            // Act
            var result = await _handler.Handle(new GetWatchListsQuery(), CancellationToken.None);

            // Assert
            Assert.Single(result.WatchLists);

            var watchList = result.WatchLists.First();

            Assert.Equal(TrendSenseContextFactory.WatchListAId, watchList.Id);
        }

        [Fact]
        public async Task GetWatchLists_ReturnsCachedLists_CacheHit()
        {
            // Arrange
            var userId = TrendSenseContextFactory.UserAId;

            var cachedLists = new List<WatchListLookupDto>
            {
                new()
                {
                    Id = TrendSenseContextFactory.WatchListAId,
                    Name = "Cached List"
                }
            };

            _currentUser.Setup(x => x.UserId)
                        .Returns(userId);

            _cache.Setup(x => x.GetAsync<List<WatchListLookupDto>>($"watchlists:{userId}", It.IsAny<CancellationToken>()))
                  .ReturnsAsync(cachedLists);

            // Act
            var result = await _handler.Handle(new GetWatchListsQuery(), CancellationToken.None);

            // Assert
            Assert.Single(result.WatchLists);

            var watchList = result.WatchLists.First();

            Assert.Equal(TrendSenseContextFactory.WatchListAId, watchList.Id);

            Assert.Equal("Cached List", watchList.Name);

            _cache.Verify(x => x.SetAsync(It.IsAny<string>(),
                                          It.IsAny<List<WatchListLookupDto>>(),
                                          It.IsAny<TimeSpan>(),
                                          It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetWatchLists_ReadsFromDBAndCaches_CacheMiss()
        {
            // Arrange
            var userId = TrendSenseContextFactory.UserAId;

            var mappedLists = new List<WatchListLookupDto>
            {
                new()
                {
                    Id = TrendSenseContextFactory.WatchListAId,
                    Name = "User A List"
                }
            };

            _currentUser.Setup(x => x.UserId)
                        .Returns(userId);

            _cache.Setup(x => x.GetAsync<List<WatchListLookupDto>>($"watchlists:{userId}", It.IsAny<CancellationToken>()))
                  .ReturnsAsync((List<WatchListLookupDto>?)null);

            // Act
            var result = await _handler.Handle(new GetWatchListsQuery(), CancellationToken.None);

            // Assert
            Assert.Single(result.WatchLists);

            Assert.Equal("User A List", result.WatchLists.First().Name);

            _cache.Verify(x => x.SetAsync($"watchlists:{userId}",
                                          It.Is<List<WatchListLookupDto>>(x => x.Count == 1 && x[0].Id == TrendSenseContextFactory.WatchListAId),
                                          It.Is<TimeSpan>(x => x == TimeSpan.FromMinutes(5)),
                                          It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
