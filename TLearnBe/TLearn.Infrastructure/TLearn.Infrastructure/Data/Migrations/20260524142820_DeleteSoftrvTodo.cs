using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TLearn.Infrastructure.TLearn.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DeleteSoftrvTodo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "TodoAssignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                table: "TodoAssignments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "TodoAssignments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SubjectMembers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                table: "SubjectMembers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SubjectMembers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_SubjectMembers_DeletedByUserId",
                table: "SubjectMembers",
                column: "DeletedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubjectMembers_AspNetUsers_DeletedByUserId",
                table: "SubjectMembers",
                column: "DeletedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubjectMembers_AspNetUsers_DeletedByUserId",
                table: "SubjectMembers");

            migrationBuilder.DropIndex(
                name: "IX_SubjectMembers_DeletedByUserId",
                table: "SubjectMembers");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "TodoAssignments");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "TodoAssignments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "TodoAssignments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SubjectMembers");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "SubjectMembers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SubjectMembers");
        }
    }
}
