using AllegroService.Domain.Entities;
using AllegroService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AllegroService.Infrastructure.Persistence.Seed;

public static class SeedData
{
    public static readonly Guid DefaultGlampingId = Guid.Parse("8A11E29D-DFBA-4E64-8956-35A3D70AC15F");
    public static readonly Guid DefaultAdminUserId = Guid.Parse("9C7093D5-0365-4F9C-8BFD-9A6C777B9A47");
    public static readonly Guid DefaultAdminUserTenantId = Guid.Parse("4FA97C3D-FCB0-43E5-B44B-9AE72C6E01A4");
    public const string DefaultAdminFirebaseUid = "CHANGE_ME_FIREBASE_UID";
    public static readonly Guid DefaultCategoryFoodId = Guid.Parse("A15703C5-F0AB-4D84-9F6D-0549E750DD57");
    public static readonly Guid DefaultCategoryDrinksId = Guid.Parse("95FCB0AC-4D68-44A7-86AA-58F1E129851F");
    public static readonly Guid DefaultCategoryExtrasId = Guid.Parse("3DC17971-EC2A-48D2-B6B6-D52CF52393AA");
    public static readonly Guid DefaultWarehouseLocationId = Guid.Parse("1D9FBBD6-43B7-41FC-9708-C165E27E89DF");

    private static readonly DateTimeOffset SeedTimestamp = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Glamping>().HasData(new Glamping
        {
            Id = DefaultGlampingId,
            Name = "Demo Glamping",
            Address = "Ruta Escenica 123",
            Timezone = "America/Bogota",
            Currency = "COP",
            CreatedAt = SeedTimestamp,
            UpdatedAt = SeedTimestamp,
            CreatedByUserId = null,
            UpdatedByUserId = null,
            CreatedByFirebaseUid = null,
            UpdatedByFirebaseUid = null
        });

        modelBuilder.Entity<User>().HasData(new User
        {
            Id = DefaultAdminUserId,
            GlampingId = DefaultGlampingId,
            Name = "Admin",
            Email = "admin@demo-glamping.local",
            PasswordHash = "CHANGE_ME",
            Status = UserStatus.Active,
            CreatedAt = SeedTimestamp,
            UpdatedAt = SeedTimestamp,
            CreatedByUserId = null,
            UpdatedByUserId = null,
            CreatedByFirebaseUid = null,
            UpdatedByFirebaseUid = null
        });

        modelBuilder.Entity<UserTenant>().HasData(new UserTenant
        {
            Id = DefaultAdminUserTenantId,
            FirebaseUid = DefaultAdminFirebaseUid,
            Email = "admin@demo-glamping.local",
            GlampingId = DefaultGlampingId,
            Role = UserTenantRole.Admin,
            Status = UserTenantStatus.Active,
            CreatedAt = SeedTimestamp,
            UpdatedAt = SeedTimestamp,
            CreatedByUserId = null,
            UpdatedByUserId = null,
            CreatedByFirebaseUid = null,
            UpdatedByFirebaseUid = null
        });

        modelBuilder.Entity<ProductCategory>().HasData(
            new ProductCategory
            {
                Id = DefaultCategoryFoodId,
                GlampingId = DefaultGlampingId,
                Name = "Alimentos",
                CreatedAt = SeedTimestamp,
                UpdatedAt = SeedTimestamp,
                CreatedByUserId = DefaultAdminUserId,
                UpdatedByUserId = DefaultAdminUserId,
                CreatedByFirebaseUid = DefaultAdminFirebaseUid,
                UpdatedByFirebaseUid = DefaultAdminFirebaseUid
            },
            new ProductCategory
            {
                Id = DefaultCategoryDrinksId,
                GlampingId = DefaultGlampingId,
                Name = "Bebidas",
                CreatedAt = SeedTimestamp,
                UpdatedAt = SeedTimestamp,
                CreatedByUserId = DefaultAdminUserId,
                UpdatedByUserId = DefaultAdminUserId,
                CreatedByFirebaseUid = DefaultAdminFirebaseUid,
                UpdatedByFirebaseUid = DefaultAdminFirebaseUid
            },
            new ProductCategory
            {
                Id = DefaultCategoryExtrasId,
                GlampingId = DefaultGlampingId,
                Name = "Extras",
                CreatedAt = SeedTimestamp,
                UpdatedAt = SeedTimestamp,
                CreatedByUserId = DefaultAdminUserId,
                UpdatedByUserId = DefaultAdminUserId,
                CreatedByFirebaseUid = DefaultAdminFirebaseUid,
                UpdatedByFirebaseUid = DefaultAdminFirebaseUid
            });

        modelBuilder.Entity<Location>().HasData(new Location
        {
            Id = DefaultWarehouseLocationId,
            GlampingId = DefaultGlampingId,
            Name = "Main Warehouse",
            Type = LocationType.Warehouse,
            UnitId = null,
            CreatedAt = SeedTimestamp,
            UpdatedAt = SeedTimestamp,
            CreatedByUserId = DefaultAdminUserId,
            UpdatedByUserId = DefaultAdminUserId,
            CreatedByFirebaseUid = DefaultAdminFirebaseUid,
            UpdatedByFirebaseUid = DefaultAdminFirebaseUid
        });
    }
}
