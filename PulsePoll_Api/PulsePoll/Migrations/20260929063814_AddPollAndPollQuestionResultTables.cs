using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PulsePoll.Migrations
{
    /// <inheritdoc />
    public partial class AddPollAndPollQuestionResultTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Polls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PollCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentQuestionIndex = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Polls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Polls_Templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PollQuestionResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PollId = table.Column<int>(type: "int", nullable: false),
                    QuestionIndex = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Options = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrectOptionIndex = table.Column<int>(type: "int", nullable: true),
                    Tally = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PollQuestionResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PollQuestionResults_Polls_PollId",
                        column: x => x.PollId,
                        principalTable: "Polls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PollQuestionResults_PollId_QuestionIndex",
                table: "PollQuestionResults",
                columns: new[] { "PollId", "QuestionIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Polls_PollCode",
                table: "Polls",
                column: "PollCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Polls_TemplateId",
                table: "Polls",
                column: "TemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PollQuestionResults");

            migrationBuilder.DropTable(
                name: "Polls");
        }
    }
}
