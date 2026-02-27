using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AllegroService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Glampings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Timezone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Glampings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Guests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DocumentId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Phone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    GlampingId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Guests_Glampings_GlampingId",
                        column: x => x.GlampingId,
                        principalTable: "Glampings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    GlampingId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCategories_Glampings_GlampingId",
                        column: x => x.GlampingId,
                        principalTable: "Glampings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Units",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    GlampingId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Units", x => x.Id);
                    table.CheckConstraint("CK_Units_Capacity", "\"Capacity\" > 0");
                    table.ForeignKey(
                        name: "FK_Units_Glampings_GlampingId",
                        column: x => x.GlampingId,
                        principalTable: "Glampings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    GlampingId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Glampings_GlampingId",
                        column: x => x.GlampingId,
                        principalTable: "Glampings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Unit = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SalePrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CostPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TrackStock = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    GlampingId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.CheckConstraint("CK_Products_SalePriceNonNegative", "\"SalePrice\" >= 0");
                    table.ForeignKey(
                        name: "FK_Products_Glampings_GlampingId",
                        column: x => x.GlampingId,
                        principalTable: "Glampings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_ProductCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    GlampingId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Locations_Glampings_GlampingId",
                        column: x => x.GlampingId,
                        principalTable: "Glampings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Locations_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    GuestId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    CheckInDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CheckOutDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TotalEstimated = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    GlampingId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.CheckConstraint("CK_Reservations_CheckOutAfterCheckIn", "\"CheckOutDate\" > \"CheckInDate\"");
                    table.ForeignKey(
                        name: "FK_Reservations_Glampings_GlampingId",
                        column: x => x.GlampingId,
                        principalTable: "Glampings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservations_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservations_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockBalances",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    QtyOnHand = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    GlampingId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockBalances", x => new { x.ProductId, x.LocationId });
                    table.ForeignKey(
                        name: "FK_StockBalances_Glampings_GlampingId",
                        column: x => x.GlampingId,
                        principalTable: "Glampings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockBalances_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockBalances_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockMovements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Qty = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    ReferenceType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    GlampingId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovements", x => x.Id);
                    table.CheckConstraint("CK_StockMovements_QtyPositive", "\"Qty\" > 0");
                    table.ForeignKey(
                        name: "FK_StockMovements_Glampings_GlampingId",
                        column: x => x.GlampingId,
                        principalTable: "Glampings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockMovements_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockMovements_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Stays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReservationId = table.Column<Guid>(type: "uuid", nullable: true),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckInAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CheckOutAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    GlampingId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stays", x => x.Id);
                    table.CheckConstraint("CK_Stays_CheckOutAfterCheckIn", "\"CheckOutAt\" IS NULL OR \"CheckOutAt\" > \"CheckInAt\"");
                    table.ForeignKey(
                        name: "FK_Stays_Glampings_GlampingId",
                        column: x => x.GlampingId,
                        principalTable: "Glampings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Stays_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Stays_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Folios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StayId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OpenedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    GlampingId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Folios_Glampings_GlampingId",
                        column: x => x.GlampingId,
                        principalTable: "Glampings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Folios_Stays_StayId",
                        column: x => x.StayId,
                        principalTable: "Stays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Charges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FolioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Qty = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    GlampingId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Charges", x => x.Id);
                    table.CheckConstraint("CK_Charges_QtyPositive", "\"Qty\" > 0");
                    table.CheckConstraint("CK_Charges_TotalNonNegative", "\"Total\" >= 0");
                    table.ForeignKey(
                        name: "FK_Charges_Folios_FolioId",
                        column: x => x.FolioId,
                        principalTable: "Folios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Charges_Glampings_GlampingId",
                        column: x => x.GlampingId,
                        principalTable: "Glampings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FolioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Method = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PaidAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    GlampingId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.CheckConstraint("CK_Payments_AmountPositive", "\"Amount\" > 0");
                    table.ForeignKey(
                        name: "FK_Payments_Folios_FolioId",
                        column: x => x.FolioId,
                        principalTable: "Folios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Glampings_GlampingId",
                        column: x => x.GlampingId,
                        principalTable: "Glampings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChargeItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChargeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: true),
                    Qty = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    GlampingId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChargeItems", x => x.Id);
                    table.CheckConstraint("CK_ChargeItems_QtyPositive", "\"Qty\" > 0");
                    table.CheckConstraint("CK_ChargeItems_TotalNonNegative", "\"Total\" >= 0");
                    table.ForeignKey(
                        name: "FK_ChargeItems_Charges_ChargeId",
                        column: x => x.ChargeId,
                        principalTable: "Charges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChargeItems_Glampings_GlampingId",
                        column: x => x.GlampingId,
                        principalTable: "Glampings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChargeItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Glampings",
                columns: new[] { "Id", "Address", "CreatedAt", "CreatedByUserId", "Currency", "Name", "Timezone", "UpdatedAt", "UpdatedByUserId" },
                values: new object[] { new Guid("8a11e29d-dfba-4e64-8956-35a3d70ac15f"), "Ruta Escenica 123", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "COP", "Demo Glamping", "America/Bogota", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null });

            migrationBuilder.InsertData(
                table: "Locations",
                columns: new[] { "Id", "CreatedAt", "CreatedByUserId", "GlampingId", "Name", "Type", "UnitId", "UpdatedAt", "UpdatedByUserId" },
                values: new object[] { new Guid("1d9fbbd6-43b7-41fc-9708-c165e27e89df"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("9c7093d5-0365-4f9c-8bfd-9a6c777b9a47"), new Guid("8a11e29d-dfba-4e64-8956-35a3d70ac15f"), "Main Warehouse", 1, null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("9c7093d5-0365-4f9c-8bfd-9a6c777b9a47") });

            migrationBuilder.InsertData(
                table: "ProductCategories",
                columns: new[] { "Id", "CreatedAt", "CreatedByUserId", "GlampingId", "Name", "UpdatedAt", "UpdatedByUserId" },
                values: new object[,]
                {
                    { new Guid("3dc17971-ec2a-48d2-b6b6-d52cf52393aa"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("9c7093d5-0365-4f9c-8bfd-9a6c777b9a47"), new Guid("8a11e29d-dfba-4e64-8956-35a3d70ac15f"), "Extras", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("9c7093d5-0365-4f9c-8bfd-9a6c777b9a47") },
                    { new Guid("95fcb0ac-4d68-44a7-86aa-58f1e129851f"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("9c7093d5-0365-4f9c-8bfd-9a6c777b9a47"), new Guid("8a11e29d-dfba-4e64-8956-35a3d70ac15f"), "Bebidas", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("9c7093d5-0365-4f9c-8bfd-9a6c777b9a47") },
                    { new Guid("a15703c5-f0ab-4d84-9f6d-0549e750dd57"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("9c7093d5-0365-4f9c-8bfd-9a6c777b9a47"), new Guid("8a11e29d-dfba-4e64-8956-35a3d70ac15f"), "Alimentos", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("9c7093d5-0365-4f9c-8bfd-9a6c777b9a47") }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "CreatedByUserId", "Email", "GlampingId", "Name", "PasswordHash", "Status", "UpdatedAt", "UpdatedByUserId" },
                values: new object[] { new Guid("9c7093d5-0365-4f9c-8bfd-9a6c777b9a47"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "admin@demo-glamping.local", new Guid("8a11e29d-dfba-4e64-8956-35a3d70ac15f"), "Admin", "CHANGE_ME", 1, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null });

            migrationBuilder.CreateIndex(
                name: "IX_ChargeItems_ChargeId",
                table: "ChargeItems",
                column: "ChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChargeItems_GlampingId",
                table: "ChargeItems",
                column: "GlampingId");

            migrationBuilder.CreateIndex(
                name: "IX_ChargeItems_GlampingId_ChargeId",
                table: "ChargeItems",
                columns: new[] { "GlampingId", "ChargeId" });

            migrationBuilder.CreateIndex(
                name: "IX_ChargeItems_GlampingId_ProductId",
                table: "ChargeItems",
                columns: new[] { "GlampingId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_ChargeItems_ProductId",
                table: "ChargeItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Charges_FolioId",
                table: "Charges",
                column: "FolioId");

            migrationBuilder.CreateIndex(
                name: "IX_Charges_GlampingId",
                table: "Charges",
                column: "GlampingId");

            migrationBuilder.CreateIndex(
                name: "IX_Charges_GlampingId_FolioId",
                table: "Charges",
                columns: new[] { "GlampingId", "FolioId" });

            migrationBuilder.CreateIndex(
                name: "IX_Folios_GlampingId",
                table: "Folios",
                column: "GlampingId");

            migrationBuilder.CreateIndex(
                name: "IX_Folios_StayId",
                table: "Folios",
                column: "StayId",
                unique: true,
                filter: "\"Status\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Guests_GlampingId",
                table: "Guests",
                column: "GlampingId");

            migrationBuilder.CreateIndex(
                name: "IX_Guests_GlampingId_Email",
                table: "Guests",
                columns: new[] { "GlampingId", "Email" });

            migrationBuilder.CreateIndex(
                name: "IX_Locations_GlampingId",
                table: "Locations",
                column: "GlampingId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_GlampingId_Name",
                table: "Locations",
                columns: new[] { "GlampingId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Locations_GlampingId_UnitId",
                table: "Locations",
                columns: new[] { "GlampingId", "UnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_Locations_UnitId",
                table: "Locations",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_FolioId",
                table: "Payments",
                column: "FolioId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_GlampingId",
                table: "Payments",
                column: "GlampingId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_GlampingId_FolioId",
                table: "Payments",
                columns: new[] { "GlampingId", "FolioId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_GlampingId",
                table: "ProductCategories",
                column: "GlampingId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_GlampingId_Name",
                table: "ProductCategories",
                columns: new[] { "GlampingId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_GlampingId",
                table: "Products",
                column: "GlampingId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_GlampingId_CategoryId",
                table: "Products",
                columns: new[] { "GlampingId", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_GlampingId_Sku",
                table: "Products",
                columns: new[] { "GlampingId", "Sku" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_GlampingId",
                table: "Reservations",
                column: "GlampingId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_GlampingId_Code",
                table: "Reservations",
                columns: new[] { "GlampingId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_GlampingId_GuestId",
                table: "Reservations",
                columns: new[] { "GlampingId", "GuestId" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_GlampingId_UnitId",
                table: "Reservations",
                columns: new[] { "GlampingId", "UnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_GuestId",
                table: "Reservations",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_UnitId",
                table: "Reservations",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Stays_GlampingId",
                table: "Stays",
                column: "GlampingId");

            migrationBuilder.CreateIndex(
                name: "IX_Stays_GlampingId_UnitId",
                table: "Stays",
                columns: new[] { "GlampingId", "UnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_Stays_ReservationId",
                table: "Stays",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_Stays_UnitId",
                table: "Stays",
                column: "UnitId",
                unique: true,
                filter: "\"Status\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_StockBalances_GlampingId",
                table: "StockBalances",
                column: "GlampingId");

            migrationBuilder.CreateIndex(
                name: "IX_StockBalances_GlampingId_LocationId",
                table: "StockBalances",
                columns: new[] { "GlampingId", "LocationId" });

            migrationBuilder.CreateIndex(
                name: "IX_StockBalances_GlampingId_ProductId",
                table: "StockBalances",
                columns: new[] { "GlampingId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_StockBalances_LocationId",
                table: "StockBalances",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_GlampingId",
                table: "StockMovements",
                column: "GlampingId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_GlampingId_LocationId",
                table: "StockMovements",
                columns: new[] { "GlampingId", "LocationId" });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_GlampingId_ProductId",
                table: "StockMovements",
                columns: new[] { "GlampingId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_LocationId",
                table: "StockMovements",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_ProductId",
                table: "StockMovements",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_ReferenceType_ReferenceId",
                table: "StockMovements",
                columns: new[] { "ReferenceType", "ReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_Units_GlampingId",
                table: "Units",
                column: "GlampingId");

            migrationBuilder.CreateIndex(
                name: "IX_Units_GlampingId_Name",
                table: "Units",
                columns: new[] { "GlampingId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_GlampingId",
                table: "Users",
                column: "GlampingId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_GlampingId_Email",
                table: "Users",
                columns: new[] { "GlampingId", "Email" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChargeItems");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "StockBalances");

            migrationBuilder.DropTable(
                name: "StockMovements");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Charges");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Folios");

            migrationBuilder.DropTable(
                name: "ProductCategories");

            migrationBuilder.DropTable(
                name: "Stays");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "Guests");

            migrationBuilder.DropTable(
                name: "Units");

            migrationBuilder.DropTable(
                name: "Glampings");
        }
    }
}
