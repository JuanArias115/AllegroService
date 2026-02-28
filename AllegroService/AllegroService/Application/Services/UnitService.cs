using AllegroService.Application.Common;
using AllegroService.Application.DTOs.Common;
using AllegroService.Application.DTOs.Units;
using AllegroService.Application.Interfaces;
using AllegroService.Domain.Entities;
using AllegroService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AllegroService.Application.Services;

public class UnitService : IUnitService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserContext _currentUser;

    public UnitService(AppDbContext dbContext, ICurrentUserContext currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<ServiceResult<PagedResponse<UnitDto>>> GetPagedAsync(ListQueryRequest request, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();
        var query = _dbContext.Units.AsNoTracking().Where(x => x.GlampingId == glampingId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(term) || x.Type.ToLower().Contains(term));
        }

        query = request.Sort?.ToLower() switch
        {
            "name_desc" => query.OrderByDescending(x => x.Name),
            "capacity" => query.OrderBy(x => x.Capacity),
            "capacity_desc" => query.OrderByDescending(x => x.Capacity),
            _ => query.OrderBy(x => x.Name)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new UnitDto(x.Id, x.Name, x.Type, x.Capacity, x.Status))
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedResponse<UnitDto>>.Success(new PagedResponse<UnitDto>
        {
            Items = items,
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }

    public async Task<ServiceResult<UnitDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();
        var unit = await _dbContext.Units.AsNoTracking()
            .Where(x => x.GlampingId == glampingId && x.Id == id)
            .Select(x => new UnitDto(x.Id, x.Name, x.Type, x.Capacity, x.Status))
            .FirstOrDefaultAsync(cancellationToken);

        return unit is null
            ? ServiceResult<UnitDto>.Failure(StatusCodes.Status404NotFound, new ServiceError("not_found", "Unit not found."))
            : ServiceResult<UnitDto>.Success(unit);
    }

    public async Task<ServiceResult<UnitDto>> CreateAsync(CreateUnitRequest request, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();

        var exists = await _dbContext.Units.AnyAsync(
            x => x.GlampingId == glampingId && x.Name == request.Name,
            cancellationToken);

        if (exists)
        {
            return ServiceResult<UnitDto>.Failure(
                StatusCodes.Status409Conflict,
                new ServiceError("duplicate_name", "A unit with the same name already exists."));
        }

        var unit = new Unit
        {
            Id = Guid.NewGuid(),
            GlampingId = glampingId,
            Name = request.Name.Trim(),
            Type = request.Type.Trim(),
            Capacity = request.Capacity
        };

        _dbContext.Units.Add(unit);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<UnitDto>.Success(
            new UnitDto(unit.Id, unit.Name, unit.Type, unit.Capacity, unit.Status),
            StatusCodes.Status201Created);
    }

    public async Task<ServiceResult<UnitDto>> UpdateAsync(Guid id, UpdateUnitRequest request, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();
        var unit = await _dbContext.Units.FirstOrDefaultAsync(x => x.GlampingId == glampingId && x.Id == id, cancellationToken);

        if (unit is null)
        {
            return ServiceResult<UnitDto>.Failure(StatusCodes.Status404NotFound, new ServiceError("not_found", "Unit not found."));
        }

        var duplicate = await _dbContext.Units.AnyAsync(
            x => x.GlampingId == glampingId && x.Name == request.Name && x.Id != id,
            cancellationToken);

        if (duplicate)
        {
            return ServiceResult<UnitDto>.Failure(
                StatusCodes.Status409Conflict,
                new ServiceError("duplicate_name", "A unit with the same name already exists."));
        }

        unit.Name = request.Name.Trim();
        unit.Type = request.Type.Trim();
        unit.Capacity = request.Capacity;
        unit.Status = request.Status;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<UnitDto>.Success(new UnitDto(unit.Id, unit.Name, unit.Type, unit.Capacity, unit.Status));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();
        var unit = await _dbContext.Units.FirstOrDefaultAsync(x => x.GlampingId == glampingId && x.Id == id, cancellationToken);

        if (unit is null)
        {
            return ServiceResult<bool>.Failure(StatusCodes.Status404NotFound, new ServiceError("not_found", "Unit not found."));
        }

        var hasStays = await _dbContext.Stays.AnyAsync(x => x.GlampingId == glampingId && x.UnitId == id, cancellationToken);
        if (hasStays)
        {
            return ServiceResult<bool>.Failure(
                StatusCodes.Status409Conflict,
                new ServiceError("has_history", "Cannot delete a unit with stay history."));
        }

        _dbContext.Units.Remove(unit);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<bool>.Success(true);
    }
}
