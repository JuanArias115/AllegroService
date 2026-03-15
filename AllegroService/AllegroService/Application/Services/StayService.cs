using AllegroService.Application.Common;
using AllegroService.Application.DTOs.Common;
using AllegroService.Application.DTOs.Folios;
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
    private readonly IFolioService _folioService;

    public StayService(AppDbContext dbContext, ICurrentUserContext currentUser, IFolioService folioService)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _folioService = folioService;
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

    public async Task<ServiceResult<IReadOnlyCollection<ConsumptionDto>>> GetConsumptionsAsync(Guid stayId, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();

        var stay = await _dbContext.Stays.AsNoTracking()
            .FirstOrDefaultAsync(x => x.GlampingId == glampingId && x.Id == stayId, cancellationToken);

        if (stay is null)
        {
            return ServiceResult<IReadOnlyCollection<ConsumptionDto>>.Failure(
                StatusCodes.Status404NotFound,
                new ServiceError("not_found", "Stay not found."));
        }

        var consumptions = await GetConsumptionsForStayAsync(glampingId, stayId, stay.ReservationId, cancellationToken);
        return ServiceResult<IReadOnlyCollection<ConsumptionDto>>.Success(consumptions);
    }

    public async Task<ServiceResult<IReadOnlyCollection<ConsumptionDto>>> GetReservationConsumptionsAsync(Guid reservationId, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();

        var reservation = await _dbContext.Reservations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.GlampingId == glampingId && x.Id == reservationId, cancellationToken);

        if (reservation is null)
        {
            return ServiceResult<IReadOnlyCollection<ConsumptionDto>>.Failure(
                StatusCodes.Status404NotFound,
                new ServiceError("not_found", "Reservation not found."));
        }

        var stays = await _dbContext.Stays.AsNoTracking()
            .Where(x => x.GlampingId == glampingId && x.ReservationId == reservationId)
            .OrderByDescending(x => x.CheckInAt)
            .ToListAsync(cancellationToken);

        var results = new List<ConsumptionDto>();

        foreach (var stay in stays)
        {
            var consumptions = await GetConsumptionsForStayAsync(glampingId, stay.Id, reservationId, cancellationToken);
            results.AddRange(consumptions);
        }

        return ServiceResult<IReadOnlyCollection<ConsumptionDto>>.Success(results
            .OrderByDescending(x => x.CreatedAt)
            .ToList());
    }

    public async Task<ServiceResult<ConsumptionDto>> AddConsumptionAsync(Guid stayId, AddChargeRequest request, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();

        var stay = await _dbContext.Stays.AsNoTracking()
            .FirstOrDefaultAsync(x => x.GlampingId == glampingId && x.Id == stayId, cancellationToken);

        if (stay is null)
        {
            return ServiceResult<ConsumptionDto>.Failure(StatusCodes.Status404NotFound, new ServiceError("not_found", "Stay not found."));
        }

        if (stay.Status != StayStatus.CheckedIn)
        {
            return ServiceResult<ConsumptionDto>.Failure(StatusCodes.Status409Conflict, new ServiceError("invalid_status", "Only active stays can register consumptions."));
        }

        var folioId = await _dbContext.Folios.AsNoTracking()
            .Where(x => x.GlampingId == glampingId && x.StayId == stayId && x.Status == FolioStatus.Open)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (!folioId.HasValue)
        {
            return ServiceResult<ConsumptionDto>.Failure(StatusCodes.Status409Conflict, new ServiceError("open_folio_not_found", "Open folio not found for this stay."));
        }

        var result = await _folioService.AddChargeAsync(folioId.Value, request, cancellationToken);
        if (!result.IsSuccess || result.Data is null)
        {
            return ServiceResult<ConsumptionDto>.Failure(result.StatusCode, result.Errors.ToArray());
        }

        return ServiceResult<ConsumptionDto>.Success(MapConsumption(result.Data, stayId, stay.ReservationId, folioId.Value), result.StatusCode);
    }

    public async Task<ServiceResult<ConsumptionDto>> AddReservationConsumptionAsync(Guid reservationId, AddChargeRequest request, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();

        var stay = await _dbContext.Stays.AsNoTracking()
            .Where(x => x.GlampingId == glampingId && x.ReservationId == reservationId && x.Status == StayStatus.CheckedIn)
            .OrderByDescending(x => x.CheckInAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (stay is null)
        {
            return ServiceResult<ConsumptionDto>.Failure(
                StatusCodes.Status409Conflict,
                new ServiceError("active_stay_not_found", "Reservation does not have an active stay."));
        }

        return await AddConsumptionAsync(stay.Id, request, cancellationToken);
    }

    public async Task<ServiceResult<CheckoutSummaryDto>> GetCheckoutSummaryAsync(Guid stayId, string? language, CancellationToken cancellationToken)
    {
        var glampingId = _currentUser.GetRequiredGlampingId();

        var stay = await _dbContext.Stays.AsNoTracking()
            .Include(x => x.Reservation)
            .ThenInclude(x => x!.Guest)
            .FirstOrDefaultAsync(x => x.GlampingId == glampingId && x.Id == stayId, cancellationToken);

        if (stay is null)
        {
            return ServiceResult<CheckoutSummaryDto>.Failure(StatusCodes.Status404NotFound, new ServiceError("not_found", "Stay not found."));
        }

        var folioIds = await _dbContext.Folios.AsNoTracking()
            .Where(x => x.GlampingId == glampingId && x.StayId == stayId)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var charges = await _dbContext.Charges.AsNoTracking()
            .Where(x => x.GlampingId == glampingId && folioIds.Contains(x.FolioId))
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var chargeIds = charges.Select(x => x.Id).ToList();

        var chargeItems = await _dbContext.ChargeItems.AsNoTracking()
            .Where(x => x.GlampingId == glampingId && chargeIds.Contains(x.ChargeId))
            .ToListAsync(cancellationToken);

        var chargeItemProductIds = chargeItems
            .Where(x => x.ProductId.HasValue)
            .Select(x => x.ProductId!.Value)
            .Distinct()
            .ToList();

        var productNames = await _dbContext.Products.AsNoTracking()
            .Where(x => x.GlampingId == glampingId && chargeItemProductIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var payments = await _dbContext.Payments.AsNoTracking()
            .Where(x => x.GlampingId == glampingId && folioIds.Contains(x.FolioId) && x.Status == PaymentStatus.Paid)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var items = BuildCheckoutItems(charges, chargeItems, productNames);
        var chargesTotal = charges.Sum(x => x.Total);
        var paymentsTotal = payments.Sum(x => x.Amount);
        var balance = decimal.Round(chargesTotal - paymentsTotal, 2, MidpointRounding.AwayFromZero);
        var message = BuildCheckoutMessage(stay, items, chargesTotal, paymentsTotal, balance, language);

        var summary = new CheckoutSummaryDto(
            stay.Id,
            stay.ReservationId,
            stay.Reservation?.Code,
            stay.Reservation?.Guest.FullName,
            stay.Reservation?.Guest.Phone,
            stay.CheckInAt,
            stay.CheckOutAt,
            new CheckoutSummaryTotalsDto(chargesTotal, paymentsTotal, balance),
            items,
            message);

        return ServiceResult<CheckoutSummaryDto>.Success(summary);
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

    private async Task<List<ConsumptionDto>> GetConsumptionsForStayAsync(
        Guid glampingId,
        Guid stayId,
        Guid? reservationId,
        CancellationToken cancellationToken)
    {
        var folioIds = await _dbContext.Folios.AsNoTracking()
            .Where(x => x.GlampingId == glampingId && x.StayId == stayId)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (folioIds.Count == 0)
        {
            return new List<ConsumptionDto>();
        }

        var charges = await _dbContext.Charges.AsNoTracking()
            .Where(x => x.GlampingId == glampingId && folioIds.Contains(x.FolioId))
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var chargeIds = charges.Select(x => x.Id).ToList();

        var chargeItems = await _dbContext.ChargeItems.AsNoTracking()
            .Where(x => x.GlampingId == glampingId && chargeIds.Contains(x.ChargeId))
            .ToListAsync(cancellationToken);

        var productIds = chargeItems.Where(x => x.ProductId.HasValue).Select(x => x.ProductId!.Value).Distinct().ToList();
        var productNames = await _dbContext.Products.AsNoTracking()
            .Where(x => x.GlampingId == glampingId && productIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        return charges.Select(charge => new ConsumptionDto(
            charge.Id,
            stayId,
            reservationId,
            charge.FolioId,
            charge.Source,
            charge.Description,
            charge.Qty,
            charge.UnitPrice,
            charge.Total,
            charge.CreatedAt,
            chargeItems
                .Where(item => item.ChargeId == charge.Id)
                .Select(item => new ConsumptionItemDto(
                    item.Id,
                    item.ProductId,
                    item.ProductId.HasValue && productNames.TryGetValue(item.ProductId.Value, out var productName) ? productName : null,
                    item.Qty,
                    item.UnitPrice,
                    item.Total))
                .ToList()))
            .ToList();
    }

    private static ConsumptionDto MapConsumption(ChargeDto charge, Guid stayId, Guid? reservationId, Guid folioId)
        => new(
            charge.Id,
            stayId,
            reservationId,
            folioId,
            charge.Source,
            charge.Description,
            charge.Qty,
            charge.UnitPrice,
            charge.Total,
            charge.CreatedAt,
            charge.Items.Select(item => new ConsumptionItemDto(
                item.Id,
                item.ProductId,
                item.ProductName,
                item.Qty,
                item.UnitPrice,
                item.Total)).ToList());

    private static List<CheckoutSummaryItemDto> BuildCheckoutItems(
        IReadOnlyCollection<Charge> charges,
        IReadOnlyCollection<ChargeItem> chargeItems,
        IReadOnlyDictionary<Guid, string> productNames)
    {
        var itemSummaries = chargeItems
            .Select(item => new CheckoutSummaryItemDto(
                item.ProductId.HasValue && productNames.TryGetValue(item.ProductId.Value, out var productName)
                    ? productName
                    : "Extra",
                item.Qty,
                item.UnitPrice,
                item.Total,
                null))
            .Take(10)
            .ToList();

        if (itemSummaries.Count > 0)
        {
            return itemSummaries;
        }

        return charges
            .Select(charge => new CheckoutSummaryItemDto(
                charge.Description,
                charge.Qty,
                charge.UnitPrice,
                charge.Total,
                charge.Source.ToString()))
            .Take(10)
            .ToList();
    }

    private static string BuildCheckoutMessage(
        Stay stay,
        IReadOnlyCollection<CheckoutSummaryItemDto> items,
        decimal chargesTotal,
        decimal paymentsTotal,
        decimal balance,
        string? language)
    {
        var isEnglish = string.Equals(language, "en", StringComparison.OrdinalIgnoreCase);
        var guestName = stay.Reservation?.Guest.FullName ?? (isEnglish ? "guest" : "huesped");
        var reservationCode = stay.Reservation?.Code ?? "-";
        var lines = items.Select(item => isEnglish
            ? $"- {item.Label}: {item.Qty:0.###} x {item.UnitPrice:0.00} = {item.Total:0.00}"
            : $"- {item.Label}: {item.Qty:0.###} x {item.UnitPrice:0.00} = {item.Total:0.00}");

        var header = isEnglish
            ? $"Hello {guestName}, here is your stay summary.\nReservation: {reservationCode}\nCheck-in: {stay.CheckInAt:yyyy-MM-dd HH:mm}\nCheck-out: {(stay.CheckOutAt?.ToString("yyyy-MM-dd HH:mm") ?? "-")}"
            : $"Hola {guestName}, este es el resumen de tu estadia.\nReserva: {reservationCode}\nCheck-in: {stay.CheckInAt:yyyy-MM-dd HH:mm}\nCheck-out: {(stay.CheckOutAt?.ToString("yyyy-MM-dd HH:mm") ?? "-")}";

        var totals = isEnglish
            ? $"\nCharges: {chargesTotal:0.00}\nPayments: {paymentsTotal:0.00}\nBalance: {balance:0.00}"
            : $"\nCargos: {chargesTotal:0.00}\nPagos: {paymentsTotal:0.00}\nSaldo: {balance:0.00}";

        var itemsSectionTitle = isEnglish ? "\nItems:" : "\nConceptos:";

        return string.Join("\n", new[] { header, itemsSectionTitle, string.Join("\n", lines), totals }.Where(x => !string.IsNullOrWhiteSpace(x)));
    }
}
