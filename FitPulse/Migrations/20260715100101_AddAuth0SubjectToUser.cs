using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitPulse.Migrations
{
    /// <inheritdoc />
    public partial class AddAuth0SubjectToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Users",
                newName: "Auth0Subject");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Auth0Subject",
                table: "Users",
                newName: "PasswordHash");
        }
    }
}
