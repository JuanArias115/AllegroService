namespace AllegroService.Application.DTOs.Guests;

public sealed record GuestDto(
    Guid Id,
    string FullName,
    string? DocumentId,
    string Phone,
    string Email);

public class CreateGuestRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? DocumentId { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class UpdateGuestRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? DocumentId { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
