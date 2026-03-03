using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportCards.Web.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCommentTemplateUniqueSourceCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CommentTemplates_SourceCode",
                table: "CommentTemplates");

            migrationBuilder.CreateIndex(
                name: "IX_CommentTemplates_SourceCode",
                table: "CommentTemplates",
                column: "SourceCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CommentTemplates_SourceCode",
                table: "CommentTemplates");

            migrationBuilder.CreateIndex(
                name: "IX_CommentTemplates_SourceCode",
                table: "CommentTemplates",
                column: "SourceCode",
                unique: true,
                filter: "[SourceCode] IS NOT NULL");
        }
    }
}
