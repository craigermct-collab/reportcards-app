using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportCards.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add Pronouns column to Students (default 2 = TheyThem)
            migrationBuilder.AddColumn<int>(
                name: "Pronouns",
                table: "Students",
                type: "int",
                nullable: false,
                defaultValue: 2);

            // Create CommentTemplates table
            migrationBuilder.CreateTable(
                name: "CommentTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GradeLabel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SourceCode = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentTemplates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommentTemplates_SourceCode",
                table: "CommentTemplates",
                column: "SourceCode",
                unique: true,
                filter: "[SourceCode] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CommentTemplates");

            migrationBuilder.DropColumn(
                name: "Pronouns",
                table: "Students");
        }
    }
}
