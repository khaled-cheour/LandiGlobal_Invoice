using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LandiGlobalTemplate.Migrations
{
    /// <inheritdoc />
    [Migration("20260526112000_AddProductCatalogItems")]
    public partial class AddProductCatalogItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductCatalogItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FamilyName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Platform = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MainDisplay = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SecondDisplay = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PaymentType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    MemoryPlan = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    G4 = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    GMS = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    HSCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCatalogItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductCatalogItems_ProductNumber",
                table: "ProductCatalogItems",
                column: "ProductNumber",
                unique: true);

            migrationBuilder.InsertData(
                table: "ProductCatalogItems",
                columns: new[]
                {
                    "ProductNumber", "FamilyName", "Platform", "Model", "MainDisplay", "SecondDisplay",
                    "PaymentType", "MemoryPlan", "G4", "GMS", "HSCode", "CreatedAt"
                },
                values: new object[]
                {
                    "WX01000012", "Accessory", "Accessory", "ECRPowerCordUK", "-", "-", "-", "-", "-", "-", null, DateTime.UtcNow
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductCatalogItems");
        }
    }
}
