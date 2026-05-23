using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TLearn.Infrastructure.TLearn.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class todolistUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoItems_AspNetUsers_CreatedById",
                table: "TodoItems");

            migrationBuilder.DropIndex(
                name: "IX_TodoItems_CreatedById",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "TodoItems");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_CreatedByUserId",
                table: "TodoItems",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoItems_AspNetUsers_CreatedByUserId",
                table: "TodoItems",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoItems_AspNetUsers_CreatedByUserId",
                table: "TodoItems");

            migrationBuilder.DropIndex(
                name: "IX_TodoItems_CreatedByUserId",
                table: "TodoItems");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "TodoItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_CreatedById",
                table: "TodoItems",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoItems_AspNetUsers_CreatedById",
                table: "TodoItems",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
