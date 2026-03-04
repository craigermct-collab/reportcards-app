using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportCards.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAiPromptModeColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubjectModifierTemplates_Name",
                table: "SubjectModifierTemplates");

            migrationBuilder.AddColumn<string>(
                name: "CommentWriterPrompt",
                table: "TeacherAiConfigs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GeneralAssistantPrompt",
                table: "TeacherAiConfigs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SubjectModifierTemplates",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<bool>(
                name: "IsSystem",
                table: "SubjectModifierTemplates",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "EnabledStrandIdsJson",
                table: "StudentSubjectModifiers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "[]");

            migrationBuilder.AlterColumn<string>(
                name: "EnabledOptionsJson",
                table: "StudentSubjectModifiers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "CommentWriterPrompt",
                table: "SchoolAiConfigs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GeneralAssistantPrompt",
                table: "SchoolAiConfigs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ActiveStrandIdsJson",
                table: "ClassGroupSubjectConfigs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommentWriterPrompt",
                table: "TeacherAiConfigs");

            migrationBuilder.DropColumn(
                name: "GeneralAssistantPrompt",
                table: "TeacherAiConfigs");

            migrationBuilder.DropColumn(
                name: "CommentWriterPrompt",
                table: "SchoolAiConfigs");

            migrationBuilder.DropColumn(
                name: "GeneralAssistantPrompt",
                table: "SchoolAiConfigs");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SubjectModifierTemplates",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsSystem",
                table: "SubjectModifierTemplates",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "EnabledStrandIdsJson",
                table: "StudentSubjectModifiers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "EnabledOptionsJson",
                table: "StudentSubjectModifiers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ActiveStrandIdsJson",
                table: "ClassGroupSubjectConfigs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectModifierTemplates_Name",
                table: "SubjectModifierTemplates",
                column: "Name",
                unique: true);
        }
    }
}
