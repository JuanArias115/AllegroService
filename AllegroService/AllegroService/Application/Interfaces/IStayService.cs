using AllegroService.Application.Common;
using AllegroService.Application.DTOs.Common;
using AllegroService.Application.DTOs.Stays;

namespace AllegroService.Application.Interfaces;

public interface IStayService
{
    Task<ServiceResult<PagedResponse<StayDto>>> GetPagedAsync(ListQueryRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<StayDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ServiceResult<CheckInResponse>> CheckInAsync(Guid reservationId, CheckInRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<CheckOutResponse>> CheckOutAsync(Guid stayId, CheckOutRequest request, CancellationToken cancellationToken);
}
