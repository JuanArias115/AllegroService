using AllegroService.Domain.Enums;

namespace AllegroService.Application.DTOs.Reservations;

public sealed record ReservationDto(
    Guid Id,
    string Code,
    Guid GuestId,
    string GuestName,
    Guid? UnitId,
    string? UnitName,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    ReservationStatus Status,
    decimal TotalEstimated);

public class CreateReservationRequest
{
    public string Code { get; set; } = string.Empty;
    public Guid GuestId { get; set; }
    public Guid? UnitId { get; set; }
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public decimal TotalEstimated { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
}

public class UpdateReservationRequest
{
    public Guid GuestId { get; set; }
    public Guid? UnitId { get; set; }
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public decimal TotalEstimated { get; set; }
    public ReservationStatus Status { get; set; }
}
