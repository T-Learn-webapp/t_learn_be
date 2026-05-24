using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TLearn.Infrastructure.TLearn.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MaterialVersio132n : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearningMaterialVersions_AspNetUsers_EditedByUserId",
                table: "LearningMaterialVersions");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningMaterialVersions_LearningMaterials_LearningMaterialId",
                table: "LearningMaterialVersions");

            migrationBuilder.DropIndex(
                name: "IX_LearningMaterialVersions_LearningMaterialId",
                table: "LearningMaterialVersions");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "LearningMaterials",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_LearningMaterialVersions_LearningMaterialId_VersionNumber",
                table: "LearningMaterialVersions",
                columns: new[] { "LearningMaterialId", "VersionNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningMaterialVersions_AspNetUsers_EditedByUserId",
                table: "LearningMaterialVersions",
                column: "EditedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningMaterialVersions_LearningMaterials_LearningMaterialId",
                table: "LearningMaterialVersions",
                column: "LearningMaterialId",
                principalTable: "LearningMaterials",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearningMaterialVersions_AspNetUsers_EditedByUserId",
                table: "LearningMaterialVersions");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningMaterialVersions_LearningMaterials_LearningMaterialId",
                table: "LearningMaterialVersions");

            migrationBuilder.DropIndex(
                name: "IX_LearningMaterialVersions_LearningMaterialId_VersionNumber",
                table: "LearningMaterialVersions");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "LearningMaterials",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);

            migrationBuilder.CreateIndex(
                name: "IX_LearningMaterialVersions_LearningMaterialId",
                table: "LearningMaterialVersions",
                column: "LearningMaterialId");

            migrationBuilder.AddForeignKey(
                name: "FK_LearningMaterialVersions_AspNetUsers_EditedByUserId",
                table: "LearningMaterialVersions",
                column: "EditedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningMaterialVersions_LearningMaterials_LearningMaterialId",
                table: "LearningMaterialVersions",
                column: "LearningMaterialId",
                principalTable: "LearningMaterials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
