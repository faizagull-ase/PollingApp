using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PulsePoll.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTemplateSourceFilename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceFilename",
                table: "Templates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourceFilename",
                table: "Templates",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
