using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AllegroService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTenantResolutionByFirebaseUid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByFirebaseUid",
                table: "Users",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByFirebaseUid",
                table: "Users",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByFirebaseUid",
                table: "Units",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByFirebaseUid",
                table: "Units",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByFirebaseUid",
                table: "StockMovements",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByFirebaseUid",
                table: "StockMovements",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByFirebaseUid",
                table: "StockBalances",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByFirebaseUid",
                table: "StockBalances",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByFirebaseUid",
                table: "Stays",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByFirebaseUid",
                table: "Stays",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByFirebaseUid",
                table: "Reservations",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByFirebaseUid",
                table: "Reservations",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByFirebaseUid",
                table: "Products",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByFirebaseUid",
                table: "Products",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByFirebaseUid",
                table: "ProductCategories",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByFirebaseUid",
                table: "ProductCategories",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByFirebaseUid",
                table: "Payments",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByFirebaseUid",
                table: "Payments",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByFirebaseUid",
                table: "Locations",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByFirebaseUid",
                table: "Locations",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByFirebaseUid",
                table: "Guests",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByFirebaseUid",
                table: "Guests",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByFirebaseUid",
                table: "Glampings",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByFirebaseUid",
                table: "Glampings",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByFirebaseUid",
                table: "Folios",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByFirebaseUid",
                table: "Folios",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByFirebaseUid",
                table: "Charges",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByFirebaseUid",
                table: "Charges",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByFirebaseUid",
                table: "ChargeItems",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByFirebaseUid",
                table: "ChargeItems",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserTenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirebaseUid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GlampingId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByFirebaseUid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedByFirebaseUid = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTenants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTenants_Glampings_GlampingId",
                        column: x => x.GlampingId,
                        principalTable: "Glampings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Glampings",
                keyColumn: "Id",
                keyValue: new Guid("8a11e29d-dfba-4e64-8956-35a3d70ac15f"),
                columns: new[] { "CreatedByFirebaseUid", "UpdatedByFirebaseUid" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Locations",
                keyColumn: "Id",
                keyValue: new Guid("1d9fbbd6-43b7-41fc-9708-c165e27e89df"),
                columns: new[] { "CreatedByFirebaseUid", "UpdatedByFirebaseUid" },
                values: new object[] { "CHANGE_ME_FIREBASE_UID", "CHANGE_ME_FIREBASE_UID" });

            migrationBuilder.UpdateData(
                table: "ProductCategories",
                keyColumn: "Id",
                keyValue: new Guid("3dc17971-ec2a-48d2-b6b6-d52cf52393aa"),
                columns: new[] { "CreatedByFirebaseUid", "UpdatedByFirebaseUid" },
                values: new object[] { "CHANGE_ME_FIREBASE_UID", "CHANGE_ME_FIREBASE_UID" });

            migrationBuilder.UpdateData(
                table: "ProductCategories",
                keyColumn: "Id",
                keyValue: new Guid("95fcb0ac-4d68-44a7-86aa-58f1e129851f"),
                columns: new[] { "CreatedByFirebaseUid", "UpdatedByFirebaseUid" },
                values: new object[] { "CHANGE_ME_FIREBASE_UID", "CHANGE_ME_FIREBASE_UID" });

            migrationBuilder.UpdateData(
                table: "ProductCategories",
                keyColumn: "Id",
                keyValue: new Guid("a15703c5-f0ab-4d84-9f6d-0549e750dd57"),
                columns: new[] { "CreatedByFirebaseUid", "UpdatedByFirebaseUid" },
                values: new object[] { "CHANGE_ME_FIREBASE_UID", "CHANGE_ME_FIREBASE_UID" });

            migrationBuilder.InsertData(
                table: "UserTenants",
                columns: new[] { "Id", "CreatedAt", "CreatedByFirebaseUid", "CreatedByUserId", "Email", "FirebaseUid", "GlampingId", "Role", "Status", "UpdatedAt", "UpdatedByFirebaseUid", "UpdatedByUserId" },
                values: new object[] { new Guid("4fa97c3d-fcb0-43e5-b44b-9ae72c6e01a4"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, "admin@demo-glamping.local", "CHANGE_ME_FIREBASE_UID", new Guid("8a11e29d-dfba-4e64-8956-35a3d70ac15f"), 1, 2, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("9c7093d5-0365-4f9c-8bfd-9a6c777b9a47"),
                columns: new[] { "CreatedByFirebaseUid", "UpdatedByFirebaseUid" },
                values: new object[] { null, null });

            migrationBuilder.CreateIndex(
                name: "IX_UserTenants_FirebaseUid",
                table: "UserTenants",
                column: "FirebaseUid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTenants_GlampingId",
                table: "UserTenants",
                column: "GlampingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserTenants");

            migrationBuilder.DropColumn(
                name: "CreatedByFirebaseUid",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedByFirebaseUid",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedByFirebaseUid",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "UpdatedByFirebaseUid",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "CreatedByFirebaseUid",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "UpdatedByFirebaseUid",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "CreatedByFirebaseUid",
                table: "StockBalances");

            migrationBuilder.DropColumn(
                name: "UpdatedByFirebaseUid",
                table: "StockBalances");

            migrationBuilder.DropColumn(
                name: "CreatedByFirebaseUid",
                table: "Stays");

            migrationBuilder.DropColumn(
                name: "UpdatedByFirebaseUid",
                table: "Stays");

            migrationBuilder.DropColumn(
                name: "CreatedByFirebaseUid",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "UpdatedByFirebaseUid",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CreatedByFirebaseUid",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UpdatedByFirebaseUid",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CreatedByFirebaseUid",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "UpdatedByFirebaseUid",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "CreatedByFirebaseUid",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "UpdatedByFirebaseUid",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CreatedByFirebaseUid",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "UpdatedByFirebaseUid",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "CreatedByFirebaseUid",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "UpdatedByFirebaseUid",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "CreatedByFirebaseUid",
                table: "Glampings");

            migrationBuilder.DropColumn(
                name: "UpdatedByFirebaseUid",
                table: "Glampings");

            migrationBuilder.DropColumn(
                name: "CreatedByFirebaseUid",
                table: "Folios");

            migrationBuilder.DropColumn(
                name: "UpdatedByFirebaseUid",
                table: "Folios");

            migrationBuilder.DropColumn(
                name: "CreatedByFirebaseUid",
                table: "Charges");

            migrationBuilder.DropColumn(
                name: "UpdatedByFirebaseUid",
                table: "Charges");

            migrationBuilder.DropColumn(
                name: "CreatedByFirebaseUid",
                table: "ChargeItems");

            migrationBuilder.DropColumn(
                name: "UpdatedByFirebaseUid",
                table: "ChargeItems");
        }
    }
}
