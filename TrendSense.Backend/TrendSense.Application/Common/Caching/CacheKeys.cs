namespace TrendSense.Application.Common.Caching
{
    public static class CacheKeys
    {
        public const string Stocks = "stocks:all";

        public const string StocksVersion = "stocks:version";

        public static string WatchLists(Guid userId) => $"watchlists:{userId}";

        public static string PriceHistory(Guid stockId) => $"stocks:{stockId}:history";

        public static string StockPage(string version, int page, int pageSize) => $"stocks:{version}:page:{page}:pageSize:{pageSize}";
    }
}
