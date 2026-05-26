using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LandiGlobalTemplate.Migrations
{
    /// <inheritdoc />
    public partial class AddColorAndSizeToProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "InvoiceHistoryProducts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "InvoiceHistoryProducts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "InvoiceHistoryProducts");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "InvoiceHistoryProducts");
        }
    }
}
