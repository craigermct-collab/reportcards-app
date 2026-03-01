using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportCards.Web.Migrations
{
    /// <inheritdoc />
    public partial class CurriculumAndReportCardFormats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReportCardFormatCode",
                table: "TermInstances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurriculumSchemaId",
                table: "SchoolYears",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "GradedAtStrandLevel",
                table: "CurriculumClassTemplates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ClassGroupReportFormats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FormatFamily = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SchoolYearId = table.Column<int>(type: "int", nullable: false),
                    ClassGroupTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassGroupReportFormats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassGroupReportFormats_ClassGroupTypes_ClassGroupTypeId",
                        column: x => x.ClassGroupTypeId,
                        principalTable: "ClassGroupTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ClassGroupReportFormats_SchoolYears_SchoolYearId",
                        column: x => x.SchoolYearId,
                        principalTable: "SchoolYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CurriculumSubStrands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CurriculumSubjectTemplateId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumSubStrands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurriculumSubStrands_CurriculumSubjectTemplates_CurriculumSubjectTemplateId",
                        column: x => x.CurriculumSubjectTemplateId,
                        principalTable: "CurriculumSubjectTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SchoolYears_CurriculumSchemaId",
                table: "SchoolYears",
                column: "CurriculumSchemaId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassGroupReportFormats_ClassGroupTypeId",
                table: "ClassGroupReportFormats",
                column: "ClassGroupTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassGroupReportFormats_SchoolYearId_ClassGroupTypeId",
                table: "ClassGroupReportFormats",
                columns: new[] { "SchoolYearId", "ClassGroupTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumSubStrands_CurriculumSubjectTemplateId",
                table: "CurriculumSubStrands",
                column: "CurriculumSubjectTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_SchoolYears_CurriculumSchemas_CurriculumSchemaId",
                table: "SchoolYears",
                column: "CurriculumSchemaId",
                principalTable: "CurriculumSchemas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SchoolYears_CurriculumSchemas_CurriculumSchemaId",
                table: "SchoolYears");

            migrationBuilder.DropTable(
                name: "ClassGroupReportFormats");

            migrationBuilder.DropTable(
                name: "CurriculumSubStrands");

            migrationBuilder.DropIndex(
                name: "IX_SchoolYears_CurriculumSchemaId",
                table: "SchoolYears");

            migrationBuilder.DropColumn(
                name: "ReportCardFormatCode",
                table: "TermInstances");

            migrationBuilder.DropColumn(
                name: "CurriculumSchemaId",
                table: "SchoolYears");

            migrationBuilder.DropColumn(
                name: "GradedAtStrandLevel",
                table: "CurriculumClassTemplates");
        }
    }
}
