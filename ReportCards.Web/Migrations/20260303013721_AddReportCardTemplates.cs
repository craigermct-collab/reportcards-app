using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportCards.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddReportCardTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReportCardTemplateId",
                table: "TermInstances",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReportCardTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TemplateType = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportCardTemplates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TermInstances_ReportCardTemplateId",
                table: "TermInstances",
                column: "ReportCardTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_TermInstances_ReportCardTemplates_ReportCardTemplateId",
                table: "TermInstances",
                column: "ReportCardTemplateId",
                principalTable: "ReportCardTemplates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TermInstances_ReportCardTemplates_ReportCardTemplateId",
                table: "TermInstances");

            migrationBuilder.DropTable(
                name: "ReportCardTemplates");

            migrationBuilder.DropIndex(
                name: "IX_TermInstances_ReportCardTemplateId",
                table: "TermInstances");

            migrationBuilder.DropColumn(
                name: "ReportCardTemplateId",
                table: "TermInstances");
        }
    }
}
