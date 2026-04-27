namespace DanGian.Application.Common;

public record ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public ApiError? Error { get; init; }
    public ApiMeta Meta { get; init; } = new();

    public static ApiResponse<T> Ok(T data) =>
        new() { Success = true, Data = data };

    public static ApiResponse<T> Fail(string code, string message) =>
        new() { Success = false, Error = new ApiError(code, message) };
}

public record ApiMeta
{
    public string Timestamp { get; init; } = DateTime.UtcNow.ToString("O");
    public string Version { get; init; } = "1.0";
}

public record ApiError(string Code, string Message);

public record PagedList<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
