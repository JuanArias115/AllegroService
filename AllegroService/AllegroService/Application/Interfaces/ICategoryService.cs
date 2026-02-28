using AllegroService.Application.Common;
using AllegroService.Application.DTOs.Categories;
using AllegroService.Application.DTOs.Common;

namespace AllegroService.Application.Interfaces;

public interface ICategoryService
{
    Task<ServiceResult<PagedResponse<CategoryDto>>> GetPagedAsync(ListQueryRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<CategoryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ServiceResult<CategoryDto>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
