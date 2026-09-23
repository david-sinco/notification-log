namespace Application.Shared.Pagination;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public PagedResult(IReadOnlyList<T> items, PageRequest paging, int totalCount)
        : this(items, paging.Page, paging.PageSize, totalCount) { }

    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
