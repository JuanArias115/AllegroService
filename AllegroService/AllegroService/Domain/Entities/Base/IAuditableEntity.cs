namespace AllegroService.Domain.Entities.Base;

public interface IAuditableEntity
{
    DateTimeOffset CreatedAt { get; set; }
    DateTimeOffset UpdatedAt { get; set; }
    Guid? CreatedByUserId { get; set; }
    Guid? UpdatedByUserId { get; set; }
    string? CreatedByFirebaseUid { get; set; }
    string? UpdatedByFirebaseUid { get; set; }
}
