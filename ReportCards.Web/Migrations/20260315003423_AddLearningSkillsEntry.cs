using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportCards.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddLearningSkillsEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LearningSkillsEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnrollmentId = table.Column<int>(type: "int", nullable: false),
                    TermInstanceId = table.Column<int>(type: "int", nullable: false),
                    Responsibility = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Organization = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IndependentWork = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Collaboration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Initiative = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SelfRegulation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrengthsNextSteps = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningSkillsEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningSkillsEntries_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LearningSkillsEntries_TermInstances_TermInstanceId",
                        column: x => x.TermInstanceId,
                        principalTable: "TermInstances",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearningSkillsEntries_EnrollmentId_TermInstanceId",
                table: "LearningSkillsEntries",
                columns: new[] { "EnrollmentId", "TermInstanceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearningSkillsEntries_TermInstanceId",
                table: "LearningSkillsEntries",
                column: "TermInstanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearningSkillsEntries");
        }
    }
}
