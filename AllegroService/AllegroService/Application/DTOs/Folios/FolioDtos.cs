using AllegroService.Domain.Enums;

namespace AllegroService.Application.DTOs.Folios;

public sealed record FolioDetailDto(
    Guid Id,
    Guid StayId,
    FolioStatus Status,
    DateTimeOffset OpenedAt,
    DateTimeOffset? ClosedAt,
    decimal ChargesTotal,
    decimal PaymentsTotal,
    decimal Balance,
    IReadOnlyCollection<ChargeDto> Charges,
    IReadOnlyCollection<PaymentDto> Payments);

public sealed record ChargeDto(
    Guid Id,
    ChargeSource Source,
    string Description,
    decimal Qty,
    decimal UnitPrice,
    decimal Total,
    IReadOnlyCollection<ChargeItemDto> Items,
    DateTimeOffset CreatedAt);

public sealed record ChargeItemDto(
    Guid Id,
    Guid? ProductId,
    decimal Qty,
    decimal UnitPrice,
    decimal Total);

public sealed record PaymentDto(
    Guid Id,
    decimal Amount,
    PaymentMethod Method,
    PaymentStatus Status,
    DateTimeOffset? PaidAt,
    string? Reference,
    DateTimeOffset CreatedAt);

public class AddChargeItemRequest
{
    public Guid? ProductId { get; set; }
    public decimal Qty { get; set; }
    public decimal? UnitPrice { get; set; }
}

public class AddChargeRequest
{
    public ChargeSource Source { get; set; } = ChargeSource.Extra;
    public string Description { get; set; } = string.Empty;
    public Guid? LocationId { get; set; }
    public bool AllowOverridePrice { get; set; }
    public decimal? Qty { get; set; }
    public decimal? UnitPrice { get; set; }
    public ICollection<AddChargeItemRequest> Items { get; set; } = new List<AddChargeItemRequest>();
}

public class AddPaymentRequest
{
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public string? Reference { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
}
