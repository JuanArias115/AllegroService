namespace AllegroService.Application.Interfaces;

public interface ICurrentUserContext
{
    bool IsAuthenticated { get; }
    Guid GetRequiredGlampingId();
    Guid? GetCurrentUserId();
    string? GetCurrentUserEmail();
}
