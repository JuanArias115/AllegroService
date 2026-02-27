using AllegroService.Domain.Entities.Base;

namespace AllegroService.Domain.Entities;

public class Glamping : AuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Guest> Guests { get; set; } = new List<Guest>();
    public ICollection<Unit> Units { get; set; } = new List<Unit>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public ICollection<Stay> Stays { get; set; } = new List<Stay>();
    public ICollection<Folio> Folios { get; set; } = new List<Folio>();
    public ICollection<Charge> Charges { get; set; } = new List<Charge>();
    public ICollection<ChargeItem> ChargeItems { get; set; } = new List<ChargeItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Location> Locations { get; set; } = new List<Location>();
    public ICollection<StockBalance> StockBalances { get; set; } = new List<StockBalance>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
