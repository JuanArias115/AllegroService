namespace AllegroService.Application.DTOs.Common;

public class PagedResponse<T>
{
    public required IReadOnlyCollection<T> Items { get; init; }
    public required int Total { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
}
