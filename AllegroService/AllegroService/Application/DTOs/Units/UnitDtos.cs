using AllegroService.Domain.Enums;

namespace AllegroService.Application.DTOs.Units;

public sealed record UnitDto(Guid Id, string Name, string Type, int Capacity, UnitStatus Status);

public class CreateUnitRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Capacity { get; set; }
}

public class UpdateUnitRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public UnitStatus Status { get; set; }
}
