using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TLearn.Infrastructure.TLearn.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Invite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_AspNetUsers_UserId",
                table: "Subjects");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Subjects",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "Subjects",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SubjectInvitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Permission = table.Column<int>(type: "int", nullable: false),
                    InviteToken = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InvitedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    LastSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcceptedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubjectInvitations_AspNetUsers_InvitedBy",
                        column: x => x.InvitedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubjectInvitations_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubjectMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Permission = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    InvitedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastViewedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubjectMembers_AspNetUsers_InvitedBy",
                        column: x => x.InvitedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubjectMembers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubjectMembers_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_UserId1",
                table: "Subjects",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectInvitations_Email",
                table: "SubjectInvitations",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectInvitations_ExpiresAt",
                table: "SubjectInvitations",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectInvitations_InvitedBy",
                table: "SubjectInvitations",
                column: "InvitedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectInvitations_InviteToken",
                table: "SubjectInvitations",
                column: "InviteToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubjectInvitations_SubjectId",
                table: "SubjectInvitations",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectMembers_InvitedBy",
                table: "SubjectMembers",
                column: "InvitedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectMembers_SubjectId_UserId",
                table: "SubjectMembers",
                columns: new[] { "SubjectId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubjectMembers_UserId",
                table: "SubjectMembers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_AspNetUsers_UserId",
                table: "Subjects",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_AspNetUsers_UserId1",
                table: "Subjects",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_AspNetUsers_UserId",
                table: "Subjects");

            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_AspNetUsers_UserId1",
                table: "Subjects");

            migrationBuilder.DropTable(
                name: "SubjectInvitations");

            migrationBuilder.DropTable(
                name: "SubjectMembers");

            migrationBuilder.DropIndex(
                name: "IX_Subjects_UserId1",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Subjects");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Subjects",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_AspNetUsers_UserId",
                table: "Subjects",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
