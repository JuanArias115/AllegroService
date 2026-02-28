using AllegroService.Application.Common;
using AllegroService.Application.DTOs.Folios;

namespace AllegroService.Application.Interfaces;

public interface IFolioService
{
    Task<ServiceResult<FolioDetailDto>> GetByIdAsync(Guid folioId, CancellationToken cancellationToken);
    Task<ServiceResult<ChargeDto>> AddChargeAsync(Guid folioId, AddChargeRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<PaymentDto>> AddPaymentAsync(Guid folioId, AddPaymentRequest request, CancellationToken cancellationToken);
}
