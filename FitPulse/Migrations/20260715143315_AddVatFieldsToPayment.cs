using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitPulse.Migrations
{
    /// <inheritdoc />
    public partial class AddVatFieldsToPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AmountExclVat",
                table: "Payments",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VatAmount",
                table: "Payments",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountExclVat",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "VatAmount",
                table: "Payments");
        }
    }
}
