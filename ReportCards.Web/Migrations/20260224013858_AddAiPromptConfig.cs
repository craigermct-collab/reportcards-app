using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportCards.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAiPromptConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SchoolAiConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GradingPhilosophy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToneGuidance = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TerminologyNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpellingGuidance = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GrammarGuidance = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RubricGuidance = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AiDetectionGuidance = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalInstructions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolAiConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeacherAiConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    PreferredTone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FocusAreas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalInstructions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherAiConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherAiConfigs_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAiConfigs_TeacherId",
                table: "TeacherAiConfigs",
                column: "TeacherId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchoolAiConfigs");

            migrationBuilder.DropTable(
                name: "TeacherAiConfigs");
        }
    }
}
