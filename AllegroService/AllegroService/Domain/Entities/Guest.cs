using AllegroService.Domain.Entities.Base;

namespace AllegroService.Domain.Entities;

public class Guest : TenantEntity
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? DocumentId { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
