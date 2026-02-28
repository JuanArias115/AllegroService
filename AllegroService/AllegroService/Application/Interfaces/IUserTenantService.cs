using AllegroService.Application.Common;
using AllegroService.Application.DTOs.Common;
using AllegroService.Application.DTOs.UserTenants;

namespace AllegroService.Application.Interfaces;

public interface IUserTenantService
{
    Task<ServiceResult<PagedResponse<UserTenantDto>>> GetPagedAsync(ListQueryRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<UserTenantDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ServiceResult<UserTenantDto>> CreateAsync(CreateUserTenantRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<UserTenantDto>> UpdateAsync(Guid id, UpdateUserTenantRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
