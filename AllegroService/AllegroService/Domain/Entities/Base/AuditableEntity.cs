namespace AllegroService.Domain.Entities.Base;

public abstract class AuditableEntity : IAuditableEntity
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public string? CreatedByFirebaseUid { get; set; }
    public string? UpdatedByFirebaseUid { get; set; }
}
