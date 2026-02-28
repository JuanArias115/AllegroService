using AllegroService.Api.Common;
using AllegroService.Application.DTOs.Folios;
using AllegroService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AllegroService.Api.Controllers;

[Route("api/v1/folios")]
public class FoliosController : BaseApiController
{
    private readonly IFolioService _folioService;

    public FoliosController(IFolioService folioService)
    {
        _folioService = folioService;
    }

    [HttpGet("{folioId:guid}")]
    public async Task<IActionResult> GetById(Guid folioId, CancellationToken cancellationToken)
        => FromResult(await _folioService.GetByIdAsync(folioId, cancellationToken));

    [HttpPost("{folioId:guid}/charges")]
    public async Task<IActionResult> AddCharge(Guid folioId, [FromBody] AddChargeRequest request, CancellationToken cancellationToken)
        => FromResult(await _folioService.AddChargeAsync(folioId, request, cancellationToken));

    [HttpPost("{folioId:guid}/payments")]
    public async Task<IActionResult> AddPayment(Guid folioId, [FromBody] AddPaymentRequest request, CancellationToken cancellationToken)
        => FromResult(await _folioService.AddPaymentAsync(folioId, request, cancellationToken));
}
