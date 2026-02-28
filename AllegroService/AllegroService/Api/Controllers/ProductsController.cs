using AllegroService.Api.Common;
using AllegroService.Application.DTOs.Common;
using AllegroService.Application.DTOs.Products;
using AllegroService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AllegroService.Api.Controllers;

[Route("api/v1/products")]
public class ProductsController : BaseApiController
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ListQueryRequest request, CancellationToken cancellationToken)
        => FromResult(await _productService.GetPagedAsync(request, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => FromResult(await _productService.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
        => FromResult(await _productService.CreateAsync(request, cancellationToken));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
        => FromResult(await _productService.UpdateAsync(id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => FromResult(await _productService.DeleteAsync(id, cancellationToken));
}
