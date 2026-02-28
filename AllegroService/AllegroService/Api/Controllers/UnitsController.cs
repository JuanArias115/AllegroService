using AllegroService.Api.Common;
using AllegroService.Application.DTOs.Common;
using AllegroService.Application.DTOs.Units;
using AllegroService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AllegroService.Api.Controllers;

[Route("api/v1/units")]
public class UnitsController : BaseApiController
{
    private readonly IUnitService _unitService;

    public UnitsController(IUnitService unitService)
    {
        _unitService = unitService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ListQueryRequest request, CancellationToken cancellationToken)
        => FromResult(await _unitService.GetPagedAsync(request, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => FromResult(await _unitService.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUnitRequest request, CancellationToken cancellationToken)
        => FromResult(await _unitService.CreateAsync(request, cancellationToken));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUnitRequest request, CancellationToken cancellationToken)
        => FromResult(await _unitService.UpdateAsync(id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => FromResult(await _unitService.DeleteAsync(id, cancellationToken));
}
