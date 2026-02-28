using AllegroService.Application.Common;
using AllegroService.Application.DTOs.Common;
using AllegroService.Application.DTOs.Guests;

namespace AllegroService.Application.Interfaces;

public interface IGuestService
{
    Task<ServiceResult<PagedResponse<GuestDto>>> GetPagedAsync(ListQueryRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<GuestDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ServiceResult<GuestDto>> CreateAsync(CreateGuestRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<GuestDto>> UpdateAsync(Guid id, UpdateGuestRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
