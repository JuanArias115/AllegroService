using AllegroService.Application.Common;
using AllegroService.Application.DTOs.Common;
using AllegroService.Application.DTOs.Stays;

namespace AllegroService.Application.Interfaces;

public interface IStayService
{
    Task<ServiceResult<PagedResponse<StayDto>>> GetPagedAsync(ListQueryRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<StayDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ServiceResult<IReadOnlyCollection<ConsumptionDto>>> GetConsumptionsAsync(Guid stayId, CancellationToken cancellationToken);
    Task<ServiceResult<IReadOnlyCollection<ConsumptionDto>>> GetReservationConsumptionsAsync(Guid reservationId, CancellationToken cancellationToken);
    Task<ServiceResult<ConsumptionDto>> AddConsumptionAsync(Guid stayId, AddChargeRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<ConsumptionDto>> AddReservationConsumptionAsync(Guid reservationId, AddChargeRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<CheckoutSummaryDto>> GetCheckoutSummaryAsync(Guid stayId, string? language, CancellationToken cancellationToken);
    Task<ServiceResult<CheckInResponse>> CheckInAsync(Guid reservationId, CheckInRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<CheckOutResponse>> CheckOutAsync(Guid stayId, CheckOutRequest request, CancellationToken cancellationToken);
}
