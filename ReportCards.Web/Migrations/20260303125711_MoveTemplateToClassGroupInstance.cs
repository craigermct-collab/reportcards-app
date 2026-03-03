using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportCards.Web.Migrations
{
    /// <inheritdoc />
    public partial class MoveTemplateToClassGroupInstance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TermInstances_ReportCardTemplates_ReportCardTemplateId",
                table: "TermInstances");

            migrationBuilder.DropIndex(
                name: "IX_TermInstances_ReportCardTemplateId",
                table: "TermInstances");

            migrationBuilder.DropColumn(
                name: "ReportCardTemplateId",
                table: "TermInstances");

            migrationBuilder.AddColumn<int>(
                name: "ReportCardTemplateId",
                table: "ClassGroupInstances",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassGroupInstances_ReportCardTemplateId",
                table: "ClassGroupInstances",
                column: "ReportCardTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassGroupInstances_ReportCardTemplates_ReportCardTemplateId",
                table: "ClassGroupInstances",
                column: "ReportCardTemplateId",
                principalTable: "ReportCardTemplates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassGroupInstances_ReportCardTemplates_ReportCardTemplateId",
                table: "ClassGroupInstances");

            migrationBuilder.DropIndex(
                name: "IX_ClassGroupInstances_ReportCardTemplateId",
                table: "ClassGroupInstances");

            migrationBuilder.DropColumn(
                name: "ReportCardTemplateId",
                table: "ClassGroupInstances");

            migrationBuilder.AddColumn<int>(
                name: "ReportCardTemplateId",
                table: "TermInstances",
                type: "int",
                nullable: true);

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
    }
}
