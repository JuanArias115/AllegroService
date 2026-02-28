using AllegroService.Application.Common;
using AllegroService.Application.DTOs.Common;
using AllegroService.Application.DTOs.Stays;
using AllegroService.Application.Interfaces;
using AllegroService.Domain.Entities;
using AllegroService.Domain.Enums;
using AllegroService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AllegroService.Application.Services;

public class StayService : IStayService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserContext _currentUser;

    public StayService(AppDbContext dbContext, ICurrentUserContext currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<ServiceResult<PagedResponse<StayDto>>> GetPagedAsync(ListQueryRequest request, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();

        var query = _dbContext.Stays.AsNoTracking()
            .Where(x => x.GlampingId == glampingId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(x => x.Unit.Name.ToLower().Contains(term)
                || (x.Reservation != null && x.Reservation.Code.ToLower().Contains(term)));
        }

        query = request.Sort?.ToLower() switch
        {
            "checkin" => query.OrderBy(x => x.CheckInAt),
            _ => query.OrderByDescending(x => x.CheckInAt)
        };

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new StayDto(
                x.Id,
                x.ReservationId,
                x.UnitId,
                x.CheckInAt,
                x.CheckOutAt,
                x.Status,
                _dbContext.Folios
                    .Where(f => f.GlampingId == glampingId && f.StayId == x.Id && f.Status == FolioStatus.Open)
                    .Select(f => (Guid?)f.Id)
                    .FirstOrDefault()))
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedResponse<StayDto>>.Success(new PagedResponse<StayDto>
        {
            Items = items,
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }

    public async Task<ServiceResult<StayDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();

        var stay = await _dbContext.Stays.AsNoTracking()
            .Where(x => x.GlampingId == glampingId && x.Id == id)
            .Select(x => new StayDto(
                x.Id,
                x.ReservationId,
                x.UnitId,
                x.CheckInAt,
                x.CheckOutAt,
                x.Status,
                _dbContext.Folios
                    .Where(f => f.GlampingId == glampingId && f.StayId == x.Id && f.Status == FolioStatus.Open)
                    .Select(f => (Guid?)f.Id)
                    .FirstOrDefault()))
            .FirstOrDefaultAsync(cancellationToken);

        return stay is null
            ? ServiceResult<StayDto>.Failure(StatusCodes.Status404NotFound, new ServiceError("not_found", "Stay not found."))
            : ServiceResult<StayDto>.Success(stay);
    }

    public async Task<ServiceResult<CheckInResponse>> CheckInAsync(Guid reservationId, CheckInRequest request, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var reservation = await _dbContext.Reservations
            .FirstOrDefaultAsync(x => x.GlampingId == glampingId && x.Id == reservationId, cancellationToken);

        if (reservation is null)
        {
            return ServiceResult<CheckInResponse>.Failure(StatusCodes.Status404NotFound, new ServiceError("not_found", "Reservation not found."));
        }

        if (reservation.Status != ReservationStatus.Confirmed)
        {
            return ServiceResult<CheckInResponse>.Failure(StatusCodes.Status409Conflict, new ServiceError("invalid_status", "Only confirmed reservations can be checked in."));
        }

        if (!reservation.UnitId.HasValue)
        {
            return ServiceResult<CheckInResponse>.Failure(StatusCodes.Status409Conflict, new ServiceError("unit_required", "Reservation must have an assigned unit before check-in."));
        }

        var unitId = reservation.UnitId.Value;

        var hasActiveStay = await _dbContext.Stays.AnyAsync(
            x => x.GlampingId == glampingId && x.UnitId == unitId && x.Status == StayStatus.CheckedIn,
            cancellationToken);

        if (hasActiveStay)
        {
            return ServiceResult<CheckInResponse>.Failure(StatusCodes.Status409Conflict, new ServiceError("unit_occupied", "Unit already has an active stay."));
        }

        var existingStayForReservation = await _dbContext.Stays.AnyAsync(
            x => x.GlampingId == glampingId && x.ReservationId == reservation.Id && x.Status == StayStatus.CheckedIn,
            cancellationToken);

        if (existingStayForReservation)
        {
            return ServiceResult<CheckInResponse>.Failure(StatusCodes.Status409Conflict, new ServiceError("already_checked_in", "Reservation already has an active stay."));
        }

        var unit = await _dbContext.Units.FirstOrDefaultAsync(x => x.GlampingId == glampingId && x.Id == unitId, cancellationToken);
        if (unit is null)
        {
            return ServiceResult<CheckInResponse>.Failure(StatusCodes.Status400BadRequest, new ServiceError("invalid_unit", "Unit not found for reservation."));
        }

        var now = request.CheckInAt ?? DateTimeOffset.UtcNow;

        var stay = new Stay
        {
            Id = Guid.NewGuid(),
            GlampingId = glampingId,
            ReservationId = reservation.Id,
            UnitId = unitId,
            CheckInAt = now,
            Status = StayStatus.CheckedIn
        };

        _dbContext.Stays.Add(stay);

        var folio = new Folio
        {
            Id = Guid.NewGuid(),
            GlampingId = glampingId,
            StayId = stay.Id,
            Status = FolioStatus.Open,
            OpenedAt = now
        };

        _dbContext.Folios.Add(folio);

        if (request.RoomUnitPrice.HasValue)
        {
            var roomNights = request.RoomNights ?? Math.Max(1, reservation.CheckOutDate.DayNumber - reservation.CheckInDate.DayNumber);
            var roomTotal = decimal.Round(roomNights * request.RoomUnitPrice.Value, 2, MidpointRounding.AwayFromZero);

            _dbContext.Charges.Add(new Charge
            {
                Id = Guid.NewGuid(),
                GlampingId = glampingId,
                FolioId = folio.Id,
                Source = ChargeSource.Room,
                Description = string.IsNullOrWhiteSpace(request.RoomDescription) ? "Room charge" : request.RoomDescription.Trim(),
                Qty = roomNights,
                UnitPrice = request.RoomUnitPrice.Value,
                Total = roomTotal
            });
        }

        reservation.Status = ReservationStatus.CheckedIn;
        unit.Status = UnitStatus.Occupied;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ServiceResult<CheckInResponse>.Success(new CheckInResponse(stay.Id, folio.Id));
    }

    public async Task<ServiceResult<CheckOutResponse>> CheckOutAsync(Guid stayId, CheckOutRequest request, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var stay = await _dbContext.Stays
            .FirstOrDefaultAsync(x => x.GlampingId == glampingId && x.Id == stayId, cancellationToken);

        if (stay is null)
        {
            return ServiceResult<CheckOutResponse>.Failure(StatusCodes.Status404NotFound, new ServiceError("not_found", "Stay not found."));
        }

        if (stay.Status != StayStatus.CheckedIn)
        {
            return ServiceResult<CheckOutResponse>.Failure(StatusCodes.Status409Conflict, new ServiceError("invalid_status", "Only checked-in stays can be checked out."));
        }

        var folio = await _dbContext.Folios
            .FirstOrDefaultAsync(x => x.GlampingId == glampingId && x.StayId == stay.Id && x.Status == FolioStatus.Open, cancellationToken);

        if (folio is null)
        {
            return ServiceResult<CheckOutResponse>.Failure(StatusCodes.Status409Conflict, new ServiceError("open_folio_not_found", "Open folio not found for this stay."));
        }

        var chargesTotal = await _dbContext.Charges
            .Where(x => x.GlampingId == glampingId && x.FolioId == folio.Id)
            .SumAsync(x => (decimal?)x.Total, cancellationToken) ?? 0;

        var paymentsTotal = await _dbContext.Payments
            .Where(x => x.GlampingId == glampingId && x.FolioId == folio.Id && x.Status == PaymentStatus.Paid)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0;

        var balance = decimal.Round(chargesTotal - paymentsTotal, 2, MidpointRounding.AwayFromZero);

        if (balance != 0 && !request.Force)
        {
            return ServiceResult<CheckOutResponse>.Failure(
                StatusCodes.Status409Conflict,
                new ServiceError("pending_balance", $"Cannot check out with pending balance: {balance:0.00}."));
        }

        var checkoutAt = request.CheckOutAt ?? DateTimeOffset.UtcNow;

        folio.Status = FolioStatus.Closed;
        folio.ClosedAt = checkoutAt;

        stay.Status = StayStatus.CheckedOut;
        stay.CheckOutAt = checkoutAt;

        var unit = await _dbContext.Units.FirstAsync(x => x.GlampingId == glampingId && x.Id == stay.UnitId, cancellationToken);
        unit.Status = UnitStatus.Dirty;

        if (stay.ReservationId.HasValue)
        {
            var reservation = await _dbContext.Reservations
                .FirstOrDefaultAsync(x => x.GlampingId == glampingId && x.Id == stay.ReservationId.Value, cancellationToken);

            if (reservation is not null)
            {
                reservation.Status = ReservationStatus.CheckedOut;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ServiceResult<CheckOutResponse>.Success(new CheckOutResponse(stay.Id, folio.Id, balance, checkoutAt));
    }
}
