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
