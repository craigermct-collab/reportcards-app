using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportCards.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddHomeworkAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HomeworkAnalyses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnalyzedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    GradeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClassGroupName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CheckedSpelling = table.Column<bool>(type: "bit", nullable: false),
                    CheckedGrammar = table.Column<bool>(type: "bit", nullable: false),
                    CheckedRubric = table.Column<bool>(type: "bit", nullable: false),
                    CheckedAiGenerated = table.Column<bool>(type: "bit", nullable: false),
                    SpellingScore = table.Column<int>(type: "int", nullable: true),
                    SpellingSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GrammarScore = table.Column<int>(type: "int", nullable: true),
                    GrammarSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RubricScore = table.Column<int>(type: "int", nullable: true),
                    RubricSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AiLikelihood = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AiDetectionSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OverallSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TeacherNote = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeworkAnalyses", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HomeworkAnalyses");
        }
    }
}
