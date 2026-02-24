using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportCards.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddHomeworkAnalysisImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HomeworkAnalysisImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    ImageData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HomeworkAnalysisId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeworkAnalysisImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HomeworkAnalysisImages_HomeworkAnalyses_HomeworkAnalysisId",
                        column: x => x.HomeworkAnalysisId,
                        principalTable: "HomeworkAnalyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkAnalysisImages_HomeworkAnalysisId",
                table: "HomeworkAnalysisImages",
                column: "HomeworkAnalysisId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HomeworkAnalysisImages");
        }
    }
}
