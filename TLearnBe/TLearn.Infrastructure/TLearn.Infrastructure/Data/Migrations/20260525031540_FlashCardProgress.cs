using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TLearn.Infrastructure.TLearn.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FlashCardProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EaseFactor",
                table: "Flashcards");

            migrationBuilder.DropColumn(
                name: "Interval",
                table: "Flashcards");

            migrationBuilder.DropColumn(
                name: "RepetitionCount",
                table: "Flashcards");

            migrationBuilder.RenameColumn(
                name: "NextReviewDate",
                table: "Flashcards",
                newName: "DeletedAt");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Flashcards",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                table: "Flashcards",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Flashcards",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "UserFlashcardProgresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FlashcardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EaseFactor = table.Column<double>(type: "float", nullable: false),
                    Interval = table.Column<int>(type: "int", nullable: false),
                    RepetitionCount = table.Column<int>(type: "int", nullable: false),
                    NextReviewDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastQuality = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFlashcardProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFlashcardProgresses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserFlashcardProgresses_Flashcards_FlashcardId",
                        column: x => x.FlashcardId,
                        principalTable: "Flashcards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Flashcards_CreatedByUserId",
                table: "Flashcards",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFlashcardProgresses_FlashcardId_UserId",
                table: "UserFlashcardProgresses",
                columns: new[] { "FlashcardId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserFlashcardProgresses_UserId",
                table: "UserFlashcardProgresses",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcards_AspNetUsers_CreatedByUserId",
                table: "Flashcards",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flashcards_AspNetUsers_CreatedByUserId",
                table: "Flashcards");

            migrationBuilder.DropTable(
                name: "UserFlashcardProgresses");

            migrationBuilder.DropIndex(
                name: "IX_Flashcards_CreatedByUserId",
                table: "Flashcards");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Flashcards");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Flashcards");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Flashcards");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "Flashcards",
                newName: "NextReviewDate");

            migrationBuilder.AddColumn<double>(
                name: "EaseFactor",
                table: "Flashcards",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "Interval",
                table: "Flashcards",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RepetitionCount",
                table: "Flashcards",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
