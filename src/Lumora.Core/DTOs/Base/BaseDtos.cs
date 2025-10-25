namespace Lumora.DTOs.Base;

/// <summary>
/// General result class for API responses for Training Section.
/// </summary>
public class GeneralResult
{
    public bool? IsSuccess { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }
    public ErrorType? ErrorType { get; set; }

    public GeneralResult(bool success, string message, object? data = null, ErrorType? errorType = null)
    {
        IsSuccess = success;
        Message = message;
        Data = data;
        ErrorType = errorType;
    }

    public GeneralResult()
    {
        // empty constructor
    }
}

/// <summary>
/// General result class for API responses.
/// </summary>
public class GeneralResult<T>
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public ErrorType? ErrorType { get; set; }

    public GeneralResult(bool isSuccess, string? message = null, T? data = default, ErrorType? errorType = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        Data = data;
        ErrorType = errorType;
    }
}

/// <summary>
/// Pagination request class.
/// </summary>
public class PaginationRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public int Skip => (PageNumber - 1) * PageSize;
}

/// <summary>
/// Pagination result class.
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
