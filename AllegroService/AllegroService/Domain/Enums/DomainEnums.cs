namespace AllegroService.Domain.Enums;

public enum UserStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3
}

public enum UserTenantRole
{
    Admin = 1,
    Reception = 2,
    Restaurant = 3,
    Inventory = 4
}

public enum UserTenantStatus
{
    Pending = 1,
    Active = 2,
    Disabled = 3
}

public enum UnitStatus
{
    Available = 1,
    Occupied = 2,
    Dirty = 3,
    Maintenance = 4
}

public enum ReservationStatus
{
    Pending = 1,
    Confirmed = 2,
    CheckedIn = 3,
    CheckedOut = 4,
    Cancelled = 5
}

public enum StayStatus
{
    CheckedIn = 1,
    CheckedOut = 2,
    Cancelled = 3
}

public enum FolioStatus
{
    Open = 1,
    Closed = 2
}

public enum ChargeSource
{
    Room = 1,
    Minibar = 2,
    Restaurant = 3,
    Extra = 4
}

public enum PaymentMethod
{
    Cash = 1,
    Card = 2,
    Transfer = 3,
    Online = 4
}

public enum PaymentStatus
{
    Pending = 1,
    Paid = 2,
    Void = 3
}

public enum StockMovementType
{
    In = 1,
    Out = 2,
    Transfer = 3,
    Adjust = 4
}

public enum LocationType
{
    Warehouse = 1,
    Kitchen = 2,
    Bar = 3,
    UnitMinibar = 4
}
