using AllegroService.Domain.Enums;

namespace AllegroService.Application.DTOs.Stays;

public sealed record StayDto(
    Guid Id,
    Guid? ReservationId,
    Guid UnitId,
    DateTimeOffset CheckInAt,
    DateTimeOffset? CheckOutAt,
    StayStatus Status,
    Guid? OpenFolioId);

public class CheckInRequest
{
    public DateTimeOffset? CheckInAt { get; set; }
    public decimal? RoomUnitPrice { get; set; }
    public decimal? RoomNights { get; set; }
    public string? RoomDescription { get; set; }
}

public sealed record CheckInResponse(Guid StayId, Guid FolioId);

public class CheckOutRequest
{
    public bool Force { get; set; }
    public DateTimeOffset? CheckOutAt { get; set; }
}

public sealed record CheckOutResponse(Guid StayId, Guid FolioId, decimal Balance, DateTimeOffset CheckOutAt);

public sealed record ConsumptionItemDto(
    Guid Id,
    Guid? ProductId,
    string? ProductName,
    decimal Qty,
    decimal UnitPrice,
    decimal Total);

public sealed record ConsumptionDto(
    Guid Id,
    Guid StayId,
    Guid? ReservationId,
    Guid FolioId,
    ChargeSource Source,
    string Description,
    decimal Qty,
    decimal UnitPrice,
    decimal Total,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<ConsumptionItemDto> Items);

public sealed record CheckoutSummaryItemDto(
    string Label,
    decimal Qty,
    decimal UnitPrice,
    decimal Total,
    string? Source);

public sealed record CheckoutSummaryTotalsDto(
    decimal ChargesTotal,
    decimal PaymentsTotal,
    decimal Balance);

public sealed record CheckoutSummaryDto(
    Guid StayId,
    Guid? ReservationId,
    string? ReservationCode,
    string? GuestName,
    string? Phone,
    DateTimeOffset CheckInAt,
    DateTimeOffset? CheckOutAt,
    CheckoutSummaryTotalsDto Totals,
    IReadOnlyCollection<CheckoutSummaryItemDto> Items,
    string Message);
