namespace Application.Shared.Pagination;

public sealed record PageRequest(int Page = PageRequest.DefaultPage, int PageSize = PageRequest.DefaultPageSize)
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public int Page { get; } = Math.Max(Page, 1);
    public int PageSize { get; } = Math.Clamp(PageSize, 1, MaxPageSize);
    public int Skip => (Page - 1) * PageSize;
}
