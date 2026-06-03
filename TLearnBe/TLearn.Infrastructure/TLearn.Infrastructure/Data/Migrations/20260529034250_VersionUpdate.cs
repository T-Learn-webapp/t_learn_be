using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TLearn.Infrastructure.TLearn.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class VersionUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContributorsJson",
                table: "LearningMaterialVersions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContributorsJson",
                table: "LearningMaterialVersions");
        }
    }
}
