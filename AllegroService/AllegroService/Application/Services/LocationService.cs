using AllegroService.Application.Common;
using AllegroService.Application.DTOs.Common;
using AllegroService.Application.DTOs.Locations;
using AllegroService.Application.Interfaces;
using AllegroService.Domain.Entities;
using AllegroService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AllegroService.Application.Services;

public class LocationService : ILocationService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserContext _currentUser;

    public LocationService(AppDbContext dbContext, ICurrentUserContext currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<ServiceResult<PagedResponse<LocationDto>>> GetPagedAsync(ListQueryRequest request, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();
        var query = _dbContext.Locations.AsNoTracking().Where(x => x.GlampingId == glampingId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(term));
        }

        query = request.Sort?.ToLower() switch
        {
            "name_desc" => query.OrderByDescending(x => x.Name),
            _ => query.OrderBy(x => x.Name)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new LocationDto(x.Id, x.Name, x.Type, x.UnitId))
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedResponse<LocationDto>>.Success(new PagedResponse<LocationDto>
        {
            Items = items,
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }

    public async Task<ServiceResult<LocationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();
        var location = await _dbContext.Locations.AsNoTracking()
            .Where(x => x.GlampingId == glampingId && x.Id == id)
            .Select(x => new LocationDto(x.Id, x.Name, x.Type, x.UnitId))
            .FirstOrDefaultAsync(cancellationToken);

        return location is null
            ? ServiceResult<LocationDto>.Failure(StatusCodes.Status404NotFound, new ServiceError("not_found", "Location not found."))
            : ServiceResult<LocationDto>.Success(location);
    }

    public async Task<ServiceResult<LocationDto>> CreateAsync(CreateLocationRequest request, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();
        var name = request.Name.Trim();

        var duplicate = await _dbContext.Locations.AnyAsync(x => x.GlampingId == glampingId && x.Name == name, cancellationToken);
        if (duplicate)
        {
            return ServiceResult<LocationDto>.Failure(StatusCodes.Status409Conflict, new ServiceError("duplicate_name", "A location with this name already exists."));
        }

        if (request.UnitId.HasValue)
        {
            var unitExists = await _dbContext.Units.AnyAsync(x => x.GlampingId == glampingId && x.Id == request.UnitId.Value, cancellationToken);
            if (!unitExists)
            {
                return ServiceResult<LocationDto>.Failure(StatusCodes.Status400BadRequest, new ServiceError("invalid_unit", "Unit does not exist for this tenant."));
            }
        }

        var location = new Location
        {
            Id = Guid.NewGuid(),
            GlampingId = glampingId,
            Name = name,
            Type = request.Type,
            UnitId = request.UnitId
        };

        _dbContext.Locations.Add(location);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<LocationDto>.Success(new LocationDto(location.Id, location.Name, location.Type, location.UnitId), StatusCodes.Status201Created);
    }

    public async Task<ServiceResult<LocationDto>> UpdateAsync(Guid id, UpdateLocationRequest request, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();
        var location = await _dbContext.Locations.FirstOrDefaultAsync(x => x.GlampingId == glampingId && x.Id == id, cancellationToken);

        if (location is null)
        {
            return ServiceResult<LocationDto>.Failure(StatusCodes.Status404NotFound, new ServiceError("not_found", "Location not found."));
        }

        var name = request.Name.Trim();
        var duplicate = await _dbContext.Locations.AnyAsync(x => x.GlampingId == glampingId && x.Name == name && x.Id != id, cancellationToken);
        if (duplicate)
        {
            return ServiceResult<LocationDto>.Failure(StatusCodes.Status409Conflict, new ServiceError("duplicate_name", "A location with this name already exists."));
        }

        if (request.UnitId.HasValue)
        {
            var unitExists = await _dbContext.Units.AnyAsync(x => x.GlampingId == glampingId && x.Id == request.UnitId.Value, cancellationToken);
            if (!unitExists)
            {
                return ServiceResult<LocationDto>.Failure(StatusCodes.Status400BadRequest, new ServiceError("invalid_unit", "Unit does not exist for this tenant."));
            }
        }

        location.Name = name;
        location.Type = request.Type;
        location.UnitId = request.UnitId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<LocationDto>.Success(new LocationDto(location.Id, location.Name, location.Type, location.UnitId));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();
        var location = await _dbContext.Locations.FirstOrDefaultAsync(x => x.GlampingId == glampingId && x.Id == id, cancellationToken);

        if (location is null)
        {
            return ServiceResult<bool>.Failure(StatusCodes.Status404NotFound, new ServiceError("not_found", "Location not found."));
        }

        var hasMovements = await _dbContext.StockMovements.AnyAsync(x => x.GlampingId == glampingId && x.LocationId == id, cancellationToken);
        if (hasMovements)
        {
            return ServiceResult<bool>.Failure(StatusCodes.Status409Conflict, new ServiceError("has_history", "Cannot delete a location with stock history."));
        }

        _dbContext.Locations.Remove(location);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<bool>.Success(true);
    }
}
