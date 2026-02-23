using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportCards.Web.Migrations
{
    /// <inheritdoc />
    public partial class FullSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Classes_ClassId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Students_StudentId",
                table: "Enrollments");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_StudentId_ClassId",
                table: "Enrollments");

            migrationBuilder.RenameColumn(
                name: "ClassId",
                table: "Enrollments",
                newName: "TermInstanceId");

            migrationBuilder.RenameIndex(
                name: "IX_Enrollments_ClassId",
                table: "Enrollments",
                newName: "IX_Enrollments_TermInstanceId");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "InactivatedOn",
                table: "Teachers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InactivatedReason",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Teachers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "InactivatedOn",
                table: "Students",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InactivatedReason",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Students",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ClassGroupInstanceId",
                table: "Enrollments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GradeId",
                table: "Enrollments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ClassGroupTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassGroupTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CurriculumSchemas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RawArtifactData = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumSchemas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GradingScales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValueType = table.Column<int>(type: "int", nullable: false),
                    MinValue = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    MaxValue = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    Step = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    DisplaySuffix = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradingScales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SchoolYears",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolYears", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Grades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    ClassGroupTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Grades_ClassGroupTypes_ClassGroupTypeId",
                        column: x => x.ClassGroupTypeId,
                        principalTable: "ClassGroupTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GradingScaleOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Label = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    GradingScaleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradingScaleOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GradingScaleOptions_GradingScales_GradingScaleId",
                        column: x => x.GradingScaleId,
                        principalTable: "GradingScales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TermInstances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    SchoolYearId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TermInstances_SchoolYears_SchoolYearId",
                        column: x => x.SchoolYearId,
                        principalTable: "SchoolYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YearCurriculums",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppliedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SchoolYearId = table.Column<int>(type: "int", nullable: false),
                    CurriculumSchemaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YearCurriculums", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YearCurriculums_CurriculumSchemas_CurriculumSchemaId",
                        column: x => x.CurriculumSchemaId,
                        principalTable: "CurriculumSchemas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YearCurriculums_SchoolYears_SchoolYearId",
                        column: x => x.SchoolYearId,
                        principalTable: "SchoolYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CurriculumGradeTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurriculumSchemaId = table.Column<int>(type: "int", nullable: false),
                    GradeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumGradeTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurriculumGradeTemplates_CurriculumSchemas_CurriculumSchemaId",
                        column: x => x.CurriculumSchemaId,
                        principalTable: "CurriculumSchemas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CurriculumGradeTemplates_Grades_GradeId",
                        column: x => x.GradeId,
                        principalTable: "Grades",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ClassGroupInstances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TermInstanceId = table.Column<int>(type: "int", nullable: false),
                    ClassGroupTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassGroupInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassGroupInstances_ClassGroupTypes_ClassGroupTypeId",
                        column: x => x.ClassGroupTypeId,
                        principalTable: "ClassGroupTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassGroupInstances_TermInstances_TermInstanceId",
                        column: x => x.TermInstanceId,
                        principalTable: "TermInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportTemplateFieldMaps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportDestinationKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PdfFieldName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TermInstanceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportTemplateFieldMaps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportTemplateFieldMaps_TermInstances_TermInstanceId",
                        column: x => x.TermInstanceId,
                        principalTable: "TermInstances",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TermGradeGradingRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TermInstanceId = table.Column<int>(type: "int", nullable: false),
                    ClassGroupTypeId = table.Column<int>(type: "int", nullable: false),
                    GradeId = table.Column<int>(type: "int", nullable: false),
                    GradingScaleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermGradeGradingRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TermGradeGradingRules_ClassGroupTypes_ClassGroupTypeId",
                        column: x => x.ClassGroupTypeId,
                        principalTable: "ClassGroupTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TermGradeGradingRules_Grades_GradeId",
                        column: x => x.GradeId,
                        principalTable: "Grades",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TermGradeGradingRules_GradingScales_GradingScaleId",
                        column: x => x.GradingScaleId,
                        principalTable: "GradingScales",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TermGradeGradingRules_TermInstances_TermInstanceId",
                        column: x => x.TermInstanceId,
                        principalTable: "TermInstances",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CurriculumClassTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CurriculumGradeTemplateId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumClassTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurriculumClassTemplates_CurriculumGradeTemplates_CurriculumGradeTemplateId",
                        column: x => x.CurriculumGradeTemplateId,
                        principalTable: "CurriculumGradeTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeacherAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    ClassGroupInstanceId = table.Column<int>(type: "int", nullable: false),
                    GradeId = table.Column<int>(type: "int", nullable: true),
                    ClassGroupTypeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherAssignments_ClassGroupInstances_ClassGroupInstanceId",
                        column: x => x.ClassGroupInstanceId,
                        principalTable: "ClassGroupInstances",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeacherAssignments_ClassGroupTypes_ClassGroupTypeId",
                        column: x => x.ClassGroupTypeId,
                        principalTable: "ClassGroupTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeacherAssignments_Grades_GradeId",
                        column: x => x.GradeId,
                        principalTable: "Grades",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeacherAssignments_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CurriculumSubjectTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CurriculumClassTemplateId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumSubjectTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurriculumSubjectTemplates_CurriculumClassTemplates_CurriculumClassTemplateId",
                        column: x => x.CurriculumClassTemplateId,
                        principalTable: "CurriculumClassTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YearClassOfferings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ReportDestinationKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GradingScaleId = table.Column<int>(type: "int", nullable: true),
                    YearCurriculumId = table.Column<int>(type: "int", nullable: false),
                    GradeId = table.Column<int>(type: "int", nullable: false),
                    CurriculumClassTemplateId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YearClassOfferings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YearClassOfferings_CurriculumClassTemplates_CurriculumClassTemplateId",
                        column: x => x.CurriculumClassTemplateId,
                        principalTable: "CurriculumClassTemplates",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_YearClassOfferings_Grades_GradeId",
                        column: x => x.GradeId,
                        principalTable: "Grades",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_YearClassOfferings_GradingScales_GradingScaleId",
                        column: x => x.GradingScaleId,
                        principalTable: "GradingScales",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_YearClassOfferings_YearCurriculums_YearCurriculumId",
                        column: x => x.YearCurriculumId,
                        principalTable: "YearCurriculums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YearSubjectOfferings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ReportDestinationKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GradingScaleId = table.Column<int>(type: "int", nullable: true),
                    YearClassOfferingId = table.Column<int>(type: "int", nullable: false),
                    CurriculumSubjectTemplateId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YearSubjectOfferings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YearSubjectOfferings_CurriculumSubjectTemplates_CurriculumSubjectTemplateId",
                        column: x => x.CurriculumSubjectTemplateId,
                        principalTable: "CurriculumSubjectTemplates",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_YearSubjectOfferings_GradingScales_GradingScaleId",
                        column: x => x.GradingScaleId,
                        principalTable: "GradingScales",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_YearSubjectOfferings_YearClassOfferings_YearClassOfferingId",
                        column: x => x.YearClassOfferingId,
                        principalTable: "YearClassOfferings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StudentLearningItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemType = table.Column<int>(type: "int", nullable: false),
                    EnrollmentId = table.Column<int>(type: "int", nullable: false),
                    YearClassOfferingId = table.Column<int>(type: "int", nullable: true),
                    YearSubjectOfferingId = table.Column<int>(type: "int", nullable: true),
                    GradingScaleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentLearningItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentLearningItems_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentLearningItems_GradingScales_GradingScaleId",
                        column: x => x.GradingScaleId,
                        principalTable: "GradingScales",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentLearningItems_YearClassOfferings_YearClassOfferingId",
                        column: x => x.YearClassOfferingId,
                        principalTable: "YearClassOfferings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentLearningItems_YearSubjectOfferings_YearSubjectOfferingId",
                        column: x => x.YearSubjectOfferingId,
                        principalTable: "YearSubjectOfferings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Assessments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ValueText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueNumber = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    StudentLearningItemId = table.Column<int>(type: "int", nullable: false),
                    TermInstanceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assessments_StudentLearningItems_StudentLearningItemId",
                        column: x => x.StudentLearningItemId,
                        principalTable: "StudentLearningItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assessments_TermInstances_TermInstanceId",
                        column: x => x.TermInstanceId,
                        principalTable: "TermInstances",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_ClassGroupInstanceId",
                table: "Enrollments",
                column: "ClassGroupInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_GradeId",
                table: "Enrollments",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_StudentId",
                table: "Enrollments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_StudentLearningItemId_TermInstanceId",
                table: "Assessments",
                columns: new[] { "StudentLearningItemId", "TermInstanceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_TermInstanceId",
                table: "Assessments",
                column: "TermInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassGroupInstances_ClassGroupTypeId",
                table: "ClassGroupInstances",
                column: "ClassGroupTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassGroupInstances_TermInstanceId",
                table: "ClassGroupInstances",
                column: "TermInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumClassTemplates_CurriculumGradeTemplateId",
                table: "CurriculumClassTemplates",
                column: "CurriculumGradeTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumGradeTemplates_CurriculumSchemaId",
                table: "CurriculumGradeTemplates",
                column: "CurriculumSchemaId");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumGradeTemplates_GradeId",
                table: "CurriculumGradeTemplates",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumSubjectTemplates_CurriculumClassTemplateId",
                table: "CurriculumSubjectTemplates",
                column: "CurriculumClassTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Grades_ClassGroupTypeId_Name",
                table: "Grades",
                columns: new[] { "ClassGroupTypeId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GradingScaleOptions_GradingScaleId_Label",
                table: "GradingScaleOptions",
                columns: new[] { "GradingScaleId", "Label" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportTemplateFieldMaps_TermInstanceId",
                table: "ReportTemplateFieldMaps",
                column: "TermInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentLearningItems_EnrollmentId",
                table: "StudentLearningItems",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentLearningItems_GradingScaleId",
                table: "StudentLearningItems",
                column: "GradingScaleId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentLearningItems_YearClassOfferingId",
                table: "StudentLearningItems",
                column: "YearClassOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentLearningItems_YearSubjectOfferingId",
                table: "StudentLearningItems",
                column: "YearSubjectOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAssignments_ClassGroupInstanceId",
                table: "TeacherAssignments",
                column: "ClassGroupInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAssignments_ClassGroupTypeId",
                table: "TeacherAssignments",
                column: "ClassGroupTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAssignments_GradeId",
                table: "TeacherAssignments",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAssignments_TeacherId",
                table: "TeacherAssignments",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_TermGradeGradingRules_ClassGroupTypeId",
                table: "TermGradeGradingRules",
                column: "ClassGroupTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TermGradeGradingRules_GradeId",
                table: "TermGradeGradingRules",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_TermGradeGradingRules_GradingScaleId",
                table: "TermGradeGradingRules",
                column: "GradingScaleId");

            migrationBuilder.CreateIndex(
                name: "IX_TermGradeGradingRules_TermInstanceId_ClassGroupTypeId_GradeId",
                table: "TermGradeGradingRules",
                columns: new[] { "TermInstanceId", "ClassGroupTypeId", "GradeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TermInstances_SchoolYearId",
                table: "TermInstances",
                column: "SchoolYearId");

            migrationBuilder.CreateIndex(
                name: "IX_YearClassOfferings_CurriculumClassTemplateId",
                table: "YearClassOfferings",
                column: "CurriculumClassTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_YearClassOfferings_GradeId",
                table: "YearClassOfferings",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_YearClassOfferings_GradingScaleId",
                table: "YearClassOfferings",
                column: "GradingScaleId");

            migrationBuilder.CreateIndex(
                name: "IX_YearClassOfferings_YearCurriculumId",
                table: "YearClassOfferings",
                column: "YearCurriculumId");

            migrationBuilder.CreateIndex(
                name: "IX_YearCurriculums_CurriculumSchemaId",
                table: "YearCurriculums",
                column: "CurriculumSchemaId");

            migrationBuilder.CreateIndex(
                name: "IX_YearCurriculums_SchoolYearId",
                table: "YearCurriculums",
                column: "SchoolYearId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_YearSubjectOfferings_CurriculumSubjectTemplateId",
                table: "YearSubjectOfferings",
                column: "CurriculumSubjectTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_YearSubjectOfferings_GradingScaleId",
                table: "YearSubjectOfferings",
                column: "GradingScaleId");

            migrationBuilder.CreateIndex(
                name: "IX_YearSubjectOfferings_YearClassOfferingId",
                table: "YearSubjectOfferings",
                column: "YearClassOfferingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_ClassGroupInstances_ClassGroupInstanceId",
                table: "Enrollments",
                column: "ClassGroupInstanceId",
                principalTable: "ClassGroupInstances",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Grades_GradeId",
                table: "Enrollments",
                column: "GradeId",
                principalTable: "Grades",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Students_StudentId",
                table: "Enrollments",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_TermInstances_TermInstanceId",
                table: "Enrollments",
                column: "TermInstanceId",
                principalTable: "TermInstances",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_ClassGroupInstances_ClassGroupInstanceId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Grades_GradeId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Students_StudentId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_TermInstances_TermInstanceId",
                table: "Enrollments");

            migrationBuilder.DropTable(
                name: "Assessments");

            migrationBuilder.DropTable(
                name: "GradingScaleOptions");

            migrationBuilder.DropTable(
                name: "ReportTemplateFieldMaps");

            migrationBuilder.DropTable(
                name: "TeacherAssignments");

            migrationBuilder.DropTable(
                name: "TermGradeGradingRules");

            migrationBuilder.DropTable(
                name: "StudentLearningItems");

            migrationBuilder.DropTable(
                name: "ClassGroupInstances");

            migrationBuilder.DropTable(
                name: "YearSubjectOfferings");

            migrationBuilder.DropTable(
                name: "TermInstances");

            migrationBuilder.DropTable(
                name: "CurriculumSubjectTemplates");

            migrationBuilder.DropTable(
                name: "YearClassOfferings");

            migrationBuilder.DropTable(
                name: "CurriculumClassTemplates");

            migrationBuilder.DropTable(
                name: "GradingScales");

            migrationBuilder.DropTable(
                name: "YearCurriculums");

            migrationBuilder.DropTable(
                name: "CurriculumGradeTemplates");

            migrationBuilder.DropTable(
                name: "SchoolYears");

            migrationBuilder.DropTable(
                name: "CurriculumSchemas");

            migrationBuilder.DropTable(
                name: "Grades");

            migrationBuilder.DropTable(
                name: "ClassGroupTypes");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_ClassGroupInstanceId",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_GradeId",
                table: "Enrollments");

            migrationBuilder.DropIndex(
                name: "IX_Enrollments_StudentId",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "InactivatedOn",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "InactivatedReason",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "InactivatedOn",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "InactivatedReason",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ClassGroupInstanceId",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "GradeId",
                table: "Enrollments");

            migrationBuilder.RenameColumn(
                name: "TermInstanceId",
                table: "Enrollments",
                newName: "ClassId");

            migrationBuilder.RenameIndex(
                name: "IX_Enrollments_TermInstanceId",
                table: "Enrollments",
                newName: "IX_Enrollments_ClassId");

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SchoolYear = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Classes_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_StudentId_ClassId",
                table: "Enrollments",
                columns: new[] { "StudentId", "ClassId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Classes_TeacherId",
                table: "Classes",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Classes_ClassId",
                table: "Enrollments",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Students_StudentId",
                table: "Enrollments",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
