// EFormServices.Web/Models/ApiResponse.cs
// Got code 30/05/2025
namespace EFormServices.Web.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public string[]? Errors { get; set; }

    public static ApiResponse<T> SuccessResult(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResponse<T> ErrorResult(string[] errors, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Errors = errors,
            Message = message
        };
    }
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse SuccessResult(string? message = null)
    {
        return new ApiResponse
        {
            Success = true,
            Message = message
        };
    }

    public static new ApiResponse ErrorResult(string[] errors, string? message = null)
    {
        return new ApiResponse
        {
            Success = false,
            Errors = errors,
            Message = message
        };
    }
}
