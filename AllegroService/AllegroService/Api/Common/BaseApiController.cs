using AllegroService.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AllegroService.Api.Common;

[ApiController]
[Authorize]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult FromResult<T>(ServiceResult<T> result)
    {
        if (result.IsSuccess && result.Data is not null)
        {
            return StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data));
        }

        return StatusCode(result.StatusCode, ApiResponse<T>.Fail(result.Errors));
    }
}
