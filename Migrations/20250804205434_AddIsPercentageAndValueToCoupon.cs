using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace buytoy.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPercentageAndValueToCoupon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPercentage",
                table: "Coupons",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Value",
                table: "Coupons",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPercentage",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "Coupons");
        }
    }
}
