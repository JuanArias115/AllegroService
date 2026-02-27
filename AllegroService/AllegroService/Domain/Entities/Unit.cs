using AllegroService.Domain.Entities.Base;
using AllegroService.Domain.Enums;

namespace AllegroService.Domain.Entities;

public class Unit : TenantEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public UnitStatus Status { get; set; } = UnitStatus.Available;

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public ICollection<Stay> Stays { get; set; } = new List<Stay>();
    public ICollection<Location> Locations { get; set; } = new List<Location>();
}
