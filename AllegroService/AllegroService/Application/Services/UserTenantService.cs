using AllegroService.Application.Common;
using AllegroService.Application.DTOs.Common;
using AllegroService.Application.DTOs.UserTenants;
using AllegroService.Application.Interfaces;
using AllegroService.Domain.Entities;
using AllegroService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AllegroService.Application.Services;

public class UserTenantService : IUserTenantService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserContext _currentUser;

    public UserTenantService(AppDbContext dbContext, ICurrentUserContext currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<ServiceResult<UserTenantDto>> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var firebaseUid = _currentUser.GetRequiredFirebaseUid();

        var entity = await _dbContext.UserTenants.AsNoTracking()
            .Where(x => x.FirebaseUid == firebaseUid)
            .Select(MapDtoExpression())
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null
            ? ServiceResult<UserTenantDto>.Failure(StatusCodes.Status404NotFound, new ServiceError("not_found", "UserTenant not found."))
            : ServiceResult<UserTenantDto>.Success(entity);
    }

    public async Task<ServiceResult<PagedResponse<UserTenantDto>>> GetPagedAsync(ListQueryRequest request, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();

        var query = _dbContext.UserTenants.AsNoTracking()
            .Where(x => x.GlampingId == glampingId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(x =>
                x.FirebaseUid.ToLower().Contains(term)
                || (x.Email != null && x.Email.ToLower().Contains(term)));
        }

        query = request.Sort?.ToLower() switch
        {
            "role" => query.OrderBy(x => x.Role),
            "role_desc" => query.OrderByDescending(x => x.Role),
            "status" => query.OrderBy(x => x.Status),
            "status_desc" => query.OrderByDescending(x => x.Status),
            _ => query.OrderBy(x => x.Email).ThenBy(x => x.FirebaseUid)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(MapDtoExpression())
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedResponse<UserTenantDto>>.Success(new PagedResponse<UserTenantDto>
        {
            Items = items,
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }

    public async Task<ServiceResult<UserTenantDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();

        var entity = await _dbContext.UserTenants.AsNoTracking()
            .Where(x => x.GlampingId == glampingId && x.Id == id)
            .Select(MapDtoExpression())
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null
            ? ServiceResult<UserTenantDto>.Failure(StatusCodes.Status404NotFound, new ServiceError("not_found", "UserTenant not found."))
            : ServiceResult<UserTenantDto>.Success(entity);
    }

    public async Task<ServiceResult<UserTenantDto>> CreateAsync(CreateUserTenantRequest request, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();
        var firebaseUid = request.FirebaseUid.Trim();

        var duplicate = await _dbContext.UserTenants.AnyAsync(x => x.FirebaseUid == firebaseUid, cancellationToken);
        if (duplicate)
        {
            return ServiceResult<UserTenantDto>.Failure(StatusCodes.Status409Conflict, new ServiceError("duplicate_uid", "Firebase UID is already registered."));
        }

        var entity = new UserTenant
        {
            Id = Guid.NewGuid(),
            FirebaseUid = firebaseUid,
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            GlampingId = glampingId,
            Role = request.Role,
            Status = request.Status
        };

        _dbContext.UserTenants.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<UserTenantDto>.Success(MapDto(entity), StatusCodes.Status201Created);
    }

    public async Task<ServiceResult<UserTenantDto>> UpdateAsync(Guid id, UpdateUserTenantRequest request, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();

        var entity = await _dbContext.UserTenants
            .FirstOrDefaultAsync(x => x.GlampingId == glampingId && x.Id == id, cancellationToken);

        if (entity is null)
        {
            return ServiceResult<UserTenantDto>.Failure(StatusCodes.Status404NotFound, new ServiceError("not_found", "UserTenant not found."));
        }

        entity.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        entity.Role = request.Role;
        entity.Status = request.Status;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<UserTenantDto>.Success(MapDto(entity));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();

        var entity = await _dbContext.UserTenants
            .FirstOrDefaultAsync(x => x.GlampingId == glampingId && x.Id == id, cancellationToken);

        if (entity is null)
        {
            return ServiceResult<bool>.Failure(StatusCodes.Status404NotFound, new ServiceError("not_found", "UserTenant not found."));
        }

        var currentUserId = _currentUser.GetCurrentUserId();
        if (currentUserId.HasValue && currentUserId.Value == entity.Id)
        {
            return ServiceResult<bool>.Failure(StatusCodes.Status409Conflict, new ServiceError("cannot_delete_self", "You cannot delete your own tenant binding."));
        }

        _dbContext.UserTenants.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<bool>.Success(true);
    }

    private static System.Linq.Expressions.Expression<Func<UserTenant, UserTenantDto>> MapDtoExpression()
        => x => new UserTenantDto(
            x.Id,
            x.FirebaseUid,
            x.Email,
            x.GlampingId,
            x.Role,
            x.Status);

    private static UserTenantDto MapDto(UserTenant entity)
        => new(
            entity.Id,
            entity.FirebaseUid,
            entity.Email,
            entity.GlampingId,
            entity.Role,
            entity.Status);
}
