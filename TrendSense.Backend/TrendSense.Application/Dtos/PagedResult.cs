namespace TrendSense.Application.Dtos
{
    public sealed record PagedResult<T>(List<T> items, int Page, int PageSize, int TotalCount, int TotalPages);
}
