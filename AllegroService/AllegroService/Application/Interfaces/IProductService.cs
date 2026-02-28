using AllegroService.Application.Common;
using AllegroService.Application.DTOs.Common;
using AllegroService.Application.DTOs.Products;

namespace AllegroService.Application.Interfaces;

public interface IProductService
{
    Task<ServiceResult<PagedResponse<ProductDto>>> GetPagedAsync(ListQueryRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<ProductDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ServiceResult<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<ProductDto>> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
