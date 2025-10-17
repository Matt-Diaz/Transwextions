namespace Transwextions.Data.Models;

public class ServiceResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Object { get; set; }
    public string? ErrorMessage { get; set; }

    public static ServiceResult<T> Success(T result)
    {
        return new ServiceResult<T>
        {
            IsSuccess = true,
            Object = result,
            ErrorMessage = null
        };
    }

    public static ServiceResult<T> Failure(string errorMessage)
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            Object = default,
            ErrorMessage = errorMessage
        };
    }
}