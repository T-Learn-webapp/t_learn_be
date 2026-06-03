using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TLearn.Infrastructure.TLearn.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionPlansAndPaymentPlanSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_AspNetUsers_UserId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_UserId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Payments_UserId",
                table: "Payments");

            migrationBuilder.AlterColumn<string>(
                name: "PayOSOrderCode",
                table: "Subscriptions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlanName",
                table: "Subscriptions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "RefundReason",
                table: "Payments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentMethod",
                table: "Payments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PayOSPaymentLinkId",
                table: "Payments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Payments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Payments",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiredAt",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastPayOSStatus",
                table: "Payments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlanName",
                table: "Payments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PlanType",
                table: "Payments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RawWebhookJson",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WebhookReceivedCount",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PlanName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DurationDays = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SubscriptionPlans",
                columns: new[] { "Id", "Amount", "CreatedAt", "Currency", "Description", "DurationDays", "IsActive", "PlanName", "PlanType", "SortOrder", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), 49000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "VND", "Nâng cấp tài khoản Vip trong 1 tháng", 30, true, "Vip 1 tháng", "PremiumMonthly", 1, null },
                    { new Guid("22222222-2222-2222-2222-222222222222"), 499000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "VND", "Nâng cấp tài khoản Vip trong 1 năm", 365, true, "Vip 1 năm", "PremiumYearly", 2, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PayOSOrderCode",
                table: "Subscriptions",
                column: "PayOSOrderCode",
                unique: true,
                filter: "[PayOSOrderCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId_IsActive_EndDate",
                table: "Subscriptions",
                columns: new[] { "UserId", "IsActive", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PayOSOrderCode",
                table: "Payments",
                column: "PayOSOrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status_ExpiresAt",
                table: "Payments",
                columns: new[] { "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserId_Status",
                table: "Payments",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_PlanType",
                table: "SubscriptionPlans",
                column: "PlanType",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_AspNetUsers_UserId",
                table: "Payments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_AspNetUsers_UserId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_PayOSOrderCode",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_UserId_IsActive_EndDate",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Payments_PayOSOrderCode",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_Status_ExpiresAt",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_UserId_Status",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PlanName",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ExpiredAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "LastPayOSStatus",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PlanName",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PlanType",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RawWebhookJson",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "WebhookReceivedCount",
                table: "Payments");

            migrationBuilder.AlterColumn<string>(
                name: "PayOSOrderCode",
                table: "Subscriptions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RefundReason",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentMethod",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PayOSPaymentLinkId",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId",
                table: "Subscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserId",
                table: "Payments",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_AspNetUsers_UserId",
                table: "Payments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
