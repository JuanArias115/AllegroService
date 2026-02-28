using AllegroService.Api.Common;
using AllegroService.Application.DTOs.Common;
using AllegroService.Application.DTOs.Stays;
using AllegroService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AllegroService.Api.Controllers;

[Route("api/v1/stays")]
public class StaysController : BaseApiController
{
    private readonly IStayService _stayService;

    public StaysController(IStayService stayService)
    {
        _stayService = stayService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ListQueryRequest request, CancellationToken cancellationToken)
        => FromResult(await _stayService.GetPagedAsync(request, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => FromResult(await _stayService.GetByIdAsync(id, cancellationToken));

    [HttpPost("{stayId:guid}/check-out")]
    public async Task<IActionResult> CheckOut(Guid stayId, [FromBody] CheckOutRequest request, CancellationToken cancellationToken)
        => FromResult(await _stayService.CheckOutAsync(stayId, request, cancellationToken));
}
