using AllegroService.Domain.Enums;

namespace AllegroService.Application.DTOs.Locations;

public sealed record LocationDto(Guid Id, string Name, LocationType Type, Guid? UnitId);

public class CreateLocationRequest
{
    public string Name { get; set; } = string.Empty;
    public LocationType Type { get; set; }
    public Guid? UnitId { get; set; }
}

public class UpdateLocationRequest
{
    public string Name { get; set; } = string.Empty;
    public LocationType Type { get; set; }
    public Guid? UnitId { get; set; }
}
