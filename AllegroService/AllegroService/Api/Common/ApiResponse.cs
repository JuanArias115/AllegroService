using AllegroService.Application.Common;

namespace AllegroService.Api.Common;

public class ApiResponse<T>
{
    public T? Data { get; init; }
    public IReadOnlyCollection<ServiceError> Errors { get; init; } = Array.Empty<ServiceError>();

    public static ApiResponse<T> Ok(T data) => new() { Data = data };

    public static ApiResponse<T> Fail(IReadOnlyCollection<ServiceError> errors) => new() { Data = default, Errors = errors };
}
