using AllegroService.Api.Common;
using AllegroService.Application.DTOs.Common;
using AllegroService.Application.DTOs.UserTenants;
using AllegroService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AllegroService.Api.Controllers;

[Route("api/v1/user-tenants")]
public class UserTenantsController : BaseApiController
{
    private readonly IUserTenantService _userTenantService;

    public UserTenantsController(IUserTenantService userTenantService)
    {
        _userTenantService = userTenantService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken)
        => FromResult(await _userTenantService.GetCurrentAsync(cancellationToken));

    [HttpGet]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Get([FromQuery] ListQueryRequest request, CancellationToken cancellationToken)
        => FromResult(await _userTenantService.GetPagedAsync(request, cancellationToken));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => FromResult(await _userTenantService.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateUserTenantRequest request, CancellationToken cancellationToken)
        => FromResult(await _userTenantService.CreateAsync(request, cancellationToken));

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserTenantRequest request, CancellationToken cancellationToken)
        => FromResult(await _userTenantService.UpdateAsync(id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => FromResult(await _userTenantService.DeleteAsync(id, cancellationToken));
}
