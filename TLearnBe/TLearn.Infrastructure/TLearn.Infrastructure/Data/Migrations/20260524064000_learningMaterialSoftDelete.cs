using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TLearn.Infrastructure.TLearn.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class learningMaterialSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "LearningMaterials",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                table: "LearningMaterials",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "LearningMaterials",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "LearningMaterials");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "LearningMaterials");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "LearningMaterials");
        }
    }
}
