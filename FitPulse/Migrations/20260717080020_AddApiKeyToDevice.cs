using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitPulse.Migrations
{
    /// <inheritdoc />
    public partial class AddApiKeyToDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiKey",
                table: "Devices",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiKey",
                table: "Devices");
        }
    }
}
