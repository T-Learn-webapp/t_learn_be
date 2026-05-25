using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TLearn.Infrastructure.TLearn.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FlashCardProgress1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flashcards_AspNetUsers_CreatedByUserId",
                table: "Flashcards");

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcards_AspNetUsers_CreatedByUserId",
                table: "Flashcards",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flashcards_AspNetUsers_CreatedByUserId",
                table: "Flashcards");

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcards_AspNetUsers_CreatedByUserId",
                table: "Flashcards",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
