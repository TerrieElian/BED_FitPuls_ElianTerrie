using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitPulse.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentNavigationToTrainingSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_SessionId",
                table: "Payments");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SessionId",
                table: "Payments",
                column: "SessionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_SessionId",
                table: "Payments");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SessionId",
                table: "Payments",
                column: "SessionId");
        }
    }
}
