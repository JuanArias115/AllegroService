using AllegroService.Application.Common;
using AllegroService.Application.DTOs.Common;
using AllegroService.Application.DTOs.Units;

namespace AllegroService.Application.Interfaces;

public interface IUnitService
{
    Task<ServiceResult<PagedResponse<UnitDto>>> GetPagedAsync(ListQueryRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<UnitDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ServiceResult<UnitDto>> CreateAsync(CreateUnitRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<UnitDto>> UpdateAsync(Guid id, UpdateUnitRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
