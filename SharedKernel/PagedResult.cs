namespace SharedKernel;

public sealed class PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
    public int TotalPages => PageSize <= 0 ? 0 : (int) Math.Ceiling((double) TotalCount / PageSize);

    public static PagedResult<T> Create(IReadOnlyList<T> items, int page, int pageSize, int totalCount) =>
        new() {Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount};
}
