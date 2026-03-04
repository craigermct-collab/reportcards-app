using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportCards.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectModifiersAndStrandConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── SubjectModifierTemplates ────────────────────────────
            migrationBuilder.CreateTable(
                name: "SubjectModifierTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OptionsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectModifierTemplates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubjectModifierTemplates_Name",
                table: "SubjectModifierTemplates",
                column: "Name",
                unique: true);

            // ── ClassGroupSubjectConfigs ────────────────────────────
            migrationBuilder.CreateTable(
                name: "ClassGroupSubjectConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassGroupInstanceId = table.Column<int>(type: "int", nullable: false),
                    CurriculumClassTemplateId = table.Column<int>(type: "int", nullable: false),
                    SubjectModifierTemplateId = table.Column<int>(type: "int", nullable: true),
                    ActiveStrandIdsJson = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "[]"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassGroupSubjectConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassGroupSubjectConfigs_ClassGroupInstances_ClassGroupInstanceId",
                        column: x => x.ClassGroupInstanceId,
                        principalTable: "ClassGroupInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassGroupSubjectConfigs_CurriculumClassTemplates_CurriculumClassTemplateId",
                        column: x => x.CurriculumClassTemplateId,
                        principalTable: "CurriculumClassTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_ClassGroupSubjectConfigs_SubjectModifierTemplates_SubjectModifierTemplateId",
                        column: x => x.SubjectModifierTemplateId,
                        principalTable: "SubjectModifierTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassGroupSubjectConfigs_ClassGroupInstanceId_CurriculumClassTemplateId",
                table: "ClassGroupSubjectConfigs",
                columns: new[] { "ClassGroupInstanceId", "CurriculumClassTemplateId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassGroupSubjectConfigs_CurriculumClassTemplateId",
                table: "ClassGroupSubjectConfigs",
                column: "CurriculumClassTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassGroupSubjectConfigs_SubjectModifierTemplateId",
                table: "ClassGroupSubjectConfigs",
                column: "SubjectModifierTemplateId");

            // ── StudentSubjectModifiers ─────────────────────────────
            migrationBuilder.CreateTable(
                name: "StudentSubjectModifiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnrollmentId = table.Column<int>(type: "int", nullable: false),
                    CurriculumClassTemplateId = table.Column<int>(type: "int", nullable: false),
                    TermInstanceId = table.Column<int>(type: "int", nullable: false),
                    EnabledOptionsJson = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "[]"),
                    EnabledStrandIdsJson = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "[]"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentSubjectModifiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentSubjectModifiers_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentSubjectModifiers_CurriculumClassTemplates_CurriculumClassTemplateId",
                        column: x => x.CurriculumClassTemplateId,
                        principalTable: "CurriculumClassTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_StudentSubjectModifiers_TermInstances_TermInstanceId",
                        column: x => x.TermInstanceId,
                        principalTable: "TermInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentSubjectModifiers_EnrollmentId_CurriculumClassTemplateId_TermInstanceId",
                table: "StudentSubjectModifiers",
                columns: new[] { "EnrollmentId", "CurriculumClassTemplateId", "TermInstanceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentSubjectModifiers_CurriculumClassTemplateId",
                table: "StudentSubjectModifiers",
                column: "CurriculumClassTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSubjectModifiers_TermInstanceId",
                table: "StudentSubjectModifiers",
                column: "TermInstanceId");

            // ── Seed the two system modifier templates ──────────────
            migrationBuilder.InsertData(
                table: "SubjectModifierTemplates",
                columns: new[] { "Name", "OptionsJson", "IsSystem", "CreatedAt" },
                columnTypes: new[] { "nvarchar(200)", "nvarchar(max)", "bit", "datetimeoffset" },
                values: new object[] { "Standard (3 options)", "[\"ESL/ELD\",\"IEP\",\"French\"]", true, DateTimeOffset.UtcNow });

            migrationBuilder.InsertData(
                table: "SubjectModifierTemplates",
                columns: new[] { "Name", "OptionsJson", "IsSystem", "CreatedAt" },
                columnTypes: new[] { "nvarchar(200)", "nvarchar(max)", "bit", "datetimeoffset" },
                values: new object[] { "Extended (4 options)", "[\"ESL/ELD\",\"IEP\",\"French\",\"N/A\"]", true, DateTimeOffset.UtcNow });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "StudentSubjectModifiers");
            migrationBuilder.DropTable(name: "ClassGroupSubjectConfigs");
            migrationBuilder.DropTable(name: "SubjectModifierTemplates");
        }
    }
}
