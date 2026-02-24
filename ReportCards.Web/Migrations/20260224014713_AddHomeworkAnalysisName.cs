using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportCards.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddHomeworkAnalysisName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "HomeworkAnalyses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "HomeworkAnalyses");
        }
    }
}
