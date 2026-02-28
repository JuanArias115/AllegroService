using AllegroService.Application.Common;
using AllegroService.Application.DTOs.Common;
using AllegroService.Application.DTOs.Reservations;

namespace AllegroService.Application.Interfaces;

public interface IReservationService
{
    Task<ServiceResult<PagedResponse<ReservationDto>>> GetPagedAsync(ListQueryRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<ReservationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ServiceResult<ReservationDto>> CreateAsync(CreateReservationRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<ReservationDto>> UpdateAsync(Guid id, UpdateReservationRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
