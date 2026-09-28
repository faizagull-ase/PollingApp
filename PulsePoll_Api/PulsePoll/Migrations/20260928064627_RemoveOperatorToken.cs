using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PulsePoll.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOperatorToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Templates_OperatorToken",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "OperatorToken",
                table: "Templates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OperatorToken",
                table: "Templates",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_OperatorToken",
                table: "Templates",
                column: "OperatorToken");
        }
    }
}
