using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportCards.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddReportCardTermSlot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReportCardTermSlot",
                table: "TermInstances",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReportCardTermSlot",
                table: "TermInstances");
        }
    }
}
