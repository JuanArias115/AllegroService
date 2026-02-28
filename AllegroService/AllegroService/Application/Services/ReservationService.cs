using AllegroService.Application.Common;
using AllegroService.Application.DTOs.Common;
using AllegroService.Application.DTOs.Reservations;
using AllegroService.Application.Interfaces;
using AllegroService.Domain.Entities;
using AllegroService.Domain.Enums;
using AllegroService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AllegroService.Application.Services;

public class ReservationService : IReservationService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserContext _currentUser;

    public ReservationService(AppDbContext dbContext, ICurrentUserContext currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<ServiceResult<PagedResponse<ReservationDto>>> GetPagedAsync(ListQueryRequest request, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();

        var query = _dbContext.Reservations.AsNoTracking()
            .Include(x => x.Guest)
            .Include(x => x.Unit)
            .Where(x => x.GlampingId == glampingId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(x => x.Code.ToLower().Contains(term)
                || x.Guest.FullName.ToLower().Contains(term)
                || (x.Unit != null && x.Unit.Name.ToLower().Contains(term)));
        }

        query = request.Sort?.ToLower() switch
        {
            "checkin" => query.OrderBy(x => x.CheckInDate),
            "checkin_desc" => query.OrderByDescending(x => x.CheckInDate),
            "code_desc" => query.OrderByDescending(x => x.Code),
            _ => query.OrderBy(x => x.Code)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => MapDto(x))
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedResponse<ReservationDto>>.Success(new PagedResponse<ReservationDto>
        {
            Items = items,
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }

    public async Task<ServiceResult<ReservationDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();

        var reservation = await _dbContext.Reservations.AsNoTracking()
            .Include(x => x.Guest)
            .Include(x => x.Unit)
            .FirstOrDefaultAsync(x => x.GlampingId == glampingId && x.Id == id, cancellationToken);

        return reservation is null
            ? ServiceResult<ReservationDto>.Failure(StatusCodes.Status404NotFound, new ServiceError("not_found", "Reservation not found."))
            : ServiceResult<ReservationDto>.Success(MapDto(reservation));
    }

    public async Task<ServiceResult<ReservationDto>> CreateAsync(CreateReservationRequest request, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();

        var guestExists = await _dbContext.Guests.AnyAsync(x => x.GlampingId == glampingId && x.Id == request.GuestId, cancellationToken);
        if (!guestExists)
        {
            return ServiceResult<ReservationDto>.Failure(StatusCodes.Status400BadRequest, new ServiceError("invalid_guest", "Guest does not exist for this tenant."));
        }

        if (request.UnitId.HasValue)
        {
            var unitExists = await _dbContext.Units.AnyAsync(x => x.GlampingId == glampingId && x.Id == request.UnitId.Value, cancellationToken);
            if (!unitExists)
            {
                return ServiceResult<ReservationDto>.Failure(StatusCodes.Status400BadRequest, new ServiceError("invalid_unit", "Unit does not exist for this tenant."));
            }

            var hasOverlap = await HasUnitOverlapAsync(glampingId, request.UnitId.Value, request.CheckInDate, request.CheckOutDate, null, cancellationToken);
            if (hasOverlap)
            {
                return ServiceResult<ReservationDto>.Failure(StatusCodes.Status409Conflict, new ServiceError("unit_unavailable", "Unit is occupied in the requested date range."));
            }
        }

        var code = request.Code.Trim();
        var duplicateCode = await _dbContext.Reservations.AnyAsync(x => x.GlampingId == glampingId && x.Code == code, cancellationToken);
        if (duplicateCode)
        {
            return ServiceResult<ReservationDto>.Failure(StatusCodes.Status409Conflict, new ServiceError("duplicate_code", "Reservation code already exists."));
        }

        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            GlampingId = glampingId,
            Code = code,
            GuestId = request.GuestId,
            UnitId = request.UnitId,
            CheckInDate = request.CheckInDate,
            CheckOutDate = request.CheckOutDate,
            Status = request.Status,
            TotalEstimated = request.TotalEstimated
        };

        _dbContext.Reservations.Add(reservation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var created = await _dbContext.Reservations.AsNoTracking()
            .Include(x => x.Guest)
            .Include(x => x.Unit)
            .FirstAsync(x => x.GlampingId == glampingId && x.Id == reservation.Id, cancellationToken);

        return ServiceResult<ReservationDto>.Success(MapDto(created), StatusCodes.Status201Created);
    }

    public async Task<ServiceResult<ReservationDto>> UpdateAsync(Guid id, UpdateReservationRequest request, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();
        var reservation = await _dbContext.Reservations.FirstOrDefaultAsync(x => x.GlampingId == glampingId && x.Id == id, cancellationToken);

        if (reservation is null)
        {
            return ServiceResult<ReservationDto>.Failure(StatusCodes.Status404NotFound, new ServiceError("not_found", "Reservation not found."));
        }

        var guestExists = await _dbContext.Guests.AnyAsync(x => x.GlampingId == glampingId && x.Id == request.GuestId, cancellationToken);
        if (!guestExists)
        {
            return ServiceResult<ReservationDto>.Failure(StatusCodes.Status400BadRequest, new ServiceError("invalid_guest", "Guest does not exist for this tenant."));
        }

        if (request.UnitId.HasValue)
        {
            var unitExists = await _dbContext.Units.AnyAsync(x => x.GlampingId == glampingId && x.Id == request.UnitId.Value, cancellationToken);
            if (!unitExists)
            {
                return ServiceResult<ReservationDto>.Failure(StatusCodes.Status400BadRequest, new ServiceError("invalid_unit", "Unit does not exist for this tenant."));
            }

            var hasOverlap = await HasUnitOverlapAsync(glampingId, request.UnitId.Value, request.CheckInDate, request.CheckOutDate, id, cancellationToken);
            if (hasOverlap)
            {
                return ServiceResult<ReservationDto>.Failure(StatusCodes.Status409Conflict, new ServiceError("unit_unavailable", "Unit is occupied in the requested date range."));
            }
        }

        reservation.GuestId = request.GuestId;
        reservation.UnitId = request.UnitId;
        reservation.CheckInDate = request.CheckInDate;
        reservation.CheckOutDate = request.CheckOutDate;
        reservation.Status = request.Status;
        reservation.TotalEstimated = request.TotalEstimated;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var updated = await _dbContext.Reservations.AsNoTracking()
            .Include(x => x.Guest)
            .Include(x => x.Unit)
            .FirstAsync(x => x.GlampingId == glampingId && x.Id == reservation.Id, cancellationToken);

        return ServiceResult<ReservationDto>.Success(MapDto(updated));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();
        var reservation = await _dbContext.Reservations.FirstOrDefaultAsync(x => x.GlampingId == glampingId && x.Id == id, cancellationToken);

        if (reservation is null)
        {
            return ServiceResult<bool>.Failure(StatusCodes.Status404NotFound, new ServiceError("not_found", "Reservation not found."));
        }

        var hasStay = await _dbContext.Stays.AnyAsync(x => x.GlampingId == glampingId && x.ReservationId == id, cancellationToken);
        if (hasStay)
        {
            return ServiceResult<bool>.Failure(StatusCodes.Status409Conflict, new ServiceError("has_stay", "Cannot delete reservation with related stays."));
        }

        _dbContext.Reservations.Remove(reservation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<bool>.Success(true);
    }

    private async Task<bool> HasUnitOverlapAsync(
        Guid glampingId,
        Guid unitId,
        DateOnly checkIn,
        DateOnly checkOut,
        Guid? excludeReservationId,
        CancellationToken cancellationToken)
    {
        var blockedStatuses = new[]
        {
            ReservationStatus.Pending,
            ReservationStatus.Confirmed,
            ReservationStatus.CheckedIn
        };

        var overlapsReservation = await _dbContext.Reservations.AnyAsync(x =>
            x.GlampingId == glampingId
            && x.UnitId == unitId
            && (!excludeReservationId.HasValue || x.Id != excludeReservationId.Value)
            && blockedStatuses.Contains(x.Status)
            && checkIn < x.CheckOutDate
            && checkOut > x.CheckInDate,
            cancellationToken);

        if (overlapsReservation)
        {
            return true;
        }

        return await _dbContext.Stays.AnyAsync(x =>
            x.GlampingId == glampingId
            && x.UnitId == unitId
            && x.Status == StayStatus.CheckedIn,
            cancellationToken);
    }

    private static ReservationDto MapDto(Reservation reservation)
        => new(
            reservation.Id,
            reservation.Code,
            reservation.GuestId,
            reservation.Guest.FullName,
            reservation.UnitId,
            reservation.Unit?.Name,
            reservation.CheckInDate,
            reservation.CheckOutDate,
            reservation.Status,
            reservation.TotalEstimated);
}
