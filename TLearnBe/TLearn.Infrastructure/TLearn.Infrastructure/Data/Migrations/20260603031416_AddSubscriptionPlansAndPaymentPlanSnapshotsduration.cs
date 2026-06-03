using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TLearn.Infrastructure.TLearn.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionPlansAndPaymentPlanSnapshotsduration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DurationDays",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationDays",
                table: "Payments");
        }
    }
}
