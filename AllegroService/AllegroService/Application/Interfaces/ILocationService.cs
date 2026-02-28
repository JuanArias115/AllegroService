using AllegroService.Application.Common;
using AllegroService.Application.DTOs.Common;
using AllegroService.Application.DTOs.Locations;

namespace AllegroService.Application.Interfaces;

public interface ILocationService
{
    Task<ServiceResult<PagedResponse<LocationDto>>> GetPagedAsync(ListQueryRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<LocationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ServiceResult<LocationDto>> CreateAsync(CreateLocationRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<LocationDto>> UpdateAsync(Guid id, UpdateLocationRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
