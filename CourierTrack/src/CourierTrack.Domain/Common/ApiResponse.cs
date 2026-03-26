namespace CourierTrack.Domain.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public ErrorDetails? Error { get; set; }
    public PaginationMeta? Meta { get; set; }

    public static ApiResponse<T> SuccessResult(T data, PaginationMeta? meta = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Error = null,
            Meta = meta
        };
    }

    public static ApiResponse<T> FailResult(string message, string code = "INTERNAL_ERROR", List<string>? details = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = default,
            Error = new ErrorDetails
            {
                Code = code,
                Message = message,
                Details = details ?? new List<string>()
            },
            Meta = null
        };
    }

    public static ApiResponse<T> FailResult(Exception exception, string code = "INTERNAL_ERROR")
    {
        var details = new List<string>();

        if (!string.IsNullOrEmpty(exception.InnerException?.Message))
        {
            details.Add(exception.InnerException.Message);
        }

        return new ApiResponse<T>
        {
            Success = false,
            Data = default,
            Error = new ErrorDetails
            {
                Code = code,
                Message = exception.Message,
                Details = details
            },
            Meta = null
        };
    }
}

public class ErrorDetails
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<string> Details { get; set; } = new();
}

public class PaginationMeta
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
