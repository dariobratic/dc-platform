namespace Audit.Application.Responses;

public sealed record PagedResponse<T>(
    List<T> Items,
    int TotalCount,
    int Skip,
    int Take)
{
    public bool HasMore => Skip + Take < TotalCount;
}
