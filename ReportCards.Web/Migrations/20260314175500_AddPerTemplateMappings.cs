using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportCards.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPerTemplateMappings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TermInstanceId",
                table: "ReportTemplateFieldMaps",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ReportCardTemplateId",
                table: "ReportTemplateFieldMaps",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportTemplateFieldMaps_ReportCardTemplateId",
                table: "ReportTemplateFieldMaps",
                column: "ReportCardTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportTemplateFieldMaps_ReportCardTemplates_ReportCardTemplateId",
                table: "ReportTemplateFieldMaps",
                column: "ReportCardTemplateId",
                principalTable: "ReportCardTemplates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportTemplateFieldMaps_ReportCardTemplates_ReportCardTemplateId",
                table: "ReportTemplateFieldMaps");

            migrationBuilder.DropIndex(
                name: "IX_ReportTemplateFieldMaps_ReportCardTemplateId",
                table: "ReportTemplateFieldMaps");

            migrationBuilder.DropColumn(
                name: "ReportCardTemplateId",
                table: "ReportTemplateFieldMaps");

            migrationBuilder.AlterColumn<int>(
                name: "TermInstanceId",
                table: "ReportTemplateFieldMaps",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
