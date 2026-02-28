namespace AllegroService.Application.Common;

public class ServiceResult<T>
{
    private ServiceResult(bool isSuccess, T? data, int statusCode, IReadOnlyCollection<ServiceError> errors)
    {
        IsSuccess = isSuccess;
        Data = data;
        StatusCode = statusCode;
        Errors = errors;
    }

    public bool IsSuccess { get; }
    public T? Data { get; }
    public int StatusCode { get; }
    public IReadOnlyCollection<ServiceError> Errors { get; }

    public static ServiceResult<T> Success(T data, int statusCode = StatusCodes.Status200OK)
        => new(true, data, statusCode, Array.Empty<ServiceError>());

    public static ServiceResult<T> Failure(int statusCode, params ServiceError[] errors)
        => new(false, default, statusCode, errors);
}
