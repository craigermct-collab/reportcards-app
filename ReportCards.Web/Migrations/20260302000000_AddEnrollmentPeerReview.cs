using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportCards.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddEnrollmentPeerReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EnrollmentPeerReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EnrollmentId = table.Column<int>(type: "int", nullable: false),
                    TermInstanceId = table.Column<int>(type: "int", nullable: false),
                    ReviewerTeacherId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnrollmentPeerReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnrollmentPeerReviews_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnrollmentPeerReviews_Teachers_ReviewerTeacherId",
                        column: x => x.ReviewerTeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EnrollmentPeerReviews_TermInstances_TermInstanceId",
                        column: x => x.TermInstanceId,
                        principalTable: "TermInstances",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentPeerReviews_EnrollmentId_TermInstanceId",
                table: "EnrollmentPeerReviews",
                columns: new[] { "EnrollmentId", "TermInstanceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentPeerReviews_ReviewerTeacherId",
                table: "EnrollmentPeerReviews",
                column: "ReviewerTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentPeerReviews_TermInstanceId",
                table: "EnrollmentPeerReviews",
                column: "TermInstanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "EnrollmentPeerReviews");
        }
    }
}
