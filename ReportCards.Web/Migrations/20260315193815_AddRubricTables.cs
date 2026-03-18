using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportCards.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddRubricTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RubricTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ClassGroupTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RubricTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RubricTemplates_ClassGroupTypes_ClassGroupTypeId",
                        column: x => x.ClassGroupTypeId,
                        principalTable: "ClassGroupTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RubricQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Segment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubjectScope = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    RubricTemplateId = table.Column<int>(type: "int", nullable: false),
                    GradingScaleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RubricQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RubricQuestions_GradingScales_GradingScaleId",
                        column: x => x.GradingScaleId,
                        principalTable: "GradingScales",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RubricQuestions_RubricTemplates_RubricTemplateId",
                        column: x => x.RubricTemplateId,
                        principalTable: "RubricTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentRubricResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResponseValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EnrollmentId = table.Column<int>(type: "int", nullable: false),
                    TermInstanceId = table.Column<int>(type: "int", nullable: false),
                    RubricQuestionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentRubricResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentRubricResponses_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentRubricResponses_RubricQuestions_RubricQuestionId",
                        column: x => x.RubricQuestionId,
                        principalTable: "RubricQuestions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentRubricResponses_TermInstances_TermInstanceId",
                        column: x => x.TermInstanceId,
                        principalTable: "TermInstances",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RubricQuestions_GradingScaleId",
                table: "RubricQuestions",
                column: "GradingScaleId");

            migrationBuilder.CreateIndex(
                name: "IX_RubricQuestions_RubricTemplateId",
                table: "RubricQuestions",
                column: "RubricTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_RubricTemplates_ClassGroupTypeId",
                table: "RubricTemplates",
                column: "ClassGroupTypeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentRubricResponses_EnrollmentId_RubricQuestionId_TermInstanceId",
                table: "StudentRubricResponses",
                columns: new[] { "EnrollmentId", "RubricQuestionId", "TermInstanceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentRubricResponses_RubricQuestionId",
                table: "StudentRubricResponses",
                column: "RubricQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRubricResponses_TermInstanceId",
                table: "StudentRubricResponses",
                column: "TermInstanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentRubricResponses");

            migrationBuilder.DropTable(
                name: "RubricQuestions");

            migrationBuilder.DropTable(
                name: "RubricTemplates");
        }
    }
}
